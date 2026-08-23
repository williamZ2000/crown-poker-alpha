using CnP.Domain.Card;
using CnP.Domain.Unit;
using CnP.Flow;
using System.Linq;
using UnityEngine;

namespace CnP.UI
{
    /// <summary>
    /// 手牌交互（IMGUI）：点选/框选牌 → 牌型预览 → 出牌召唤；弃牌换抽；开战。
    /// </summary>
    public class HandView : MonoBehaviour
    {
        bool _discardMode;

        GUIStyle _cardStyle;
        GUIStyle _cardStyleSelected;
        GUIStyle _previewStyle;
        bool _built;

        void BuildStyles()
        {
            if (_built) return;
            _cardStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            _cardStyleSelected = new GUIStyle(_cardStyle)
            {
                fontSize = 19,
                normal = { textColor = new Color(0.55f, 0.42f, 0.1f) },
                fontStyle = FontStyle.Bold,
            };
            _previewStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.85f, 0.85f, 0.9f) },
            };
            _built = true;
        }

        void OnGUI()
        {
            BuildStyles();
            var flow = RoundFlowController.Instance;
            if (flow == null || flow.Phase != Phase.Play) return;

            var hand = flow.Cards.Hand.Cards;
            var pattern = flow.Cards.CurrentPattern;

            float cardW = 54f, cardH = 74f, gap = 4f;
            float totalW = hand.Count * (cardW + gap) - gap;
            float startX = (Screen.width - totalW) * 0.5f;
            float rowY = Screen.height - cardH - 12f;

            // 牌型预览行
            string preview;
            if (pattern == null)
            {
                preview = hand.Count == 0 ? "手牌已打空 —— 点击「开战」结束部署"
                                          : "选中牌未构成有效牌型（整体判定）";
            }
            else
            {
                var batches = UnitFactory.CreateSummons(pattern); // 只读预览，不改状态
                var parts = new System.Text.StringBuilder();
                foreach (var b in batches)
                {
                    if (parts.Length > 0) parts.Append("  +  ");
                    parts.Append(b.Template.Name).Append("×").Append(b.Count);
                }
                if (pattern.Kind == PatternKind.FullHouse) parts.Append("  +全体护甲+2（本场）");
                preview = pattern.DisplayName + " → 召唤 " + parts;
            }
            GUI.Label(new Rect(0f, rowY - 46f, Screen.width, 24f), preview, _previewStyle);

            // 手牌
            for (int i = 0; i < hand.Count; i++)
            {
                var card = hand[i];
                var rect = new Rect(startX + i * (cardW + gap), rowY, cardW, cardH);
                bool selected = flow.Cards.Selection.Contains(card);
                bool red = card.Suit == Suit.Heart || card.Suit == Suit.Diamond;

                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = selected ? new Color(1f, 0.85f, 0.45f) : Color.white;
                var style = selected ? _cardStyleSelected : _cardStyle;
                var prevColor = style.normal.textColor;
                if (!selected) style.normal.textColor = red ? new Color(0.75f, 0.15f, 0.15f) : Color.black;

                if (GUI.Button(rect, card.Display, style))
                {
                    if (_discardMode && flow.Cards.DiscardsLeft > 0)
                    {
                        flow.DiscardCard(card);
                        // ISSUE-002 修复：弃牌次数耗尽自动退出弃牌模式，避免点牌永远走弃牌分支
                        if (flow.Cards.DiscardsLeft <= 0) _discardMode = false;
                    }
                    else
                    {
                        flow.Cards.ToggleSelect(card);
                    }
                }
                style.normal.textColor = prevColor;
                GUI.backgroundColor = prevBg;
            }

            // 操作按钮行
            float btnY = rowY - 84f;
            float cx = Screen.width * 0.5f;

            var playLabel = flow.RoundsLeft > 0
                ? "出 牌（剩 " + flow.RoundsLeft + " 轮）"
                : "轮次已用完";
            bool canPlay = flow.RoundsLeft > 0 && pattern != null;
            var prev = GUI.enabled;
            GUI.enabled = canPlay;
            if (GUI.Button(new Rect(cx - 330f, btnY, 200f, 34f), playLabel))
                flow.TryPlayCurrentPattern();
            GUI.enabled = prev;

            var discardLabel = flow.Cards.DiscardsLeft > 0
                ? (_discardMode ? "弃牌中：点一张牌（取消）" : "弃牌换抽 ×" + flow.Cards.DiscardsLeft)
                : "弃牌已用完";
            GUI.enabled = flow.Cards.DiscardsLeft > 0;
            if (GUI.Button(new Rect(cx - 118f, btnY, 210f, 34f), discardLabel))
                _discardMode = !_discardMode;
            GUI.enabled = prev;

            if (GUI.Button(new Rect(cx + 106f, btnY, 150f, 34f), "开 战"))
            {
                _discardMode = false;
                flow.RequestStartBattle();
            }
        }
    }
}
