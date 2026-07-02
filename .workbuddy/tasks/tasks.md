# 任务看板

最后更新：2026-07-02 18:05

## 任务状态说明

| 状态 | 含义 |
|------|------|
| 待开始 | 尚未开始 |
| 进行中 | 正在开发 |
| 待审核 | 代码已完成，等张总审核 |
| 已完成 | 已审核通过 |
| 阻塞 | 被依赖项阻塞（注明原因） |
| 取消 | 不再需要 |

---

## 里程碑 M0: 项目初始化（进行中）

目标：项目管理机制搭建 + 设计文档迁移 + 技术架构确立

| ID | 任务 | 状态 | 平台 | 预计文件 | Commit | 备注 |
|----|------|------|------|----------|--------|------|
| M0-01 | 创建项目管理基础设施 | 进行中 | WB | 15 个文档/配置文件 | — | 当前任务 |
| M0-02 | 迁移设计文档 | 待开始 | WB | docs/design/design.md | — | 从 c&pv-1 迁移 |
| M0-03 | 编写 Unity 版技术架构 | 待开始 | WB | docs/dev/architecture.md | — | 参考旧项目 |
| M0-04 | Git 初始化 | 待开始 | Manual | — | — | git init + 首次提交 |
| M0-05 | 创建 Unity 项目 | 待开始 | Trae | Unity 项目文件 | — | Unity 2022 Hub |

---

## 里程碑 M1: 核心系统

目标：Card 模型 + 事件系统 + 配置系统 + PlayerState

| ID | 任务 | 状态 | 平台 | 预计文件 | Commit | 备注 |
|----|------|------|------|----------|--------|------|
| M1-01 | Card 模型 + Deck | 待开始 | Trae | Card.cs, Deck.cs | — | 参考旧 card.gd, deck.gd |
| M1-02 | EventContracts 定义 | 待开始 | Trae | EventContracts.cs | — | 参考旧 event_contracts.gd |
| M1-03 | CardService 实现 | 待开始 | Trae | CardService.cs | — | 抽牌/换牌/出牌 |
| M1-04 | PlayerState 模型 | 待开始 | Trae | PlayerState.cs | — | HP/金币/换牌次数 |
| M1-05 | 配置系统（ScriptableObject） | 待开始 | Trae | *.asset 文件 | — | 替代 JSON 配置 |

---

## 里程碑 M2: 战斗系统

目标：8 种牌型判定 + 伤害计算 + 敌人 AI

| ID | 任务 | 状态 | 平台 | 预计文件 | Commit | 备注 |
|----|------|------|------|----------|--------|------|
| M2-01 | 牌型判定逻辑 | 待开始 | Trae | HandEvaluator.cs | — | 8 种牌型 |
| M2-02 | CombatService | 待开始 | Trae | CombatService.cs | — | 攻击结算 |
| M2-03 | StageService | 待开始 | Trae | StageService.cs | — | 关卡敌人生成 |
| M2-04 | 战斗场景 UI | 待开始 | Trae | BattleScene.unity | — | Unity 场景 |

---

## 里程碑 M3: 游戏流程

目标：GameFlowController 状态机 + UI 交互

| ID | 任务 | 状态 | 平台 | 预计文件 | Commit | 备注 |
|----|------|------|------|----------|--------|------|
| M3-01 | GameFlowController | 待开始 | Trae | GameFlowController.cs | — | 状态机 |
| M3-02 | 主菜单场景 | 待开始 | Trae | MainMenu.unity | — | — |
| M3-03 | 战斗 UI 交互 | 待开始 | Trae | BattleUI.cs + Scene | — | — |

---

## 里程碑 M4: 商店与进度

目标：ShopService + 通关判断 + 金币结算

| ID | 任务 | 状态 | 平台 | 预计文件 | Commit | 备注 |
|----|------|------|------|----------|--------|------|
| M4-01 | ShopService | 待开始 | Trae | ShopService.cs | — | — |
| M4-02 | 商店 UI | 待开始 | Trae | ShopScene.unity | — | — |
| M4-03 | 存档系统 | 待开始 | Trae | SaveManager.cs | — | PlayerPrefs 或文件 |

---

## 里程碑 M5: 打磨与发布

目标：平衡性调优 + 音效/特效 + 打包

| ID | 任务 | 状态 | 平台 | 预计文件 | Commit | 备注 |
|----|------|------|------|----------|--------|------|
| M5-01 | 数值平衡 | 待开始 | Trae | SO configs | — | 关卡难度曲线 |
| M5-02 | 音效/特效 | 待开始 | Trae | Asset 导入 | — | — |
| M5-03 | 构建测试 | 待开始 | Manual | — | — | — |
