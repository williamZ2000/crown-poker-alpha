using CnP.Domain.Unit;
using NUnit.Framework;

namespace CnP.Tests
{
    public class RoleSourceMapTests
    {
        /// <summary>TD-002 完整性兜底：新增 UnitRole 枚举值而漏配源值表时，此测试红</summary>
        [Test]
        public void 全职能可查_源值表完整()
        {
            foreach (UnitRole role in System.Enum.GetValues(typeof(UnitRole)))
            {
                var src = RoleSourceMap.Get(role);
                Assert.NotNull(src, "职能 " + role + " 缺源值配置");
                Assert.Greater(src.Hp, 0f, role + " HP 应为正");
                Assert.Greater(src.Atk, 0f, role + " Atk 应为正");
            }
        }
    }
}
