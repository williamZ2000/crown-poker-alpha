using CnP.Flow;
using UnityEngine;

namespace CnP.UI
{
    /// <summary>
    /// 标题界面（IMGUI）。S3 接入回合流后，开始按钮将调用 RoundFlowController.StartNewRound()。
    /// </summary>
    public class TitleScreen : MonoBehaviour
    {
        public static TitleScreen Instance { get; private set; }

        /// <summary>是否已进入对局（用于隐藏标题）</summary>
        public bool HasStarted { get; private set; }

        GUIStyle _titleStyle;
        GUIStyle _subStyle;
        GUIStyle _hintStyle;
        bool _stylesBuilt;

        void Awake()
        {
            Instance = this;
        }

        void BuildStyles()
        {
            if (_stylesBuilt) return;
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 44,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.92f, 0.84f, 0.62f) },
            };
            _subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.72f, 0.72f, 0.78f) },
            };
            _hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                normal = { textColor = new Color(0.55f, 0.57f, 0.63f) },
            };
            _stylesBuilt = true;
        }

        void OnGUI()
        {
            BuildStyles();

            if (!HasStarted)
            {
                // 全屏暗色遮罩
                var prevColor = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.92f);
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
                GUI.color = prevColor;

                float cx = Screen.width * 0.5f;
                GUI.Label(new Rect(cx - 400f, Screen.height * 0.24f, 800f, 60f), "皇 冠 与 扑 克", _titleStyle);
                GUI.Label(new Rect(cx - 400f, Screen.height * 0.24f + 66f, 800f, 26f),
                    "Crown & Poker · alpha 原型切片 —— 关卡 1-1", _subStyle);

                var btnRect = new Rect(cx - 110f, Screen.height * 0.5f, 220f, 48f);
                if (GUI.Button(btnRect, "开 始 游 戏"))
                {
                    StartGame();
                }

                GUI.Label(new Rect(cx - 320f, Screen.height * 0.5f + 90f, 640f, 90f),
                    "抽 13 张起手 · 出牌凑牌型召唤棋子（4 轮）· 自动战斗\n" +
                    "本切片范围：进入界面 + 关卡 1-1（第一关第一回合），无商店",
                    _hintStyle);
            }
            else
            {
                // 对局进行中由 HUD/HandView 接管界面，标题不再绘制
            }
        }

        void StartGame()
        {
            HasStarted = true;
            if (RoundFlowController.Instance != null)
                RoundFlowController.Instance.StartNewRound();
        }

        /// <summary>回到标题（结算面板"返回标题"用）</summary>
        public void ReturnToTitle()
        {
            HasStarted = false;
        }
    }
}
