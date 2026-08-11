# 技术架构文档

> 版本：v1.0（2026-08-07）
> 状态：已定稿（M0-04）
> 技术栈：Unity 2022.3 LTS（URP 2D）+ C#
> 参考旧版：`docs/archive/architecture-v1-godot.md`（Godot 4.6 版本，已归档）

## 1. 总体架构

采用 **4 层事件驱动架构**：Core → Domain → Flow → UI，依赖严格单向，禁止跨层反向引用。

```
┌──────────────────────────────┐
│  UI 表现层（渲染/输入/动画） │  ← 最外层，只订阅 Flow 事件
├──────────────────────────────┤
│  Flow 游戏系统层（流程/规则） │  ← 依赖 Domain，协调系统
├──────────────────────────────┤
│  Domain 领域层（纯逻辑/模型） │  ← 无 Unity 依赖，可单测
├──────────────────────────────┤
│  Core 基础设施层（工具/事件） │  ← 最底层，被各层使用
└──────────────────────────────┘
```

### 依赖规则

| 层 | 可以依赖 | 禁止依赖 |
|----|----------|----------|
| UI | Flow、Domain、Core | 无 |
| Flow | Domain、Core | UI |
| Domain | Core（少量工具） | UI、Flow |
| Core | 无（或 Unity API） | 业务层 |

> 关键约束：**UI 不能直接调用 Domain 的修改逻辑**，必须通过 Flow 的系统接口；Domain 不引用 UnityEngine（保持可测试性），纯数据与纯函数放这里。

## 2. 目录结构

```
Assets/
├── Scripts/
│   ├── Core/        # 基础设施：事件定义、工具类、单例基类、配置加载器
│   ├── Domain/      # 纯逻辑：卡牌模型、牌型判定、棋子属性、伤害计算、经济模型
│   ├── Flow/        # 游戏系统：回合流程、战斗系统、商店、经济、敌人AI、英雄技能
│   ├── UI/          # 表现层：卡牌 UI、棋盘渲染、HUD、商店界面、拖拽交互
│   └── Config/      # ScriptableObject 定义：配置资产的 C# 类型声明
├── Data/            # 配置资产（.asset 实例）：牌堆/兵种/派系/英雄/Buff/关卡模板
├── Scenes/          # Unity 场景
├── Prefabs/         # 预制体（棋子、卡牌、特效等）
└── Art/             # 美术资源（贴图、材质、动画）
```

## 3. 核心设计决策

### 3.1 事件通信

- 使用 **C# event/delegate**，不用全局单例 EventBus
- 数据流向：Domain/Flow 产生事件 → UI 订阅展示
- 事件定义集中在 Core/Events，按系统分组（如 `CardEvents`、`CombatEvents`、`EconomyEvents`）

### 3.2 配置系统（ScriptableObject）

- 所有可调数值与内容配置使用 ScriptableObject，不硬编码
- 配置类型定义在 `Scripts/Config/`，具体资产实例放在 `Assets/Data/`
- 核心配置资产清单：

| 配置资产 | 对应设计章节 | 说明 |
|----------|--------------|------|
| 牌堆配置 | §3 | 初始 52 张牌结构、可扩充内容 |
| 兵种模板 | §3.1 / §7.3 | 6 属性（HP/ATK/DEF/SPD/RNG/MOV）数值模板 |
| 牌型定义 | §3.2 | 牌型判定条件与召唤映射 |
| 派系配置 | §5 | 兵种池、被动加成、偏好花色、专属英雄 |
| 英雄配置 | §6 | 属性基准、技能槽、技能池 |
| Buff 定义 | §4.4 | 双载体（将领/装备）、四阶段作用 |
| 关卡模板 | §10 | 三回合结构、敌人配置模板、随机变量范围 |
| 经济参数 | §9 | 利息率、上限、商品价格体系 |

### 3.3 回合流程（Flow 层核心）

对应设计 §2.1 四阶段，由 Flow 层 `RoundFlowController` 驱动：

```
抽牌(自动) → 出牌(多轮,玩家操作) → 战斗(自动) → 结算(展示结果)
```

各阶段职责：
| 阶段 | 负责系统 | 主要事件 |
|------|----------|----------|
| 抽牌 | CardSystem | 补满手牌至上限、弃牌换抽 |
| 出牌 | CardSystem + BoardSystem | 牌型判定、召唤棋子、站位调整、开战 |
| 战斗 | CombatSystem | 索敌/移动/攻击、技能触发、时限检测 |
| 结算 | EconomySystem + MetaSystem | 胜负判定、金币/利息结算、升级 |

### 3.4 关键系统模块（M1 起逐步实现）

| 模块 | 所在层 | 说明 | 关联设计 |
|------|--------|------|----------|
| CardSystem | Flow | 手牌/牌堆/牌型判定调度 | §2.1, §3 |
| BoardSystem | Flow | 棋盘部署、支援区、阵型 | §3.4, §7.1-7.2 |
| CombatSystem | Flow | 自走棋战斗循环、AI、时限 | §7.3, §10.2 |
| EconomySystem | Flow | 金币、利息、结算 | §9 |
| ShopSystem | Flow | 商品生成、购买、刷新 | §8 |
| HeroSystem | Flow | 英雄属性、技能槽、成长 | §6 |
| BuffSystem | Flow | 四阶段 Buff 叠加 | §4.4 |
| MetaSystem | Flow | 局外成长、解锁、难度 | §2.4 |
| EnemyAISystem | Flow | 敌人配置生成与行为 | §10.2 |

### 3.5 初始化流程

1. 启动场景加载 `Bootstrap`（Core 层）：加载全部 ScriptableObject 配置
2. 创建各 Flow 系统并注册事件
3. 进入主菜单 → 派系/英雄选择 → 关卡循环（RoundFlowController）

## 4. 技术选型与约束

| 项 | 选择 | 理由 |
|----|------|------|
| Unity 版本 | 2022.3 LTS | 项目已创建，URP 2D |
| 渲染管线 | URP 2D Renderer | 2D 游戏，灯光/后效支持好 |
| 配置 | ScriptableObject | Schema 约束 + Inspector 可视化（MEMORY 技术偏好） |
| 事件 | C# event/delegate | 无全局单例，可追踪 |
| 测试 | Unity Test Framework | Domain 层纯逻辑优先覆盖单测 |
| 代码注释 | 中文 | 项目规范 |
| 命名 | PascalCase | C# 惯例 |

## 5. 未决事项（等待设计收敛）

| 事项 | 影响 |
|------|------|
| 兵种数值模板 | 影响 CombatSystem 数值平衡 |
| 派系 IP 设定（M0-03h 阻塞） | 影响派系配置资产与内容 |

棋盘方案已定稿为方案 A（RTS 自由部署，#D14）；架构按方案 A 预留扩展点，棋盘渲染与布阵交互由 BoardSystem 与 UI 层承担。
