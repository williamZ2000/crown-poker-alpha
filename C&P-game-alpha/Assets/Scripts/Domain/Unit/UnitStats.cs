using System;
using System.Collections.Generic;

namespace CnP.Domain.Unit
{
    /// <summary>
    /// 棋子属性集（8 属性，#D12/#D33）+ 单位元信息。
    /// 档位倍率/家族修正只作用于 HP/物攻/魔攻/护甲/魔抗；攻速/射程/移速为手感属性不缩放。
    /// </summary>
    public class UnitStats
    {
        public string Name;          // 兵种名（如 帝国步兵）
        public UnitRole Role;        // 职能
        public AttackType Attack;    // 主攻击类型
        public string Family;        // 家族标签（基础/顺子/炸弹/同花顺，显示用）

        public float Hp;      // 生命值
        public float AtkP;    // 物理攻击
        public float AtkM;    // 法术攻击
        public float DefP;    // 护甲
        public float DefM;    // 魔抗
        public float Spd;     // 攻速（次/秒）
        public int Range;     // 射程档（1 近战 / 2 中程 / 3 远程）
        public float Move;    // 移速（格/秒）

        public int TierRank;  // 判档点数（2~14，显示用）

        public override string ToString()
        {
            return Name + "（" + Role + "·" + TierRank + "档） " +
                   "HP" + Hp.ToString("0") + " 攻" + (Attack == AttackType.法术 ? AtkM.ToString("0") + "魔" : AtkP.ToString("0")) +
                   " 甲" + DefP.ToString("0") + " 抗" + DefM.ToString("0") +
                   " 速" + Spd.ToString("0.0") + " 程" + Range;
        }
    }
}
