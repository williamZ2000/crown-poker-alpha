using UnityEngine;

namespace CnP.Core
{
    /// <summary>
    /// 布阵算法（§7.2.3 RTS 式手动布阵）：线列排布（含折行/间距压缩）与整组保持阵型移动。
    /// 纯函数、不落地钳制——调用方（BoardSystem）负责把结果钳制进玩家半场。
    /// </summary>
    public static class FormationPlanner
    {
        /// <summary>普通单位间距（§7.2.3 折行间距同此）</summary>
        public const float DefaultSpacing = 0.85f;
        /// <summary>线列间距压缩下限</summary>
        public const float MinSpacing = 0.55f;
        /// <summary>折行行距（= 普通单位间距）</summary>
        public const float RowSpacing = DefaultSpacing;

        /// <summary>
        /// 线列排布：首行以 anchor 为中心沿 x 展开；单行放不下自动折行（行沿 y 方向堆叠，行距 = 普通间距）；
        /// 行数超出可用深度时压缩行距兜底。间距随拖拽半长自动缩放（长拉稀疏、短拉紧密，限制在 [Min, Default]）。
        /// </summary>
        /// <param name="anchor">线列中心（首行中心点）</param>
        /// <param name="count">单位数</param>
        /// <param name="halfLength">拖拽半长（&lt;=0 时用默认间距）</param>
        /// <param name="availableWidth">可用横向宽度（玩家半场）</param>
        /// <param name="availableHeight">可用纵向深度（玩家半场）</param>
        public static Vector2[] ArrangeLine(Vector2 anchor, int count, float halfLength, float availableWidth, float availableHeight)
        {
            var result = new Vector2[count];
            if (count <= 0) return result;
            if (count == 1) { result[0] = anchor; return result; }

            // 间距：拖拽长度决定（span = 2×halfLength），限制在 [Min, Default]
            float spacing = DefaultSpacing;
            if (halfLength > 0f)
            {
                float span = halfLength * 2f;
                spacing = Mathf.Clamp(span / (count - 1), MinSpacing, DefaultSpacing);
            }

            // 单行容量：n 个单位占 (n-1)×spacing
            int perRow = Mathf.Max(1, Mathf.FloorToInt((availableWidth - 0.0001f) / spacing) + 1);
            perRow = Mathf.Min(perRow, count);
            int rows = Mathf.CeilToInt((float)count / perRow);

            // 行距压缩兜底：总深度超出可用深度 → 等比压缩行距
            float rowSpacing = RowSpacing;
            if (rows > 1)
            {
                float need = (rows - 1) * RowSpacing;
                if (need > availableHeight) rowSpacing = availableHeight / (rows - 1);
            }

            int index = 0;
            for (int r = 0; r < rows; r++)
            {
                int inRow = Mathf.Min(perRow, count - index);
                for (int i = 0; i < inRow; i++)
                {
                    float x = anchor.x + (i - (inRow - 1) / 2f) * spacing;
                    float y = anchor.y + r * rowSpacing;
                    result[index++] = new Vector2(x, y);
                }
            }
            return result;
        }

        /// <summary>
        /// 整组保持相对阵型移动：以编组质心对齐 target，逐单位平移（不做钳制，由调用方落地）。
        /// </summary>
        public static Vector2[] GroupMove(Vector2[] currentPositions, Vector2 targetCenter)
        {
            if (currentPositions == null || currentPositions.Length == 0) return currentPositions;

            var centroid = Vector2.zero;
            foreach (var p in currentPositions) centroid += p;
            centroid /= currentPositions.Length;

            var delta = targetCenter - centroid;
            var result = new Vector2[currentPositions.Length];
            for (int i = 0; i < currentPositions.Length; i++)
                result[i] = currentPositions[i] + delta;
            return result;
        }
    }
}
