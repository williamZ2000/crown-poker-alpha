using System.Collections.Generic;
using System.Linq;
using CnP.Core;
using CnP.Domain.Card;
using CnP.Domain.Unit;
using UnityEngine;

namespace CnP.Flow
{
    /// <summary>
    /// 回合流控制器（四阶段状态机：抽牌并入启动自动执行 → 出牌 → 战斗 → 结算）。
    /// 本切片 = 关卡 1-1 单回合；商店/经济/多关循环后置。
    /// </summary>
    public class RoundFlowController : MonoBehaviour
    {
        public static RoundFlowController Instance { get; private set; }

        public CardSystem Cards { get; } = new CardSystem();
        public BoardSystem Board { get; } = new BoardSystem();

        public Phase Phase { get; private set; } = Phase.Title;

        /// <summary>已用出牌轮次（基础 4 轮，#D37）</summary>
        public int RoundsUsed { get; private set; }
        public int RoundsLeft => GameParams.PlayRoundsBase - RoundsUsed;

        /// <summary>三带二小增益待生效（本场战斗全体护甲 +2，#D18；战斗引擎开局应用后置回 false）</summary>
        public bool FullHouseBuffPending { get; set; }

        void Awake()
        {
            Instance = this;
        }

        /// <summary>开始新一回合（关卡 1-1）：重置全部状态 → 抽起手 13 张 → 预算生成敌军 → 出牌阶段</summary>
        public void StartNewRound()
        {
            Board.Clear();
            Cards.StartNewRound();
            RoundsUsed = 0;
            FullHouseBuffPending = false;

            // 敌军预算生成（#D34：B = B₀×E×回合系数；关卡 1 回合 1 = 250 CP）
            // 出牌阶段即可见，玩家可针对性布阵（2026-08-24 方案约定，可改到开战时生成）
            var army = new Domain.Enemy.EnemyArmyGenerator().Generate(level: 1, round: 1);
            Board.SpawnEnemyArmy(army);
            EnemyArmyCp = army.Sum(u => Domain.Combat.CombatPower.Cp(u));

            SetPhase(Phase.Play);
            FlowEvents.RaiseToast("关卡 1-1 · 天灵军 " + army.Count + " 个单位来袭（战力 " + EnemyArmyCp.ToString("0") + "）—— 出牌布阵，随时开战");
        }

        /// <summary>本回合敌军总战力（生成时记录，供显示/校验）</summary>
        public float EnemyArmyCp { get; private set; }

        /// <summary>出牌：判定 → 校验部署上限 → 召唤（消耗 1 轮）</summary>
        public bool TryPlayCurrentPattern()
        {
            if (Phase != Phase.Play) return false;
            var pattern = Cards.CurrentPattern;
            if (pattern == null)
            {
                FlowEvents.RaiseToast("牌型无效：选中牌须恰好构成一个牌型");
                return false;
            }
            if (RoundsLeft <= 0)
            {
                FlowEvents.RaiseToast("出牌轮次已用完（基础 4 轮）");
                return false;
            }

            var batches = UnitFactory.CreateSummons(pattern);
            int totalCount = 0;
            foreach (var b in batches) totalCount += b.Count;

            if (!Board.CanDeploy(totalCount))
            {
                FlowEvents.RaiseToast("棋盘已满（36 上限），支援区后置版本暂不可超额部署");
                return false;
            }

            if (!Cards.PlaySelected(out var played)) return false;

            Board.SpawnPlayerUnits(batches);
            if (played.Kind == PatternKind.FullHouse) FullHouseBuffPending = true;

            RoundsUsed++;

            // ISSUE-005 修复：成功出牌此前无任何播报（炸弹 4 张换 1 个单位尤其无感），
            // 播报牌型与召唤明细，让"打出去"有明确反馈
            var desc = new System.Text.StringBuilder(played.DisplayName).Append("：召唤 ");
            for (int i = 0; i < batches.Count; i++)
            {
                if (i > 0) desc.Append(" + ");
                desc.Append(batches[i].Template.Name).Append(" ×").Append(batches[i].Count);
            }
            if (played.Kind == PatternKind.FullHouse) desc.Append("（全体护甲 +2，开战生效）");
            FlowEvents.RaisePatternPlayed(played, totalCount);
            FlowEvents.RaiseToast(desc.ToString());
            return true;
        }

        /// <summary>换牌（#D39 定稿：与出牌按钮同构——作用于当前选中牌，恰好 1 张；
        /// 出牌阶段内随时，每回合 1 次 × 1 张）</summary>
        public bool SwapSelected()
        {
            if (Phase != Phase.Play) return false;
            if (Cards.SwapsLeft <= 0)
            {
                FlowEvents.RaiseToast("本回合换牌次数已用完");
                return false;
            }
            if (Cards.Selection.Count != 1)
            {
                FlowEvents.RaiseToast(Cards.Selection.Count == 0
                    ? "先选中要换掉的牌，再点「换牌」"
                    : "换牌一次只换 1 张（当前选中 " + Cards.Selection.Count + " 张）");
                return false;
            }
            if (Cards.SwapAndDraw(Cards.Selection[0]))
            {
                FlowEvents.RaiseToast("换牌：弃 1 张，补抽 1 张");
                return true;
            }
            return false;
        }

        /// <summary>开战（玩家随时可点）→ 战斗阶段（S5 起敌军生成在此前接入）</summary>
        public void RequestStartBattle()
        {
            if (Phase != Phase.Play) return;
            Cards.ClearSelection();
            SetPhase(Phase.Battle);
            if (CombatSystem.Instance != null)
                CombatSystem.Instance.Begin();
        }

        /// <summary>战斗结束（S4 战斗引擎回调）→ 结算阶段</summary>
        public void OnBattleEnded(bool playerWin)
        {
            SetPhase(Phase.Settle);
            FlowEvents.RaiseBattleEnded(playerWin);
        }

        /// <summary>回标题（清场）</summary>
        public void ReturnToTitle()
        {
            Board.Clear();
            SetPhase(Phase.Title);
        }

        void SetPhase(Phase p)
        {
            Phase = p;
            FlowEvents.RaisePhaseChanged(p);
        }
    }
}
