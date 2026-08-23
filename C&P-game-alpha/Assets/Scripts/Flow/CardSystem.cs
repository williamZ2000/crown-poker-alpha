using System.Collections.Generic;
using System.Linq;
using CnP.Core;
using CnP.Domain.Card;

namespace CnP.Flow
{
    /// <summary>
    /// 卡牌系统：牌堆/手牌/选牌/弃牌换抽（#D37：换牌 1 次 × 1 张）。
    /// </summary>
    public class CardSystem
    {
        public DeckModel Deck { get; private set; }
        public HandModel Hand { get; private set; }

        readonly List<CardModel> _selection = new List<CardModel>();
        public IReadOnlyList<CardModel> Selection => _selection;

        /// <summary>剩余弃牌次数</summary>
        public int DiscardsLeft { get; private set; } = GameParams.DiscardsPerRound;

        public void StartNewRound()
        {
            Deck = new DeckModel();
            Hand = new HandModel();
            _selection.Clear();
            DiscardsLeft = GameParams.DiscardsPerRound;
            // 抽牌阶段（自动）：起手补满
            Hand.AddRange(Deck.Draw(GameParams.HandStart));
        }

        /// <summary>当前选牌的牌型判定（无效为 null）</summary>
        public HandPattern CurrentPattern => PokerHandEvaluator.Evaluate(_selection);

        public void ToggleSelect(CardModel card)
        {
            if (_selection.Contains(card)) _selection.Remove(card);
            else if (Hand.Contains(card)) _selection.Add(card);
            FlowEvents.RaiseHandChanged();
        }

        public void ClearSelection()
        {
            if (_selection.Count == 0) return;
            _selection.Clear();
            FlowEvents.RaiseHandChanged();
        }

        /// <summary>打出选中牌（调用前需判定有效）；牌进弃牌堆</summary>
        public bool PlaySelected(out HandPattern played)
        {
            played = CurrentPattern;
            if (played == null) return false;
            if (!Hand.RemoveAll(_selection)) return false;
            Deck.AddToDiscard(_selection);
            _selection.Clear();
            FlowEvents.RaiseHandChanged();
            return true;
        }

        /// <summary>弃一张牌并补抽一张（#D37：1 次 × 1 张）</summary>
        public bool DiscardAndDraw(CardModel card)
        {
            if (DiscardsLeft <= 0) return false;
            if (!Hand.RemoveAll(new[] { card })) return false;
            Deck.AddToDiscard(new[] { card });
            var drawn = Deck.Draw(GameParams.DiscardSize);
            Hand.AddRange(drawn);
            _selection.Remove(card);
            DiscardsLeft--;
            FlowEvents.RaiseHandChanged();
            return true;
        }
    }
}
