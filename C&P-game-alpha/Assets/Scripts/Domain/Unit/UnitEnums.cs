namespace CnP.Domain.Unit
{
    /// <summary>职能（#D24 职能制；巨蟒走战士源值仅名字区分）</summary>
    public enum UnitRole
    {
        战士,
        坦克,
        射手,
        辅助,
    }

    /// <summary>主攻击类型（#D33：单位级三选一，出生固定；战斗只按主线结算）</summary>
    public enum AttackType
    {
        物理,
        法术,
        无攻击,
    }

    /// <summary>档位（数字 2-10 / 人头 JQK / A）</summary>
    public enum TierKind
    {
        Number,
        Face,
        Ace,
    }
}
