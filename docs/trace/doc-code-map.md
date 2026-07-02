# 文档代码映射表

最后更新：2026-07-02

## 映射总览

| 设计文档章节 | 开发文档 | 预计代码文件 |
|--------------|----------|-------------|
| design.md §5.1 牌型规则 | 待创建 dev/card-system.md | Card.cs, Deck.cs, CardService.cs, HandEvaluator.cs |
| design.md §5.2 伤害计算 | 待创建 dev/combat-system.md | CombatService.cs |
| design.md §4.3.4 战斗循环 | architecture.md §3 | GameFlowController.cs (BATTLE_* states) |
| design.md §4.4 商店系统 | 待创建 dev/shop-system.md | ShopService.cs, ShopUI.cs |
| design.md §4.3.1-4.3.5 关卡 | 待创建 dev/stage-system.md | StageService.cs, StageCurve.asset |
| design.md §4.2 初始化/角色状态 | architecture.md §4 | PlayerState.cs, PlayerStateService.cs |

## 变更追踪规则

当设计文档 (design.md) 被修改时：
1. 检查本映射表，找到受影响的设计文档章节
2. 确认对应的开发文档是否需要同步更新
3. 确认对应的代码文件是否需要修改
4. 更新本映射表的版本对应关系

## 版本对应关系

| 设计文档版本 | 开发文档版本 | 代码 Tag | 日期 |
|--------------|--------------|----------|------|
| v0.1 | v0.1 | — | 2026-07-02 |
