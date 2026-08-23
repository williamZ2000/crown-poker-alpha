using System.Collections.Generic;

namespace CnP.Domain.Unit
{
    /// <summary>
    /// 帝国 v1 兵种目录（14 条目，#D24-#D27 定稿；派系兵种设计.md §2）。
    /// 键 =（家族, 档位）；顺子/同花顺三合一（不分档，数值按最高点数判档）。
    /// </summary>
    public static class UnitCatalog
    {
        public enum Family
        {
            单牌,
            对子,
            三张,
            顺子,
            炸弹,
            同花顺,
        }

        class Entry
        {
            public string Name;
            public UnitRole Role;
            public AttackType Attack;
            public Entry(string name, UnitRole role, AttackType attack)
            {
                Name = name; Role = role; Attack = attack;
            }
        }

        // 基础线（数字档, 人头档, A 档）+ 特殊线（三合一单条目）
        static readonly Dictionary<Family, Entry[]> _table = new Dictionary<Family, Entry[]>
        {
            { Family.单牌, new[] { new Entry("征召民兵", UnitRole.战士, AttackType.物理),
                                   new Entry("猎人",     UnitRole.射手, AttackType.物理),
                                   new Entry("征召官",   UnitRole.战士, AttackType.物理) } },
            { Family.对子, new[] { new Entry("帝国步兵", UnitRole.坦克, AttackType.物理),
                                   new Entry("帝国弩手", UnitRole.射手, AttackType.物理),
                                   new Entry("帝国队长", UnitRole.坦克, AttackType.物理) } },
            { Family.三张, new[] { new Entry("帝国重盾", UnitRole.坦克, AttackType.物理),
                                   new Entry("帝国牧师", UnitRole.辅助, AttackType.法术),
                                   new Entry("禁卫军士", UnitRole.坦克, AttackType.物理) } },
            { Family.顺子, new[] { new Entry("帝国铁骑", UnitRole.战士, AttackType.物理), null, null } },
            { Family.炸弹, new[] { new Entry("叶皇之刃",   UnitRole.战士, AttackType.物理),
                                   new Entry("帝国之心",   UnitRole.辅助, AttackType.法术),
                                   new Entry("皇家骑士团", UnitRole.战士, AttackType.法术) } },
            { Family.同花顺, new[] { new Entry("斯维尔尼斯巨蟒", UnitRole.战士, AttackType.物理), null, null } },
        };

        /// <summary>取兵种条目（顺子/同花顺三合一，任意档位返回同一条目）</summary>
        public static (string name, UnitRole role, AttackType attack) Get(Family family, TierKind tier)
        {
            var entries = _table[family];
            Entry e;
            if (entries[0] != null && entries[1] == null) e = entries[0]; // 三合一
            else e = entries[(int)tier];
            return (e.Name, e.Role, e.Attack);
        }
    }
}
