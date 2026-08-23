using System.Collections.Generic;
using System.Linq;

namespace CnP.Domain.Card
{
    /// <summary>牌型类别（6 家族 + 三带二，#D18；双同花顺需后天构筑牌堆，本切片不判定）</summary>
    public enum PatternKind
    {
        Single,        // 单牌
        Pair,          // 对子
        Triple,        // 三张
        PairRun,       // 连对（≥3 对连续）
        TripleRun,     // 连三张（≥2 连连续三条）
        FullHouse,     // 三带二（三条 + 对子）
        Straight,      // 顺子（≥5 张连续）
        Bomb,          // 炸弹（≥4 张同点）
        StraightFlush, // 同花顺（≥5 张连续同花色）
    }

    /// <summary>
    /// 判定结果：类别 + 构成牌 + 判档信息。
    /// 判档规则：最高点数判档（#D18；顺子/同花顺按最高点数，#D25/#D26 数值层修订）。
    /// </summary>
    public class HandPattern
    {
        public PatternKind Kind;
        public List<CardModel> Cards;
        /// <summary>判档点数（最高点数；三带二取三条点数）</summary>
        public int KeyRank;
        /// <summary>三带二的对子点数（其余牌型为 0）</summary>
        public int SecondaryRank;
        /// <summary>连数/张数：连对=对数、连三张=三张数、顺子与同花顺=张数、其余=1</summary>
        public int GroupCount;

        public HandPattern(PatternKind kind, List<CardModel> cards, int keyRank, int secondaryRank, int groupCount)
        {
            Kind = kind;
            Cards = cards;
            KeyRank = keyRank;
            SecondaryRank = secondaryRank;
            GroupCount = groupCount;
        }

        /// <summary>中文名（UI 直用）</summary>
        public string DisplayName
        {
            get
            {
                switch (Kind)
                {
                    case PatternKind.Single: return "单牌";
                    case PatternKind.Pair: return "对子";
                    case PatternKind.Triple: return "三张";
                    case PatternKind.PairRun: return "连对·" + GroupCount + "连";
                    case PatternKind.TripleRun: return "连三张·" + GroupCount + "连";
                    case PatternKind.FullHouse: return "三带二";
                    case PatternKind.Straight: return "顺子·" + GroupCount + "张";
                    case PatternKind.Bomb: return "炸弹·" + Cards.Count + "炸";
                    case PatternKind.StraightFlush: return "同花顺·" + GroupCount + "张";
                    default: return Kind.ToString();
                }
            }
        }
    }
}
