using UnityEngine;

namespace CnP.Core
{
    /// <summary>
    /// 运行时资源工具：原型期零美术资源，白色方块精灵运行时生成 + 染色。
    /// </summary>
    public static class RuntimeAssets
    {
        private static Sprite _whiteSquare;

        /// <summary>白色方块精灵（共享单例，用 SpriteRenderer.color 染色）</summary>
        public static Sprite WhiteSquare
        {
            get
            {
                if (_whiteSquare == null)
                {
                    var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                    var pixels = new Color32[64];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    _whiteSquare = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
                    _whiteSquare.name = "WhiteSquare";
                }
                return _whiteSquare;
            }
        }
    }
}
