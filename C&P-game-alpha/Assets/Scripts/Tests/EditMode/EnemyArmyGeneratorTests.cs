using System.Linq;
using CnP.Core;
using CnP.Domain.Combat;
using CnP.Domain.Enemy;
using NUnit.Framework;

namespace CnP.Tests
{
    /// <summary>敌军预算生成器单测（#D34：B = B₀×E×回合系数，按 CP 购买 + 轻护栏）</summary>
    public class EnemyArmyGeneratorTests
    {
        [Test]
        public void 关卡1回合1_预算正确并耗尽()
        {
            // B = 250 × 1.5^0 × 1.0 = 250
            var army = new EnemyArmyGenerator(seed: 42).Generate(1, 1);
            float spent = army.Sum(CombatPower.Cp);
            Assert.LessOrEqual(spent, GameParams.BaseBudget, "总花费不得超过预算");

            // 预算耗尽：剩余额度买不起池里最便宜的
            float cheapest = Tier1Pool.All().Min(CombatPower.Cp);
            Assert.Less(GameParams.BaseBudget - spent, cheapest, "必须买到买不起为止");
        }

        [Test]
        public void 数量区间_约九到十二个()
        {
            // 最便宜 22.4（射手×0.8）、最贵 27.2（战士×0.8）→ 250 预算约 9~11 个
            for (int seed = 1; seed <= 10; seed++)
            {
                var army = new EnemyArmyGenerator(seed).Generate(1, 1);
                Assert.GreaterOrEqual(army.Count, 8, "seed=" + seed);
                Assert.LessOrEqual(army.Count, 12, "seed=" + seed);
            }
        }

        [Test]
        public void 轻护栏_至少一近战一远程()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                var army = new EnemyArmyGenerator(seed).Generate(1, 1);
                Assert.IsTrue(army.Any(u => u.Range == 1), "缺少近战 seed=" + seed);
                Assert.IsTrue(army.Any(u => u.Range > 1), "缺少远程 seed=" + seed);
            }
        }

        [Test]
        public void 同种子确定性生成()
        {
            var a = new EnemyArmyGenerator(seed: 7).Generate(1, 1);
            var b = new EnemyArmyGenerator(seed: 7).Generate(1, 1);
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
                Assert.AreEqual(a[i].Name, b[i].Name);
        }

        [Test]
        public void 全部为T1标准件()
        {
            var army = new EnemyArmyGenerator(seed: 3).Generate(1, 1);
            foreach (var u in army)
            {
                var src = GameParams.RoleSources[u.Role.ToString()];
                Assert.AreEqual(src.Hp * GameParams.EnemyTier1, u.Hp, 0.001f);
                Assert.AreEqual(src.Atk * GameParams.EnemyTier1, u.AtkP, 0.001f);
                Assert.AreEqual(src.Spd, u.Spd, 0.001f, "手感属性不缩放");
                Assert.AreEqual("天灵", u.Family);
            }
        }

        [Test]
        public void 难度倍率生效_关内回合系数()
        {
            // 关 2 回合 3：B = 250 × 1.5^1 × 2.5 = 937.5 → 军队明显更大
            var big = new EnemyArmyGenerator(seed: 5).Generate(2, 3);
            var small = new EnemyArmyGenerator(seed: 5).Generate(1, 1);
            Assert.Greater(big.Count, small.Count);
            float bigSpent = big.Sum(CombatPower.Cp);
            Assert.LessOrEqual(bigSpent, 250f * 1.5f * 2.5f + 0.01f);
        }
    }
}
