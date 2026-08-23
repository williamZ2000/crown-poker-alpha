using System;

namespace CnP.Domain.Combat
{
    /// <summary>
    /// 战斗力换算（#D34）：CP = √(EHP × DPS) × 特效系数（本切片特效系数 = 1）。
    /// 用途：S5 敌军预算生成计价 + 战力对比显示。
    /// </summary>
    public static class CombatPower
    {
        /// <summary>有效生命（物抗/魔抗取平均折算，敌方全物理环境下等价）</summary>
        public static float Ehp(Unit.UnitStats s)
        {
            return s.Hp * (1f + (s.DefP + s.DefM) / 200f);
        }

        /// <summary>战斗输出 = 主攻值 × 攻速</summary>
        public static float Dps(Unit.UnitStats s)
        {
            float atk = Math.Max(s.AtkP, s.AtkM);
            return atk * s.Spd;
        }

        public static float Cp(Unit.UnitStats s)
        {
            return (float)Math.Sqrt(Ehp(s) * Dps(s));
        }
    }
}
