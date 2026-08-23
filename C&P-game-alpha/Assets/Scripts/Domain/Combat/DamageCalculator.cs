using System;

namespace CnP.Domain.Combat
{
    /// <summary>
    /// 伤害公式（#D13 百分比减免，参照 LoL 护甲机制）：
    /// 物理 = 物攻 × 100/(100+护甲)；法术 = 魔攻 × 100/(100+魔抗)。
    /// 防御永不免疫；负防御第一版钳制为 0；穿透/真实伤害不进第一版。
    /// </summary>
    public static class DamageCalculator
    {
        public static float PhysicalDamage(float atkP, float defP)
        {
            defP = Math.Max(0f, defP);
            return atkP * 100f / (100f + defP);
        }

        public static float MagicDamage(float atkM, float defM)
        {
            defM = Math.Max(0f, defM);
            return atkM * 100f / (100f + defM);
        }

        /// <summary>按攻击方主攻击类型走对应线（#D33：战斗只按主线结算）</summary>
        public static float Damage(Unit.UnitStats attacker, Unit.UnitStats defender)
        {
            switch (attacker.Attack)
            {
                case Unit.AttackType.物理: return PhysicalDamage(attacker.AtkP, defender.DefP);
                case Unit.AttackType.法术: return MagicDamage(attacker.AtkM, defender.DefM);
                default: return 0f; // 无攻击单位
            }
        }
    }
}
