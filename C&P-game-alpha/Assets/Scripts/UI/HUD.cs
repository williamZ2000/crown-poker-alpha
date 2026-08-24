using CnP.Core;
using CnP.Flow;
using UnityEngine;

namespace CnP.UI
{
    /// <summary>
    /// 顶部 HUD：关卡/阶段/资源信息 + 提示条（Toast）+ 战斗结果面板（完整结算面板 S6 接入）。
    /// </summary>
    public class HUD : MonoBehaviour
    {
        string _toast;
        float _toastUntil;
        bool? _playerWin; // 战斗结果（null = 未出）
        GUIStyle _barStyle;
        GUIStyle _toastStyle;
        GUIStyle _resultStyle;
        bool _built;

        void Awake()
        {
            FlowEvents.Toast += OnToast;
            FlowEvents.BattleEnded += OnBattleEnded;
        }

        void OnDestroy()
        {
            FlowEvents.Toast -= OnToast;
            FlowEvents.BattleEnded -= OnBattleEnded;
        }

        void OnToast(string msg)
        {
            _toast = msg;
            _toastUntil = Time.time + 2.6f;
        }

        void OnBattleEnded(bool playerWin)
        {
            _playerWin = playerWin;
        }

        void BuildStyles()
        {
            if (_built) return;
            _barStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.82f, 0.82f, 0.88f) },
            };
            _toastStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.98f, 0.9f, 0.6f) },
            };
            _resultStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.86f, 0.55f) },
            };
            _built = true;
        }

        void OnGUI()
        {
            BuildStyles();
            var flow = RoundFlowController.Instance;
            if (flow == null) return;

            // 标题阶段不显示 HUD
            if (flow.Phase == Phase.Title) return;

            string phaseName = flow.Phase == Phase.Play ? "出牌阶段"
                             : flow.Phase == Phase.Battle ? "战斗阶段"
                             : "结算阶段";
            GUI.Label(new Rect(14f, 10f, 300f, 22f), "关卡 1-1 · 第 1/1 回合 · " + phaseName, _barStyle);
            GUI.Label(new Rect(Screen.width - 430f, 10f, 416f, 22f),
                "敌军 " + flow.Board.EnemyUnitCount +
                "（战力 " + flow.EnemyArmyCp.ToString("0") + "）   " +
                "我方 " + flow.Board.PlayerUnitCount + "/" + GameParams.DeployLimit +
                "   轮次剩 " + flow.RoundsLeft +
                "   弃牌剩 " + flow.Cards.DiscardsLeft, _barStyle);

            // 战斗中计时
            if (flow.Phase == Phase.Battle && CombatSystem.Instance != null)
                GUI.Label(new Rect(0f, 10f, Screen.width, 22f),
                    "⏱ " + CombatSystem.Instance.BattleTime.ToString("F0") + "s", _centerStyle ?? BuildCenterStyle());

            // 战斗结果面板（完整结算与重开 S6 接入）
            if (flow.Phase == Phase.Settle)
            {
                var prev = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.75f);
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
                GUI.color = prev;

                string title = _playerWin == true ? "胜  利" : _playerWin == false ? "战  败" : "战斗结束";
                GUI.Label(new Rect(0f, Screen.height * 0.34f, Screen.width, 48f), title, _resultStyle);
                GUI.Label(new Rect(0f, Screen.height * 0.34f + 58f, Screen.width, 24f),
                    "关卡 1-1 · 完整结算面板与重新开始将在 S6 接入", _toastStyle);
                if (GUI.Button(new Rect(Screen.width * 0.5f - 90f, Screen.height * 0.34f + 100f, 180f, 34f), "回到标题"))
                    flow.ReturnToTitle();
            }

            // 提示条
            if (!string.IsNullOrEmpty(_toast) && Time.time < _toastUntil)
                GUI.Label(new Rect(0f, 64f, Screen.width, 26f), _toast, _toastStyle);
        }

        GUIStyle _centerStyle;
        GUIStyle BuildCenterStyle()
        {
            _centerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.82f, 0.82f, 0.88f) },
            };
            return _centerStyle;
        }
    }
}
