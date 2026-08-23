using System.Collections.Generic;
using System.Linq;
using CnP.Domain.Card;
using NUnit.Framework;

namespace CnP.Tests
{
    /// <summary>牌型判定引擎单测（#D18/#D25/#D26 规则矩阵）</summary>
    public class PokerHandEvaluatorTests
    {
        static CardModel C(Suit suit, int rank) => new CardModel(suit, rank);

        static HandPattern Eval(params CardModel[] cards) => PokerHandEvaluator.Evaluate(cards);

        // ── 基础线 ──────────────────────────────────────

        [Test]
        public void 单牌()
        {
            var p = Eval(C(Suit.Spade, 7));
            Assert.AreEqual(PatternKind.Single, p.Kind);
            Assert.AreEqual(7, p.KeyRank);
        }

        [Test]
        public void 对子()
        {
            var p = Eval(C(Suit.Heart, 5), C(Suit.Diamond, 5));
            Assert.AreEqual(PatternKind.Pair, p.Kind);
            Assert.AreEqual(5, p.KeyRank);
        }

        [Test]
        public void 三张()
        {
            var p = Eval(C(Suit.Heart, 9), C(Suit.Diamond, 9), C(Suit.Club, 9));
            Assert.AreEqual(PatternKind.Triple, p.Kind);
        }

        [Test]
        public void 炸弹_四张同点()
        {
            var p = Eval(C(Suit.Spade, 13), C(Suit.Heart, 13), C(Suit.Club, 13), C(Suit.Diamond, 13));
            Assert.AreEqual(PatternKind.Bomb, p.Kind);
            Assert.AreEqual(13, p.KeyRank);
        }

        [Test]
        public void 炸弹_混入杂牌无效()
        {
            var p = Eval(C(Suit.Spade, 13), C(Suit.Heart, 13), C(Suit.Club, 13), C(Suit.Diamond, 13), C(Suit.Spade, 2));
            Assert.IsNull(p); // 整体判定：炸弹 + 单张 = 无效
        }

        // ── 连对 / 连三张 ───────────────────────────────

        [Test]
        public void 连对_三连()
        {
            var p = Eval(C(Suit.Spade, 3), C(Suit.Heart, 3),
                         C(Suit.Spade, 4), C(Suit.Heart, 4),
                         C(Suit.Spade, 5), C(Suit.Heart, 5));
            Assert.AreEqual(PatternKind.PairRun, p.Kind);
            Assert.AreEqual(3, p.GroupCount);
            Assert.AreEqual(5, p.KeyRank); // 最高点数判档
        }

        [Test]
        public void 连对_两对不足无效()
        {
            var p = Eval(C(Suit.Spade, 3), C(Suit.Heart, 3), C(Suit.Spade, 4), C(Suit.Heart, 4));
            Assert.IsNull(p);
        }

        [Test]
        public void 连对_不连续无效()
        {
            var p = Eval(C(Suit.Spade, 3), C(Suit.Heart, 3),
                         C(Suit.Spade, 5), C(Suit.Heart, 5),
                         C(Suit.Spade, 7), C(Suit.Heart, 7));
            Assert.IsNull(p);
        }

        [Test]
        public void 连三张_两连()
        {
            var p = Eval(C(Suit.Spade, 12), C(Suit.Heart, 12), C(Suit.Club, 12),
                         C(Suit.Spade, 13), C(Suit.Heart, 13), C(Suit.Club, 13));
            Assert.AreEqual(PatternKind.TripleRun, p.Kind);
            Assert.AreEqual(2, p.GroupCount);
            Assert.AreEqual(13, p.KeyRank);
        }

        // ── 三带二 ─────────────────────────────────────

        [Test]
        public void 三带二_顺写()
        {
            var p = Eval(C(Suit.Spade, 3), C(Suit.Heart, 3), C(Suit.Club, 3),
                         C(Suit.Spade, 2), C(Suit.Heart, 2));
            Assert.AreEqual(PatternKind.FullHouse, p.Kind);
            Assert.AreEqual(3, p.KeyRank);
            Assert.AreEqual(2, p.SecondaryRank);
        }

        [Test]
        public void 三带二_对子在前()
        {
            var p = Eval(C(Suit.Spade, 2), C(Suit.Heart, 2),
                         C(Suit.Spade, 3), C(Suit.Heart, 3), C(Suit.Club, 3));
            Assert.AreEqual(PatternKind.FullHouse, p.Kind);
            Assert.AreEqual(3, p.KeyRank);
            Assert.AreEqual(2, p.SecondaryRank);
        }

        [Test]
        public void 三带二_双三条走连三张()
        {
            var p = Eval(C(Suit.Spade, 3), C(Suit.Heart, 3), C(Suit.Club, 3),
                         C(Suit.Spade, 4), C(Suit.Heart, 4), C(Suit.Club, 4));
            Assert.AreEqual(PatternKind.TripleRun, p.Kind); // 333444 = 连三张而非三带二
        }

        [Test]
        public void 混合结构_整体无效()
        {
            var p = Eval(C(Suit.Spade, 3), C(Suit.Heart, 3), C(Suit.Club, 3),
                         C(Suit.Spade, 4), C(Suit.Heart, 4), C(Suit.Club, 4),
                         C(Suit.Spade, 5), C(Suit.Heart, 5));
            Assert.IsNull(p); // 33344455：三带二后余 44，整体无效
        }

