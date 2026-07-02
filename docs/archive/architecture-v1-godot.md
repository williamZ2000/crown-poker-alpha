# 技术架构（Unity 2022 + C# 版）

## 1. 分层结构

沿用 Godot 版本的四层架构，映射到 Unity C# 环境：

```
Assets/Scripts/
├── UI/              ← 表现层：Unity UI 组件、动画、输入处理
├── Flow/            ← 应用层：GameFlowController 状态机编排主循环
├── Domain/          ← 领域层：纯 C# 业务逻辑（不依赖 UnityEngine）
│   ├── Models/      ← 数据模型（Card, PlayerState, Enemy...）
│   └── Services/    ← 领域服务（CardService, CombatService...）
└── Core/            ← 基础设施层：事件契约定义、配置加载
```

**依赖规则**：上层依赖下层，下层不依赖上层。Domain 层不使用 Unity 场景节点，确保纯逻辑可测试和可复用。

## 2. 事件通信方案

**方案**：使用 C# 原生 `event` / `delegate`，非全局单例。

`Core/EventContracts` 中定义标准委托类型和事件数据结构，各服务暴露自己的 event。`GameFlowController` 订阅各服务的 event 来驱动状态流转。

**关键事件列表**（对应旧项目 EventBus 12 个信号）：

- OnRoundStarted
- OnCardsDrawn
- OnCardsSwitched
- OnPlayAttempted
- OnPlayValidated
- OnDamageResolved
- OnEnemyActed
- OnBattleEnded
- OnRewardsGranted
- OnShopOpened
- OnPurchaseCompleted
- OnStageAdvanced

## 3. 主循环状态机

`Flow/GameFlowController` 实现以下状态机（等待式，等待 UI 输入或事件触发后再切换状态，非自动顺序触发）：

1. `INIT` 初始化
2. `STAGE_SETUP` 关卡载入
3. `PLAYER_RESET` 重置玩家状态
4. `PLAYER_DRAW` 玩家抽牌
5. `BATTLE_ROUND_START` 回合开始
6. `PLAYER_SWITCH` 玩家换牌（等待 UI 操作）
7. `PLAYER_PLAY` 玩家出牌（等待 UI 操作）
8. `PLAYER_PLAY_VALIDATE` 出牌校验
9. `PLAYER_ATTACK_RESOLVE` 攻击结算
10. `PLAYER_WIN_CHECK` 玩家胜利判断
11. `ENEMY_ATTACK` 敌人攻击
12. `PLAYER_FAIL_CHECK` 玩家失败判断
13. `BATTLE_POST` 战斗后记录
14. `STAGE_ADVANCE` 更新关卡进度
15. `CLEAR_CHECK` 通关判断
16. `REWARD_SETTLE` 金币结算
17. `DECK_RESTORE` 重置牌堆
18. `SHOP` 商店系统（等待 UI 操作）
19. `GAME_END` 游戏结束

## 4. 领域服务接口

- `CardService`：抽牌、换牌、出牌、牌堆恢复。
- `CombatService`：出牌校验（8种牌型判定）、玩家攻击结算、敌人攻击结算。
- `StageService`：关卡敌人生成、通关判断。
- `ShopService`：商店打开、购买、刷新。
- `PlayerStateService`：创建 PlayerState、关卡重置、应用角色加成。

## 5. 配置系统

**方案**：Unity ScriptableObject（非 JSON）

| 配置项 | ScriptableObject | 对应旧项目 JSON |
|--------|-----------------|------------------|
| 扑克牌基础 | CardConfig.asset | cards/base_deck.json |
| 玩家默认属性 | PlayerDefaults.asset | player/player_defaults.json |
| 商店商品 | ShopCatalog.asset | shop/shop_catalog.json |
| 关卡难度曲线 | StageCurve.asset | stages/stage_curve.json |

**要求**：
- 可调参数优先 ScriptableObject 配置化，不硬编码。
- 配置缺失时使用默认值，避免 NullReferenceException。
- 后续如需 JSON 导入导出，可额外添加 Converter 工具类。
