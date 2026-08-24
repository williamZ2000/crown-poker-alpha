using System;
using System.Collections.Generic;
using System.Linq;
using CnP.Core;
using CnP.Domain.Combat;
using CnP.Domain.Unit;

namespace CnP.Domain.Enemy
{
    /// <summary>
    /// 天灵 T1 兵种池（§10.3.1 方向命名）：模板标准件 = 职能源值 × T1 倍率（#D34：
    /// 敌人单位全程标准件，不乘难度 E——难度只进预算）。T2-T4 池随内容设计补齐。
    /// </summary>
    public static class Tier1Pool
    {
        /// <summary>池模板（名字, 职能）</summary>
        static readonly (string name, UnitRole role)[] Templates =
        {
            ("天灵刀手", UnitRole.战士),
            ("天灵轻骑", UnitRole.战士),
            ("天灵枪卫", UnitRole.坦克),
            ("天灵弓手", UnitRole.射手),
            ("天灵猎手", UnitRole.射手),
        };

        /// <summary>生成整池标准件（AttackType 全物理，近战/远程由职能决定）</summary>
        public static List<UnitStats> All(float tierMultiplier = GameParams.EnemyTier1)
        {
            var list = new List<UnitStats>();
            foreach (var (name, role) in Templates)
            {
                var src = RoleSourceMap.Get(role);
                list.Add(new UnitStats
                {
                    Name = name,
                    Role = role,
                    Attack = AttackType.物理,
                    Family = "天灵",
                    Hp = src.Hp * tierMultiplier,
                    AtkP = src.Atk * tierMultiplier,
                    AtkM = 0f,
                    DefP = src.DefP * tierMultiplier,
                    DefM = src.DefM * tierMultiplier,
                    Spd = src.Spd,
                    Range = src.Range,
                    Move = src.Move,
                    TierRank = 1, // T 档显示
                });
            }
            return list;
        }
    }

    /// <summary>
    /// 敌军预算生成器（#D34 §10.3.3）：B = B₀ × E(关) × 回合系数，按 CP 从兵种池随机"购买"直到预算耗尽。
    /// 轻护栏：至少 1 近战 + 1 远程，避免极端阵容（原型期约定，护栏规则可随平衡调整）。
    /// </summary>
    public class EnemyArmyGenerator
    {
        readonly Random _rng;

        public EnemyArmyGenerator(int seed = 0)
        {
            _rng = seed == 0 ? new Random() : new Random(seed);
        }

        public List<UnitStats> Generate(int level, int round, List<UnitStats> pool = null)
        {
            if (pool == null) pool = Tier1Pool.All();
            float budget = GameParams.BaseBudget * GameParams.DifficultyAt(level) * GameParams.RoundCoefficient(round);

            var army = new List<UnitStats>();
            float Cheapest(List<UnitStats> list) => list.Min(CombatPower.Cp);

            // 轻护栏：预算允许时先各买 1 近战 + 1 远程
            var melee = pool.Where(t => t.Range == 1).ToList();
            var ranged = pool.Where(t => t.Range > 1).ToList();
            if (budget >= Cheapest(melee) + Cheapest(ranged))
            {
                army.Add(Pick(melee, budget));
                budget -= CombatPower.Cp(army[army.Count - 1]);
                army.Add(Pick(ranged, budget));
                budget -= CombatPower.Cp(army[army.Count - 1]);
            }

            // 随机购买直到预算买不起最便宜的
            while (true)
            {
                var affordable = pool.Where(t => CombatPower.Cp(t) <= budget).ToList();
                if (affordable.Count == 0) break;
                var pick = Pick(affordable, budget);
                army.Add(pick);
                budget -= CombatPower.Cp(pick);
            }
            return army;
        }

        UnitStats Pick(List<UnitStats> affordable, float budget)
        {
            return affordable[_rng.Next(affordable.Count)];
        }
    }
}
