using UnityEngine;

namespace CnP.Flow
{
    /// <summary>阵营</summary>
    public enum Side
    {
        Player,
        Enemy,
    }

    /// <summary>
    /// 棋子运行时实例（纯数据，表现由 UI 层 BoardView 同步渲染）。
    /// </summary>
    public class UnitInstance
    {
        public int Id;
        public Side Side;
        public Domain.Unit.UnitStats Stats;
        public float Hp;
        public float MaxHp;
        public bool Alive = true;
        public Vector2 Position;

        /// <summary>本场战斗临时护甲加成（三带二小增益等，公式 v2 固定列）</summary>
        public float BonusDefP;

        public float CurrentDefP => Stats.DefP + BonusDefP;

        public void TakeDamage(float dmg)
        {
            Hp -= dmg;
            if (Hp <= 0f)
            {
                Hp = 0f;
                Alive = false;
            }
        }
    }
}
