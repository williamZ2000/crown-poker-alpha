using CnP.Core;
using CnP.Flow;
using CnP.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CnP.Bootstrap
{
    /// <summary>
    /// 运行时自举（组合根）：Play 即生成相机 / 棋盘底板 / 游戏根节点，不依赖任何编辑器操作。
    /// 注：组合根需引用全部层级（Core/Domain/Flow/UI），故放在最外层程序集而非 Core.asmdef，
    /// 与架构文档"依赖严格单向"一致（Core 不得反向引用业务层）。
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            EnsureCamera();
            BoardBackdrop.Build();
            BuildGameRoot();
        }

        /// <summary>保证存在主相机并规范到固定参数（ISSUE-006 定稿：按 16:9 基准固定，不做运行时自适应；
        /// 张总 2026-08-24 指示——编辑器调试时在 Game 视图左上角锁定 16:9 Aspect 即可全盘可见）</summary>
        static void EnsureCamera()
        {
            if (Camera.main == null)
            {
                var go = new GameObject("MainCamera");
                go.tag = "MainCamera";
                var cam = go.AddComponent<Camera>();
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.07f, 0.08f, 0.11f);
                cam.GetUniversalAdditionalCameraData(); // URP 附加数据，避免管线告警
            }

            // 场景自带相机也规范到固定尺寸：5.6 @16:9 → 可视 ±9.96×±5.6，全棋盘（18.4×7.4）含留白可见
            var main = Camera.main;
            main.orthographic = true;
            main.orthographicSize = 5.6f;
            main.transform.position = new Vector3(0f, BoardGeometry.CenterY, -10f);
        }

        /// <summary>游戏根节点：挂接流程控制器与各 UI 控制器</summary>
        static void BuildGameRoot()
        {
            var root = new GameObject("GameRoot");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<RoundFlowController>(); // 流程层先就位（UI 依赖其 Instance）
            root.AddComponent<CombatSystem>();
            root.AddComponent<BoardView>();
            root.AddComponent<HUD>();
            root.AddComponent<HandView>();
            root.AddComponent<TitleScreen>();
        }
    }
}
