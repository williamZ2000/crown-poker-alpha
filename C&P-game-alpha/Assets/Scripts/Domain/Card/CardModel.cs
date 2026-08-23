using System;
using System.Collections.Generic;

namespace CnP.Domain.Card
{
    /// <summary>花色（黑桃/红桃/梅花/方片）</summary>
    public enum Suit
    {
        Spade = 0,
        Heart = 1,
        Club = 2,
        Diamond = 3,
    }

    /// <summary>
    /// 一张扑克牌（不可变）。点数 2~14：2-10 数字、J=11 / Q=12 / K=13 人头、A=14。
    /// </summary>
    public sealed class CardModel : IComparable<CardModel>
    {
        public readonly Suit Suit;
        public readonly int Rank;

        public CardModel(Suit suit, int rank)
        {
            if (rank < 2 || rank > 14)
                throw new ArgumentOutOfRangeException(nameof(rank), "点数必须在 2~14（A=14）");
            Suit = suit;
            Rank = rank;
        }

        /// <summary>是否人头牌（J/Q/K）</summary>
        public bool IsFace => Rank >= 11 && Rank <= 13;

        /// <summary>是否 A</summary>
        public bool IsAce => Rank == 14;

        /// <summary>显示用花色符号</summary>
        public string SuitSymbol
        {
            get
            {
                switch (Suit)
                {
                    case Suit.Spade: return "♠";
                    case Suit.Heart: return "♥";
                    case Suit.Club: return "♣";
                    default: return "♦";
                }
            }
        }

        /// <summary>显示用点数标签（10/J/Q/K/A）</summary>
        public string RankLabel
        {
            get
            {
                if (Rank <= 10) return Rank.ToString();
                switch (Rank)
                {
                    case 11: return "J";
                    case 12: return "Q";
                    case 13: return "K";
                    default: return "A";
                }
            }
        }

        /// <summary>显示名，如 ♠A、♥10</summary>
        public string Display => SuitSymbol + RankLabel;

        public override string ToString() => Display;

        public int CompareTo(CardModel other)
        {
            if (other == null) return 1;
            int c = Rank.CompareTo(other.Rank);
            return c != 0 ? c : ((int)Suit).CompareTo((int)other.Suit);
        }

        public override bool Equals(object obj) =>
            obj is CardModel other && Suit == other.Suit && Rank == other.Rank;

        public override int GetHashCode() => ((int)Suit << 8) | Rank;

        /// <summary>生成标准 52 张牌堆（4 花色 × 13 点数，每花色点数各 1 张，#D37）</summary>
        public static List<CardModel> FullDeck()
        {
            var deck = new List<CardModel>(52);
            for (int suit = 0; suit < 4; suit++)
                for (int rank = 2; rank <= 14; rank++)
                    deck.Add(new CardModel((Suit)suit, rank));
            return deck;
        }
    }
}
