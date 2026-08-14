# crown&poker-alpha

## 项目概述

单人扑克牌回合制闯关游戏。玩家打出扑克牌组合与敌人对战，逐关推进。游戏玩法类似于"小丑牌"（Balatro），重点是牌型组合和策略构筑。

- **游戏背景**: 流落边疆的年轻王子，通过战胜敌人，从篡位者手中夺回皇位
- **技术栈**: Unity 2022 + C#（从 Godot 4.6 / GDScript 重构）
- **开发者**: William Zhang

## 角色分工

| 角色 | 职责 | 工具 |
|------|------|------|
| 张总 | 唯一决策者、审批人、审核者 | 人工 |
| Codex | 项目管理 + 技术开发 + 质量把关（三位一体） | CLI + Unity Editor + MCP |

> 2026-08-07 起由 Codex 单一智能体承接项目全部 AI 工作（原 WorkBuddy + Trae 职责合并，决策 #D10）。
> 旧协作体系文件已归档至 `docs/archive/`。

## 文档导航

### 必读 — AI 启动时按此顺序读取

| 文件 | 用途 | 自动注入 |
|------|------|----------|
| `PROJECT_RULES.md` | AI 行为准则（Codex 单一智能体版） | ❌ |
| `.workbuddy/memory/MEMORY.md` | 项目长期记忆 + 技术偏好 | ✅ |
| `.workbuddy/session/CONTEXT.md` | 当前上下文快照 | ❌ |
| `.workbuddy/tasks/tasks.md` | 任务看板 | ❌ |

### 设计 — 做什么

| 文件 | 用途 |
|------|------|
| `docs/design/design.md` | 游戏设计总纲 |
| `docs/design/派系兵种设计.md` | 派系兵种映射表（帝国 14 兵种） |
| `docs/research/` | 设计研究资料（牌型难度 CSV、概率衰减拟合） |

### 开发 — 怎么做

| 文件 | 用途 |
|------|------|
| `docs/dev/architecture.md` | 技术架构文档 |

### 管理 — 怎么管

| 文件 | 用途 |
|------|------|
| `.workbuddy/memory/decisions.md` | 技术决策记录 |
| `docs/trace/doc-code-map.md` | 文档代码映射表 |
| `docs/CHANGELOG.md` | 项目变更日志 |

### 运维 — 参考

| 文件 | 用途 |
|------|------|
| `.gitignore` | Git 忽略规则 |
| `docs/archive/` | 废弃文档归档 |

## 依赖规则

项目严格遵守以下自上而下的依赖链：

```
设计文档 (docs/design/design.md)
    → 开发文档 (docs/dev/architecture.md)
        → 代码实现 (Assets/Scripts/**/*.cs)
```

不得跳过层级或逆向依赖。所有开发工作必须先有文档再有代码。
