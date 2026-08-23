using System;
using System.Collections.Generic;
using System.Linq;

namespace CnP.Domain.Card
{
    /// <summary>
    /// 牌型判定引擎（design.md §2.1.2 / #D18）。
    /// 规则：对选中牌做"整体判定"——整副选中必须恰好构成一个牌型，剩余杂牌判定无效。
    /// A 只作顺子/连类顶端（Q-K-A 连续，A-2-3 不连续）。
    /// </summary>
    public static class PokerHandEvaluator
    {
        /// <summary>判定选中牌型；无效返回 null</summary>
        public static HandPattern Evaluate(IReadOnlyList<CardModel> selection)
        {
            if (selection == null || selection.Count == 0) return null;

            // 按点数分组
            var groups = selection.GroupBy(c => c.Rank)
                                  .Select(g => new { Rank = g.Key, Cards = g.ToList() })
                                  .OrderBy(g => g.Rank)
                                  .ToList();

            if (groups.Count == 1)
            {
                int n = groups[0].Cards.Count;
                int rank = groups[0].Rank;
                if (n == 1) return new HandPattern(PatternKind.Single, selection.ToList(), rank, 0, 1);
                if (n == 2) return new HandPattern(PatternKind.Pair, selection.ToList(), rank, 0, 1);
                if (n == 3) return new HandPattern(PatternKind.Triple, selection.ToList(), rank, 0, 1);
                if (n >= 4) return new HandPattern(PatternKind.Bomb, selection.ToList(), rank, 0, 1);
                return null;
            }

            // 三带二：恰好两条（3 + 2）；其余两组结构（3+3 连三张 / 2+2 两对）落入下方通用判定
            if (groups.Count == 2)
            {
                var a = groups[0];
                var b = groups[1];
                if (a.Cards.Count == 3 && b.Cards.Count == 2)
                    return new HandPattern(PatternKind.FullHouse, selection.ToList(), a.Rank, b.Rank, 1);
                if (a.Cards.Count == 2 && b.Cards.Count == 3)
                    return new HandPattern(PatternKind.FullHouse, selection.ToList(), b.Rank, a.Rank, 1);
                // 不返回：3+3 连续的连三张（2 连）由通用路径处理
            }

            // ≥2 组且同结构：连对（≥3 对）/ 连三张（≥2 连）/ 顺子与同花顺（≥5 张）
            int[] counts = groups.Select(g => g.Cards.Count).Distinct().ToArray();

            if (counts.Length == 1)
            {
                int per = counts[0];
                bool consecutive = IsConsecutive(groups.Select(g => g.Rank).ToList());
                int n = groups.Count;
                int high = groups[groups.Count - 1].Rank;

                if (per == 2 && n >= 3 && consecutive)
                    return new HandPattern(PatternKind.PairRun, selection.ToList(), high, 0, n);
                if (per == 3 && n >= 2 && consecutive)
                    return new HandPattern(PatternKind.TripleRun, selection.ToList(), high, 0, n);
                if (per == 1 && n >= 5 && consecutive)
                {
                    bool sameSuit = selection.All(c => c.Suit == selection[0].Suit);
                    var kind = sameSuit ? PatternKind.StraightFlush : PatternKind.Straight;
                    return new HandPattern(kind, selection.ToList(), high, 0, n);
                }
            }

            return null; // 混合结构（如 33344455）整体无效
        }

        /// <summary>点数列表严格连续（排序后每项 +1）</summary>
        static bool IsConsecutive(List<int> ranks)
        {
            for (int i = 1; i < ranks.Count; i++)
                if (ranks[i] != ranks[i - 1] + 1) return false;
            return true;
        }
    }
}
