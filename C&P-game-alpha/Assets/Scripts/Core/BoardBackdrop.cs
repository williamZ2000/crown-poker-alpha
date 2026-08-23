using UnityEngine;

namespace CnP.Core
{
    /// <summary>
    /// 棋盘底板视觉：运行时生成（背景 / 双方半场 / 中线），零编辑器操作、零美术资源。
    /// </summary>
    public static class BoardBackdrop
    {
        public static void Build()
        {
            var parent = new GameObject("BoardBackdrop");
            Object.DontDestroyOnLoad(parent);

            // 全盘底色
            MakeSprite(parent, "Bg", new Vector2(0f, BoardGeometry.CenterY),
                new Vector2(BoardGeometry.HalfWidth * 2f, BoardGeometry.HalfHeight * 2f),
                new Color(0.10f, 0.11f, 0.16f), 0);

            // 玩家半场（冷色）
            MakeSprite(parent, "PlayerZone", new Vector2(-BoardGeometry.HalfWidth / 2f, BoardGeometry.CenterY),
                new Vector2(BoardGeometry.HalfWidth, BoardGeometry.HalfHeight * 2f),
                new Color(0.13f, 0.16f, 0.24f), 1);

            // 敌方半场（暖色）
            MakeSprite(parent, "EnemyZone", new Vector2(BoardGeometry.HalfWidth / 2f, BoardGeometry.CenterY),
                new Vector2(BoardGeometry.HalfWidth, BoardGeometry.HalfHeight * 2f),
                new Color(0.20f, 0.13f, 0.15f), 1);

            // 中线（暗金）
            MakeSprite(parent, "Midline", new Vector2(0f, BoardGeometry.CenterY),
                new Vector2(0.07f, BoardGeometry.HalfHeight * 2f),
                new Color(0.85f, 0.72f, 0.42f), 2);
        }

        static void MakeSprite(GameObject parent, string name, Vector2 pos, Vector2 scale, Color color, int sortOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeAssets.WhiteSquare;
            sr.color = color;
            sr.sortingOrder = sortOrder;
        }
    }
}
