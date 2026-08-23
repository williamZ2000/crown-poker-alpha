using UnityEngine;

namespace CnP.Core
{
    /// <summary>
    /// 棋盘世界坐标几何（原型 2D 俯视棋盘：玩家左半场 / 敌方右半场，中线分隔）。
    /// 战斗与部署代码用这里的常量做边界钳制。
    /// </summary>
    public static class BoardGeometry
    {
        public const float HalfWidth = 9.2f;   // 棋盘半宽（全宽 18.4）
        public const float HalfHeight = 3.7f;  // 棋盘半高（全高 7.4）
        public const float CenterY = 0.55f;    // 棋盘中心 Y（给底部手牌 UI 留屏幕空间）
        public const float MidGap = 0.55f;     // 中线两侧禁入区半宽（单位不可跨半场）
        public const float Margin = 0.55f;     // 棋盘外沿留白

        /// <summary>玩家半场可行区域（x ∈ [左边界, −中线禁区]，y ∈ 棋盘上下沿）</summary>
        public static Vector2 ClampToPlayerZone(Vector2 pos, float unitHalf = 0.4f)
        {
            float minX = -HalfWidth + Margin + unitHalf;
            float maxX = -MidGap - unitHalf;
            float minY = CenterY - HalfHeight + Margin * 0.5f + unitHalf;
            float maxY = CenterY + HalfHeight - Margin * 0.5f - unitHalf;
            return new Vector2(Mathf.Clamp(pos.x, minX, maxX), Mathf.Clamp(pos.y, minY, maxY));
        }

        /// <summary>敌方半场可行区域（x ∈ [+中线禁区, 右边界]）</summary>
        public static Vector2 ClampToEnemyZone(Vector2 pos, float unitHalf = 0.4f)
        {
            float minX = MidGap + unitHalf;
            float maxX = HalfWidth - Margin - unitHalf;
            float minY = CenterY - HalfHeight + Margin * 0.5f + unitHalf;
            float maxY = CenterY + HalfHeight - Margin * 0.5f - unitHalf;
            return new Vector2(Mathf.Clamp(pos.x, minX, maxX), Mathf.Clamp(pos.y, minY, maxY));
        }

        /// <summary>全棋盘可行区域（战斗移动用——战斗中允许越过中线，否则近战永远够不到中线外的远程；
        /// "不可被推出半场"（§7.1）指防击退位移类效果，不限制主动战斗移动）</summary>
        public static Vector2 ClampToBoard(Vector2 pos, float unitHalf = 0.4f)
        {
            float minX = -HalfWidth + Margin + unitHalf;
            float maxX = HalfWidth - Margin - unitHalf;
            float minY = CenterY - HalfHeight + Margin * 0.5f + unitHalf;
            float maxY = CenterY + HalfHeight - Margin * 0.5f - unitHalf;
            return new Vector2(Mathf.Clamp(pos.x, minX, maxX), Mathf.Clamp(pos.y, minY, maxY));
        }

        /// <summary>玩家半场可用横向宽度（布阵线列容量计算用）</summary>
        public static float PlayerZoneWidth(float unitHalf = 0.4f)
        {
            return (-MidGap - unitHalf) - (-HalfWidth + Margin + unitHalf);
        }

        /// <summary>玩家半场可用纵向深度（布阵线列容量计算用）</summary>
        public static float PlayerZoneHeight(float unitHalf = 0.4f)
        {
            return (CenterY + HalfHeight - Margin * 0.5f - unitHalf) - (CenterY - HalfHeight + Margin * 0.5f + unitHalf);
        }
    }
}