        // ── 顺子 / 同花顺 ───────────────────────────────

        [Test]
        public void 顺子_基本()
        {
            var p = Eval(C(Suit.Spade, 2), C(Suit.Heart, 3), C(Suit.Club, 4),
                         C(Suit.Diamond, 5), C(Suit.Spade, 6));
            Assert.AreEqual(PatternKind.Straight, p.Kind);
            Assert.AreEqual(5, p.GroupCount);
            Assert.AreEqual(6, p.KeyRank);
        }

        [Test]
        public void 顺子_十到A()
        {
            var p = Eval(C(Suit.Spade, 10), C(Suit.Heart, 11), C(Suit.Club, 12),
                         C(Suit.Diamond, 13), C(Suit.Spade, 14));
            Assert.AreEqual(PatternKind.Straight, p.Kind);
            Assert.AreEqual(14, p.KeyRank);
        }

        [Test]
        public void 顺子_A2345有效按5判档()
        {
            var p = Eval(C(Suit.Spade, 14), C(Suit.Heart, 2), C(Suit.Club, 3),
                         C(Suit.Diamond, 4), C(Suit.Spade, 5));
            Assert.AreEqual(PatternKind.Straight, p.Kind);
            Assert.AreEqual(5, p.KeyRank); // A 低用判档取顺内最高非 A 牌（#D38）
        }

        [Test]
        public void 同花顺_A2345有效()
        {
            var p = Eval(C(Suit.Heart, 14), C(Suit.Heart, 2), C(Suit.Heart, 3),
                         C(Suit.Heart, 4), C(Suit.Heart, 5));
            Assert.AreEqual(PatternKind.StraightFlush, p.Kind);
            Assert.AreEqual(5, p.KeyRank);
        }

        [Test]
        public void 顺子_A23456按6判档()
        {
            var p = Eval(C(Suit.Spade, 14), C(Suit.Heart, 2), C(Suit.Club, 3),
                         C(Suit.Diamond, 4), C(Suit.Spade, 5), C(Suit.Heart, 6));
            Assert.AreEqual(PatternKind.Straight, p.Kind);
            Assert.AreEqual(6, p.KeyRank);
        }

        [Test]
        public void 连对_AA22仍无效()
        {
            var p = Eval(C(Suit.Spade, 14), C(Suit.Heart, 14),
                         C(Suit.Spade, 2), C(Suit.Heart, 2),
                         C(Suit.Spade, 3), C(Suit.Heart, 3));
            Assert.IsNull(p); // A 两用不外溢到连对（#D38）：A22 33 不连续
        }

        [Test]
        public void 顺子_A不能同时高低用()
        {
            var p = Eval(C(Suit.Spade, 14), C(Suit.Heart, 12), C(Suit.Club, 13),
                         C(Suit.Diamond, 2), C(Suit.Spade, 3), C(Suit.Heart, 4));
            Assert.IsNull(p); // QKA234：A 只能取一种身份，整体无效
        }

        [Test]
        public void 顺子_四张不足无效()
        {
            var p = Eval(C(Suit.Spade, 2), C(Suit.Heart, 3), C(Suit.Club, 4), C(Suit.Diamond, 5));
            Assert.IsNull(p);
        }

        [Test]
        public void 同花顺_判定优先于顺子()
        {
            var p = Eval(C(Suit.Spade, 4), C(Suit.Spade, 5), C(Suit.Spade, 6),
                         C(Suit.Spade, 7), C(Suit.Spade, 8));
            Assert.AreEqual(PatternKind.StraightFlush, p.Kind);
            Assert.AreEqual(8, p.KeyRank);
        }

        [Test]
        public void 十三连顺_二到A()
        {
            // 混花色：全同花色的 13 连按规则应判同花顺
            var suits = new[] { Suit.Heart, Suit.Spade };
            var cards = Enumerable.Range(2, 13).Select((r, i) => C(suits[i % 2], r)).ToArray();
            var p = PokerHandEvaluator.Evaluate(cards);
            Assert.AreEqual(PatternKind.Straight, p.Kind);
            Assert.AreEqual(13, p.GroupCount);
            Assert.AreEqual(14, p.KeyRank);
        }

        // ── 边界 ───────────────────────────────────────

        [Test]
        public void 空选与非法点数()
        {
            Assert.IsNull(PokerHandEvaluator.Evaluate(new List<CardModel>()));
            // ArgumentOutOfRangeException 是 ArgumentException 的子类，用 Catch 断言族
            Assert.Catch<System.ArgumentException>(() => new CardModel(Suit.Heart, 1));
            Assert.Catch<System.ArgumentException>(() => new CardModel(Suit.Heart, 15));
        }

        [Test]
        public void 随机子集不抛异常()
        {
            var deck = CardModel.FullDeck();
            var rng = new System.Random(42);
            for (int trial = 0; trial < 1000; trial++)
            {
                int n = rng.Next(1, 10);
                var subset = new List<CardModel>();
                var used = new HashSet<int>();
                while (subset.Count < n)
                {
                    int idx = rng.Next(52);
                    if (used.Add(idx)) subset.Add(deck[idx]);
                }
                Assert.DoesNotThrow(() => PokerHandEvaluator.Evaluate(subset));
            }
        }
    }
}
