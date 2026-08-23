using System.Collections.Generic;
using System.Linq;
using CnP.Domain.Card;
using NUnit.Framework;

namespace CnP.Tests
{
    /// <summary>牌堆/手牌域模型单测</summary>
    public class DeckHandTests
    {
        [Test]
        public void 标准牌堆_五十二张不重复()
        {
            var deck = new DeckModel(seed: 1);
            // 抽干检查唯一性
            var all = new List<CardModel>();
            CardModel c;
            while ((c = deck.Draw()) != null) all.Add(c);
            Assert.AreEqual(52, all.Count);
            Assert.AreEqual(52, all.Distinct().Count());
        }

        [Test]
        public void 抽牌_牌堆递减_空堆返回null()
        {
            var deck = new DeckModel(seed: 7);
            Assert.AreEqual(52, deck.DrawPileCount);
            var drawn = deck.Draw(13);
            Assert.AreEqual(13, drawn.Count);
            Assert.AreEqual(39, deck.DrawPileCount);
            deck.Draw(39);
            Assert.AreEqual(0, deck.DrawPileCount);
            Assert.IsNull(deck.Draw());
        }

        [Test]
        public void 弃牌进弃牌堆()
        {
            var deck = new DeckModel(seed: 3);
            var cards = deck.Draw(5);
            deck.AddToDiscard(cards);
            Assert.AreEqual(5, deck.DiscardPileCount);
        }

        [Test]
        public void 手牌_加入自动排序()
        {
            var hand = new HandModel();
            hand.Add(new CardModel(Suit.Heart, 10));
            hand.Add(new CardModel(Suit.Spade, 3));
            hand.Add(new CardModel(Suit.Club, 14));
            Assert.AreEqual(3, hand.Cards[0].Rank);   // 3 最前
            Assert.AreEqual(14, hand.Cards[2].Rank);  // A 最后
        }

        [Test]
        public void 手牌_超上限抛异常()
        {
            var hand = new HandModel(2);
            hand.Add(new CardModel(Suit.Heart, 2));
            hand.Add(new CardModel(Suit.Heart, 3));
            Assert.Throws<System.InvalidOperationException>(() => hand.Add(new CardModel(Suit.Heart, 4)));
        }

        [Test]
        public void 手牌_移除选中牌()
        {
            var hand = new HandModel();
            var a = new CardModel(Suit.Heart, 5);
            var b = new CardModel(Suit.Spade, 5);
            var d = new CardModel(Suit.Club, 9);
            hand.AddRange(new[] { a, b, d });
            Assert.IsTrue(hand.RemoveAll(new[] { a, b }));
            Assert.AreEqual(1, hand.Count);
            // 移除不在手牌中的牌返回 false
            Assert.IsFalse(hand.RemoveAll(new[] { a }));
        }

        [Test]
        public void 手牌_起手十三张()
        {
            var deck = new DeckModel(seed: 9);
            var hand = new HandModel();
            hand.AddRange(deck.Draw(13));
            Assert.AreEqual(13, hand.Count);
        }
    }
}
