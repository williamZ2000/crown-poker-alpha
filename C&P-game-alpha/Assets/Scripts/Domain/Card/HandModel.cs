using System;
using System.Collections.Generic;
using System.Linq;
using CnP.Core;

namespace CnP.Domain.Card
{
    /// <summary>
    /// 手牌：上限 13（#D37），加入时按点数排序保持稳定展示。
    /// </summary>
    public class HandModel
    {
        readonly List<CardModel> _cards = new List<CardModel>();

        public int MaxCount { get; }

        public HandModel(int maxCount = GameParams.HandMax)
        {
            MaxCount = maxCount;
        }

        public IReadOnlyList<CardModel> Cards => _cards;

        /// <summary>加入手牌（超上限抛异常，由 Flow 层保证不超）</summary>
        public void Add(CardModel card)
        {
            if (_cards.Count >= MaxCount)
                throw new InvalidOperationException("手牌已满");
            _cards.Add(card);
            Sort();
        }

        public void AddRange(IEnumerable<CardModel> cards)
        {
            foreach (var c in cards) Add(c);
        }

        /// <summary>移除指定手牌（引用匹配）；任一张不在手牌则返回 false 且不改动</summary>
        public bool RemoveAll(IEnumerable<CardModel> cards)
        {
            var set = new HashSet<CardModel>(cards);
            if (!set.IsSubsetOf(_cards)) return false;
            foreach (var c in set) _cards.Remove(c);
            return true;
        }

        public bool Contains(CardModel card) => _cards.Contains(card);

        public int Count => _cards.Count;

        private void Sort()
        {
            _cards.Sort();
        }
    }
}
