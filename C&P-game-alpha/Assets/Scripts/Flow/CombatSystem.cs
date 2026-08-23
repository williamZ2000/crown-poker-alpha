using System.Collections.Generic;
using CnP.Core;
using CnP.Domain.Combat;
using CnP.Domain.Unit;
using UnityEngine;

namespace CnP.Flow
{
    /// <summary>
    /// 战斗引擎（§7.3 自走棋式全自动）：
    /// 最近索敌 → 射程外移动（钳制己方半场）→ 射程内按攻速攻击 → LoL 百分比减免结算。
    /// 结束条件（任一触发）：一方全灭 / 180s 绝对时限 / 10s 静默（时限与静默按存活数判定，平局判玩家胜）。
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        public static CombatSystem Instance { get; private set; }

        RoundFlowController _flow;
        readonly Dictionary<int, float> _attackCooldowns = new Dictionary<int, float>();
        float _battleTime;
        float _lastDamageTime;
        bool _running;

        /// <summary>战斗计时（结算面板显示用）</summary>
        public float BattleTime => _battleTime;

        void Awake()
        {
            Instance = this;
        }

        /// <summary>开战（RoundFlowController.RequestStartBattle 调用）</summary>
        public void Begin()
        {
            _flow = RoundFlowController.Instance;
            _attackCooldowns.Clear();
            _battleTime = 0f;
            _lastDamageTime = 0f;
            _running = true;

            // 三带二小增益落地（#D18）：本场战斗全体己方护甲 +2（公式 v2 固定列）
            if (_flow.FullHouseBuffPending)
            {
                foreach (var u in _flow.Board.Units)
                    if (u.Side == Side.Player) u.BonusDefP += GameParams.FullHouseArmorBonus;
                _flow.FullHouseBuffPending = false;
            }

            FlowEvents.RaiseToast("战斗开始！");
        }

        void Update()
        {
            if (!_running) return;
            if (_flow == null || _flow.Phase != Phase.Battle)
            {
                _running = false; // 中途被重置（回标题等）
                return;
            }

            float dt = Time.deltaTime;
            _battleTime += dt;
            TickCombat(dt);
            CheckEndConditions();
        }

        // ── 战斗主循环 ─────────────────────────────────

        void TickCombat(float dt)
        {
            var units = _flow.Board.Units;
            foreach (var u in units)
            {
                if (!u.Alive) continue;

                var target = FindNearestEnemy(u, units);
                if (target == null) continue; // 无敌人可打（同帧全灭由结束条件兜底）

                bool noAttack = u.Stats.AtkP <= 0f && u.Stats.AtkM <= 0f;
                if (noAttack) continue; // 无攻击单位原地待命（辅助效果池后置）

                float range = RangeToWorld(u.Stats.Range);
                float dist = Vector2.Distance(u.Position, target.Position);

                if (dist <= range)
                {
                    // 射程内：按攻速攻击（首次接敌立即出手）
                    if (!_attackCooldowns.TryGetValue(u.Id, out var cd)) cd = 0f;
                    cd -= dt;
                    if (cd <= 0f)
                    {
                        Attack(u, target);
                        cd = 1f / Mathf.Max(0.01f, u.Stats.Spd);
                    }
                    _attackCooldowns[u.Id] = cd;
                }
                else
                {
                    // 射程外：向最近敌人移动。战斗移动允许越中线（近战要能追到远程），仅钳制棋盘外边界
                    var dir = ((Vector2)target.Position - u.Position).normalized;
                    var next = u.Position + dir * (u.Stats.Move * dt);
                    u.Position = BoardGeometry.ClampToBoard(next);
                }
            }
        }

        void Attack(UnitInstance attacker, UnitInstance target)
        {
            float dmg;
            switch (attacker.Stats.Attack)
            {
                case AttackType.物理:
                    dmg = DamageCalculator.PhysicalDamage(attacker.Stats.AtkP, target.CurrentDefP);
                    break;
                case AttackType.法术:
                    dmg = DamageCalculator.MagicDamage(attacker.Stats.AtkM, target.Stats.DefM);
                    break;
                default:
                    return;
            }

            target.TakeDamage(dmg);
            _lastDamageTime = _battleTime;
            if (!target.Alive) FlowEvents.RaiseBoardChanged(); // BoardView 同步移除阵亡视图
        }

        static UnitInstance FindNearestEnemy(UnitInstance self, IReadOnlyList<UnitInstance> units)
        {
            UnitInstance best = null;
            float bestSq = float.MaxValue;
            foreach (var u in units)
            {
                if (!u.Alive || u.Side == self.Side) continue;
                float sq = (u.Position - self.Position).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = u; }
            }
            return best;
        }

        // ── 结束条件（§7.3）────────────────────────────

        void CheckEndConditions()
        {
            var units = _flow.Board.Units;
            bool playerAlive = false, enemyAlive = false;
            int playerCount = 0, enemyCount = 0;
            foreach (var u in units)
            {
                if (!u.Alive) continue;
                if (u.Side == Side.Player) { playerAlive = true; playerCount++; }
                else { enemyAlive = true; enemyCount++; }
            }

            // 一方全灭（同时全灭判玩家负）
            if (!enemyAlive) { EndBattle(true); return; }
            if (!playerAlive) { EndBattle(false); return; }

            // 绝对时限 180s / 静默 10s：按存活数判定，平局判玩家胜（原型友好向，可调）
            if (_battleTime >= GameParams.BattleTimeLimit ||
                _battleTime - _lastDamageTime >= GameParams.BattleSilenceLimit)
            {
                EndBattle(playerCount >= enemyCount);
            }
        }

        void EndBattle(bool playerWin)
        {
            _running = false;
            _flow.OnBattleEnded(playerWin);
        }

        /// <summary>射程档 → 世界单位（手感参数，平衡期可调）。
        /// 近战必须 > 两侧钳制的最小间隔 2×(MidGap+单位半径)=1.9，否则近战永远无法接敌（ISSUE 见 issue-log）</summary>
        public static float RangeToWorld(int range)
        {
            switch (range)
            {
                case 1: return 2.2f;  // 近战
                case 2: return 4.2f;  // 中程
                default: return 6.5f; // 远程
            }
        }
    }
}
