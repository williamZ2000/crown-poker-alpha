using System.Collections.Generic;
using System.Linq;
using CnP.Core;
using CnP.Domain.Card;

namespace CnP.Domain.Unit
{
    /// <summary>一次召唤订单：兵种模板 + 数量</summary>
    public class SummonBatch
    {
        public UnitStats Template;
        public int Count;

        public SummonBatch(UnitStats template, int count)
        {
            Template = template;
            Count = count;
        }
    }

    /// <summary>
    /// 牌型 → 召唤工厂（#D18 数量规则 + #D33 数值公式）。
    /// 属性 = 职能源值 × 档位倍率（判档点数）× 家族修正；基础线每张 1 个、特殊线指数（每 +2 阶翻倍）。
    /// </summary>
    public static class UnitFactory
    {
        /// <summary>按牌型生成召唤订单（三带二附带全场护甲小增益，由 Flow 层落地）</summary>
        public static List<SummonBatch> CreateSummons(HandPattern pattern)
        {
            var result = new List<SummonBatch>();
            switch (pattern.Kind)
            {
                case PatternKind.Single:
                    result.Add(Make(UnitCatalog.Family.单牌, pattern.KeyRank, GameParams.FamilyBasic, 1));
                    break;

                case PatternKind.Pair:
                case PatternKind.PairRun:
                    // 对子家族：每张 1 个（连对加长同规则），档位按最高点数
                    result.Add(Make(UnitCatalog.Family.对子, pattern.KeyRank, GameParams.FamilyBasic, pattern.Cards.Count));
                    break;

                case PatternKind.Triple:
                case PatternKind.TripleRun:
                    // 三张家族：每张 1 个
                    result.Add(Make(UnitCatalog.Family.三张, pattern.KeyRank, GameParams.FamilyBasic, pattern.Cards.Count));
                    break;

                case PatternKind.FullHouse:
                    // 三带二 = 3 个三张家族兵 + 2 个对子家族兵（+ 小增益由调用方处理）
                    result.Add(Make(UnitCatalog.Family.三张, pattern.KeyRank, GameParams.FamilyBasic, 3));
                    result.Add(Make(UnitCatalog.Family.对子, pattern.SecondaryRank, GameParams.FamilyBasic, 2));
                    break;

                case PatternKind.Straight:
                    // 顺子：基准 5 张 = 1 个铁骑，每 +2 阶翻倍
                    result.Add(Make(UnitCatalog.Family.顺子, pattern.KeyRank, GameParams.FamilyStraight,
                                    GameParams.ExponentialCount(pattern.GroupCount, 5)));
                    break;

                case PatternKind.Bomb:
                    // 炸弹：基准 4 炸 = 1 个，每 +2 阶翻倍（基数 52 最多 4 炸 = 1）
                    result.Add(Make(UnitCatalog.Family.炸弹, pattern.KeyRank, GameParams.FamilyBomb,
                                    GameParams.ExponentialCount(pattern.Cards.Count, 4)));
                    break;

                case PatternKind.StraightFlush:
                    // 同花顺：基准 5 张 = 1 条巨蟒，每 +2 阶翻倍
                    result.Add(Make(UnitCatalog.Family.同花顺, pattern.KeyRank, GameParams.FamilyStraightFlush,
                                    GameParams.ExponentialCount(pattern.GroupCount, 5)));
                    break;
            }
            return result;
        }

        /// <summary>组装一个兵种模板：源值 × 档位倍率 × 家族修正（手感属性不缩放）</summary>
        static SummonBatch Make(UnitCatalog.Family family, int tierRank, float familyMult, int count)
        {
            var tier = TierOf(tierRank);
            var entry = UnitCatalog.Get(family, tier);
            var src = RoleSourceMap.Get(entry.role);

            // 无攻击单位主攻取 0（帝国 v1 目录暂无，规则保留）
            float atk = entry.attack == AttackType.无攻击 ? 0f : src.Atk;
            float atkP = entry.attack == AttackType.法术 ? 0f : atk * GameParams.TierMultiplier(tierRank) * familyMult;
            float atkM = entry.attack == AttackType.法术 ? atk * GameParams.TierMultiplier(tierRank) * familyMult : 0f;

            var stats = new UnitStats
            {
                Name = entry.name,
                Role = entry.role,
                Attack = entry.attack,
                Family = family.ToString(),
                Hp = src.Hp * GameParams.TierMultiplier(tierRank) * familyMult,
                AtkP = atkP,
                AtkM = atkM,
                DefP = src.DefP * GameParams.TierMultiplier(tierRank) * familyMult,
                DefM = src.DefM * GameParams.TierMultiplier(tierRank) * familyMult,
                Spd = src.Spd,
                Range = src.Range,
                Move = src.Move,
                TierRank = tierRank,
            };
            return new SummonBatch(stats, count);
        }

        /// <summary>点数 → 档位（2-10 数字 / JQK 人头 / A）</summary>
        public static TierKind TierOf(int rank)
        {
            if (rank >= 14) return TierKind.Ace;
            if (rank >= 11) return TierKind.Face;
            return TierKind.Number;
        }
    }
}
