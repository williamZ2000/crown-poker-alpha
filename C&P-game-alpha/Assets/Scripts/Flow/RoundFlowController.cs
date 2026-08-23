using System.Collections.Generic;
using CnP.Core;
using CnP.Domain.Card;
using CnP.Domain.Unit;
using UnityEngine;

namespace CnP.Flow
{
    /// <summary>
    /// 回合流控制器（四阶段状态机：抽牌并入启动自动执行 → 出牌 → 战斗 → 结算）。
    /// 本切片 = 关卡 1-1 单回合；商店/经济/多关循环后置。
    /// </summary>
    public class RoundFlowController : MonoBehaviour
    {
        public static RoundFlowController Instance { get; private set; }

        public CardSystem Cards { get; } = new CardSystem();
        public BoardSystem Board { get; } = new BoardSystem();

        public Phase Phase { get; private set; } = Phase.Title;

        /// <summary>已用出牌轮次（基础 4 轮，#D37）</summary>
        public int RoundsUsed { get; private set; }
        public int RoundsLeft => GameParams.PlayRoundsBase - RoundsUsed;

        /// <summary>三带二小增益待生效（本场战斗全体护甲 +2，#D18；战斗引擎开局应用后置回 false）</summary>
        public bool FullHouseBuffPending { get; set; }

        void Awake()
        {
            Instance = this;
        }

        /// <summary>开始新一回合（关卡 1-1）：重置全部状态 → 抽起手 13 张 → 出牌阶段</summary>
        public void StartNewRound()
        {
            Board.Clear();
            Cards.StartNewRound();
            RoundsUsed = 0;
            FullHouseBuffPending = false;
            SetPhase(Phase.Play);
            FlowEvents.RaiseToast("关卡 1-1 · 出牌阶段：点选手牌凑牌型，随时可开战");
        }

        /// <summary>出牌：判定 → 校验部署上限 → 召唤（消耗 1 轮）</summary>
        public bool TryPlayCurrentPattern()
        {
            if (Phase != Phase.Play) return false;
            var pattern = Cards.CurrentPattern;
            if (pattern == null)
            {
                FlowEvents.RaiseToast("牌型无效：选中牌须恰好构成一个牌型");
                return false;
            }
            if (RoundsLeft <= 0)
            {
                FlowEvents.RaiseToast("出牌轮次已用完（基础 4 轮）");
                return false;
            }

            var batches = UnitFactory.CreateSummons(pattern);
            int totalCount = 0;
            foreach (var b in batches) totalCount += b.Count;

            if (!Board.CanDeploy(totalCount))
            {
                FlowEvents.RaiseToast("棋盘已满（36 上限），支援区后置版本暂不可超额部署");
                return false;
            }

            if (!Cards.PlaySelected(out var played)) return false;

            Board.SpawnPlayerUnits(batches);
            if (played.Kind == PatternKind.FullHouse) FullHouseBuffPending = true;

            RoundsUsed++;
            FlowEvents.RaisePatternPlayed(played, totalCount);
            return true;
        }

        /// <summary>弃牌换抽（出牌阶段内随时，1 次 × 1 张）</summary>
        public bool DiscardCard(CardModel card)
        {
            if (Phase != Phase.Play) return false;
            if (Cards.DiscardsLeft <= 0)
            {
                FlowEvents.RaiseToast("本回合弃牌次数已用完");
                return false;
            }
            if (Cards.DiscardAndDraw(card))
            {
                FlowEvents.RaiseToast("弃 1 张，补抽 1 张");
                return true;
            }
            return false;
        }

        /// <summary>开战（玩家随时可点）→ 战斗阶段（S5 起敌军生成在此前接入）</summary>
        public void RequestStartBattle()
        {
            if (Phase != Phase.Play) return;
            Cards.ClearSelection();
            SetPhase(Phase.Battle);
            if (CombatSystem.Instance != null)
                CombatSystem.Instance.Begin();
        }

        /// <summary>战斗结束（S4 战斗引擎回调）→ 结算阶段</summary>
        public void OnBattleEnded(bool playerWin)
        {
            SetPhase(Phase.Settle);
            FlowEvents.RaiseBattleEnded(playerWin);
        }

        /// <summary>回标题（清场）</summary>
        public void ReturnToTitle()
        {
            Board.Clear();
            SetPhase(Phase.Title);
        }

        void SetPhase(Phase p)
        {
            Phase = p;
            FlowEvents.RaisePhaseChanged(p);
        }
    }
}
