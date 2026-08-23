using System;
using System.Collections.Generic;
using CnP.Core;

namespace CnP.Domain.Card
{
    /// <summary>
    /// 牌堆：标准 52 张（#D37），支持洗牌/抽牌/弃牌堆。
    /// 本切片单回合内牌堆不会耗尽（起手 13 + 弃换 1），"回合结束洗回"逻辑留多回合时补。
    /// </summary>
    public class DeckModel
    {
        readonly List<CardModel> _drawPile = new List<CardModel>();
        readonly List<CardModel> _discardPile = new List<CardModel>();
        readonly Random _rng;

        public DeckModel(int seed = 0)
        {
            _rng = seed == 0 ? new Random() : new Random(seed);
            _drawPile.AddRange(CardModel.FullDeck());
            Shuffle();
        }

        /// <summary>洗牌（Fisher-Yates）</summary>
        public void Shuffle()
        {
            for (int i = _drawPile.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                var tmp = _drawPile[i];
                _drawPile[i] = _drawPile[j];
                _drawPile[j] = tmp;
            }
        }

        /// <summary>抽一张；牌堆空返回 null</summary>
        public CardModel Draw()
        {
            if (_drawPile.Count == 0) return null;
            var card = _drawPile[_drawPile.Count - 1];
            _drawPile.RemoveAt(_drawPile.Count - 1);
            return card;
        }

        /// <summary>抽 n 张（不足则返回实际数量）</summary>
        public List<CardModel> Draw(int n)
        {
            var result = new List<CardModel>(n);
            for (int i = 0; i < n; i++)
            {
                var c = Draw();
                if (c == null) break;
                result.Add(c);
            }
            return result;
        }

        /// <summary>打出/弃掉的牌进弃牌堆</summary>
        public void AddToDiscard(IEnumerable<CardModel> cards)
        {
            _discardPile.AddRange(cards);
        }

        public int DrawPileCount => _drawPile.Count;
        public int DiscardPileCount => _discardPile.Count;
    }
}
