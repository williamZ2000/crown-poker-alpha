using CnP.Core;
using CnP.Flow;
using UnityEngine;

namespace CnP.UI
{
    /// <summary>
    /// 顶部 HUD：关卡/阶段/资源信息 + 提示条（Toast）。
    /// 战斗阶段占位面板在 S4 移除。
    /// </summary>
    public class HUD : MonoBehaviour
    {
        string _toast;
        float _toastUntil;
        GUIStyle _barStyle;
        GUIStyle _toastStyle;
        bool _built;

        void Awake()
        {
            FlowEvents.Toast += OnToast;
        }

        void OnDestroy()
        {
            FlowEvents.Toast -= OnToast;
        }

        void OnToast(string msg)
        {
            _toast = msg;
            _toastUntil = Time.time + 2.6f;
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
            GUI.Label(new Rect(Screen.width - 330f, 10f, 316f, 22f),
                "部署 " + flow.Board.PlayerUnitCount + "/" + GameParams.DeployLimit +
                "   轮次剩 " + flow.RoundsLeft +
                "   弃牌剩 " + flow.Cards.DiscardsLeft, _barStyle);

            // S3 战斗占位面板（S4 战斗引擎接入后移除）
            if (flow.Phase == Phase.Battle)
            {
                var box = new Rect(Screen.width * 0.5f - 190f, Screen.height * 0.42f, 380f, 110f);
                GUI.Box(box, "");
                GUI.Label(new Rect(box.x, box.y + 18f, box.width, 24f), "战斗阶段 —— S4 引擎接入中", _toastStyle);
                if (GUI.Button(new Rect(box.center.x - 90f, box.yMax - 46f, 180f, 32f), "返回出牌（临时）"))
                    flow.TemporaryReturnToPlay();
            }

            // 提示条
            if (!string.IsNullOrEmpty(_toast) && Time.time < _toastUntil)
                GUI.Label(new Rect(0f, 64f, Screen.width, 26f), _toast, _toastStyle);
        }
    }
}
