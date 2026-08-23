using System;
using CnP.Domain.Card;

namespace CnP.Flow
{
    /// <summary>回合阶段（抽牌并入回合启动自动执行）</summary>
    public enum Phase
    {
        Title,  // 标题
        Play,   // 出牌（含抽牌后的部署交互）
        Battle, // 战斗（自动）
        Settle, // 结算
    }

    /// <summary>
    /// Flow 层事件集中定义（C# event/delegate，UI 只订阅不直改 Domain）。
    /// </summary>
    public static class FlowEvents
    {
        /// <summary>阶段切换</summary>
        public static event Action<Phase> PhaseChanged;

        /// <summary>手牌/选牌变化（UI 重算牌型预览）</summary>
        public static event Action HandChanged;

        /// <summary>一次出牌结算完成（牌型 + 召唤总数）</summary>
        public static event Action<HandPattern, int> PatternPlayed;

        /// <summary>提示条（错误/状态反馈）</summary>
        public static event Action<string> Toast;

        /// <summary>棋盘单位集合变化（生成/死亡/清除）</summary>
        public static event Action BoardChanged;

        /// <summary>战斗结束（玩家是否获胜）</summary>
        public static event Action<bool> BattleEnded;

        // 触发器（Flow 内部调用）
        public static void RaisePhaseChanged(Phase p) => PhaseChanged?.Invoke(p);
        public static void RaiseHandChanged() => HandChanged?.Invoke();
        public static void RaisePatternPlayed(HandPattern p, int summoned) => PatternPlayed?.Invoke(p, summoned);
        public static void RaiseToast(string msg) => Toast?.Invoke(msg);
        public static void RaiseBoardChanged() => BoardChanged?.Invoke();
        public static void RaiseBattleEnded(bool playerWin) => BattleEnded?.Invoke(playerWin);
    }
}
