using System;
using CnP.Core;

namespace CnP.Domain.Unit
{
    /// <summary>
    /// 职能 → 职能源值 的编译期映射（技术债偿还 TD-002：替代 RoleSources[enum.ToString()]
    /// 与 Enum.Parse 字符串互转——拼写错误从运行时 KeyNotFoundException 提前到编译期；
    /// 完整性由单测枚举全部枚举值兜底）。
    /// </summary>
    public static class RoleSourceMap
    {
        public static GameParams.RoleSource Get(UnitRole role)
        {
            switch (role)
            {
                case UnitRole.战士: return GameParams.RoleSources["战士"];
                case UnitRole.坦克: return GameParams.RoleSources["坦克"];
                case UnitRole.射手: return GameParams.RoleSources["射手"];
                case UnitRole.辅助: return GameParams.RoleSources["辅助"];
                default: throw new ArgumentOutOfRangeException(nameof(role), role, "职能源值表缺失该职能：请同时补 GameParams.RoleSources 与本映射");
            }
        }
    }
}
