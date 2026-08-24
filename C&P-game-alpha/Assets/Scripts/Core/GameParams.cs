using System.Collections.Generic;

namespace CnP.Core
{
    /// <summary>
    /// P0 全局参数（常量起手，M1 正式任务再迁移 ScriptableObject 配置化）。
    /// 数值来源：design.md 定稿决策 #D33（强度档模板）/ #D36（叠加公式 v2）/ #D37（P0 参数）。
    /// </summary>
    public static class GameParams
    {
        // ── 卡牌基础（#D37）────────────────────────────
        public const int HandStart = 13;      // 起手抽牌数
        public const int HandMax = 13;        // 手牌上限
        public const int PlayRoundsBase = 4;  // 出牌轮次基础值
        public const int DiscardsPerRound = 1; // 每回合弃牌次数
        public const int DiscardSize = 1;     // 每次弃牌张数

        // ── 部署（#D14）────────────────────────────────
        public const int DeployLimit = 36;    // 棋盘部署上限（含英雄/将领/棋子）

        // ── 档位倍率（#D33 §7.4.2）─────────────────────
        // 倍率 = 0.8 + 0.05 ×（点数 − 2）；A = 1.5
        public static float TierMultiplier(int rank)
        {
            if (rank >= 14) return 1.5f; // A
            return 0.8f + 0.05f * (rank - 2);
        }

        // ── 家族修正（#D33 §7.4.3）─────────────────────
        public const float FamilyBasic = 1.0f;        // 单牌/对子/三张（基础线）
        public const float FamilyStraight = 3.0f;     // 顺子
        public const float FamilyBomb = 4.0f;         // 炸弹
        public const float FamilyStraightFlush = 5.0f; // 同花顺

        // ── 特殊召唤数量公式（#D18）────────────────────
        // 顺子/炸弹/同花顺：基准张数 = 1，每 +2 阶翻倍（5 张顺 = 1，13 张 = 16）
        public static int ExponentialCount(int length, int baseLength)
        {
            int steps = (length - baseLength) / 2;
            if (steps < 0) steps = 0;
            return 1 << steps;
        }

        // ── 三带二小增益（占位数值，效果池待内容设计）────
        public const float FullHouseArmorBonus = 2f; // 本场战斗全体护甲 +2

        // ── 职能源值表（#D33 §7.4.1，基准 = 6 点数字档普通兵）──
        // 源值 × 档位倍率 × 家族修正 只作用于 HP/物攻/魔攻/护甲/魔抗；
        // 攻速/射程/移速为手感属性不缩放。
        public class RoleSource
        {
            public float Hp;
            public float Atk;      // 主攻值（副攻击线一律 0）
            public float DefP;     // 护甲
            public float DefM;     // 魔抗
            public float Spd;      // 攻速（次/秒）
            public int Range;      // 射程档（1 近战 / 2 中程 / 3 远程）
            public float Move;     // 移速（格/秒）

            public RoleSource(float hp, float atk, float defP, float defM, float spd, int range, float move)
            {
                Hp = hp; Atk = atk; DefP = defP; DefM = defM; Spd = spd; Range = range; Move = move;
            }
        }

        public static readonly Dictionary<string, RoleSource> RoleSources = new Dictionary<string, RoleSource>
        {
            { "战士", new RoleSource(100f, 10f, 15f, 10f, 1.0f, 1, 2.5f) },
            { "坦克", new RoleSource(150f,  6f, 30f, 20f, 0.8f, 1, 2.0f) },
            { "射手", new RoleSource( 70f, 12f,  5f,  5f, 0.9f, 3, 2.0f) },
            { "辅助", new RoleSource( 80f,  6f, 10f, 15f, 0.8f, 2, 2.2f) }, // 攻击型参考值；无攻击单位主攻取 0
        };

        // ── 敌方强度档（#D33 §7.4.5）───────────────────
        public const float EnemyTier1 = 0.8f;
        public const float EnemyTier2 = 1.2f;
        public const float EnemyTier3 = 1.8f;
        public const float EnemyTier4 = 3.0f;

        // ── 难度曲线与预算（#D34 §10.3.3）──────────────
        // E(关) = 1.5^(关−1)；E(关,R) = E(关) × 回合系数；B = B₀ × E(关,R)
        public const float BaseBudget = 250f;   // B₀ 候选起点（关 1 R1 ≈ 9-10 个 T1）
        public const float RoundCoeffR1 = 1.0f;
        public const float RoundCoeffR2 = 1.5f;
        public const float RoundCoeffR3 = 2.5f;

        public static float DifficultyAt(int level)
        {
            return Mathf_Pow(1.5f, level - 1);
        }

        /// <summary>关内回合系数（R1=1.0 / R2=1.5 / R3=2.5，§10.3.3）</summary>
        public static float RoundCoefficient(int round)
        {
            switch (round)
            {
                case 1: return RoundCoeffR1;
                case 2: return RoundCoeffR2;
                default: return RoundCoeffR3;
            }
        }

        // Domain 层不引用 UnityEngine，用托管实现 pow（底数固定，整数指数）
        private static float Mathf_Pow(float baseValue, int exponent)
        {
            float result = 1f;
            for (int i = 0; i < exponent; i++) result *= baseValue;
            return result;
        }

        // ── 战斗时限（§7.3）────────────────────────────
        public const float BattleTimeLimit = 180f; // 绝对时限
        public const float BattleSilenceLimit = 10f; // 静默结束（双方无伤害后）
    }
}
