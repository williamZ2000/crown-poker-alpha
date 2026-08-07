# 跨平台协作协议

## 角色分工

| 角色 | 职责 | 禁止 |
|------|------|------|
| 张总 | 决策者、审核者、两个平台的唯一连接点 | — |
| WorkBuddy | 项目管理、文档编写、方案设计、进度追踪、**代码审核与验收** | 不写 .cs 代码、不操作 Unity、不调用 MCP |
| Trae | C# 代码编写、Unity 场景编辑、MCP 实施 | 不修改设计文档、不编写项目管理文件、不做方案决策 |

## 会话启动协议（每次启动必须执行）

```
1. 按顺序读取：
   AI_WORK_RULES.md → COLLABORATION.md → .workbuddy/memory/MEMORY.md
   → .workbuddy/session/CONTEXT.md → TASK_CLAIM.md
2. git pull（拉取对方的最新变更）
3. 开始工作
```

## 任务完成协议（操作即更新）

AI 不会主动感知"会话结束"，因此不需要依赖会话结束来更新状态。改为即时更新：

- **WorkBuddy**：每完成一个操作后，立即更新相关文档（TASK_CLAIM.md、CONTEXT.md、CHANGELOG.md 等）
- **Trae**：每次编码任务完成后，立即 commit（带任务 ID），并主动提醒张总审核
- **WorkBuddy 启动时**：自动检查 Git log，与 tasks.md 比对，发现已完成但未审核的 [Trae] commit 时提醒张总

## 状态同步机制

Trae 和 WorkBuddy 之间没有直接通信通道，张总是唯一连接点。

```
Trae 完成编码 → commit [Trae] feat: ... (Mx-xx) → 提醒张总
    ↓
张总打开 WorkBuddy → 「审核 Mx-xx」
    ↓
WorkBuddy 执行审核 → 更新 tasks.md 状态

兜底：下次 WB 启动时自动扫描 Git log，发现遗漏提醒张总
```

## 代码审核流程

1. 张总通知 WorkBuddy 审核指定任务
2. WorkBuddy 读取 Git log 找到对应 [Trae] commit
3. WorkBuddy 读取代码，对照开发文档逐项检查
4. WorkBuddy 产出审核报告：
   - 通过 → 更新 `tasks.md`（状态→已完成）、`doc-code-map.md`、`decisions.md`
   - 不通过 → 列出问题清单，`tasks.md` 保持「待审核」，等 Trae 修复后重新审核

## 冲突避免

- **WorkBuddy 只改文档类文件**（.md、.gitignore、.workbuddy/ 下内容）
- **Trae 只改代码类文件**（.cs、.unity、.asset、.prefab）
- 两个平台的修改范围天然不重叠，几乎不会发生冲突
- 唯一重叠点 `TASK_CLAIM.md` 和 `CONTEXT.md`：Trae 只读不写；WorkBuddy 负责维护

## 冲突解决

如出现意外冲突（极少情况）：
1. Git merge conflict → 标记为 [CONFLICT]，张总介入
2. TASK_CLAIM.md 冲突 → 以 WorkBuddy 版本为准
3. 紧急通知 → 在 CONTEXT.md 顶部写 URGENT 标记

## Git 提交规范

格式：`[平台标识] 类型(范围): 描述`

| 元素 | 可选值 |
|------|--------|
| 平台标识 | `[WB]` / `[Trae]` / `[Manual]` |
| 类型 | `feat` / `fix` / `docs` / `refactor` / `config` / `chore` |
| 范围 | `card` / `combat` / `shop` / `stage` / `ui` / `core` / `project` |

示例：
```
[WB] docs: update combat system design §5.2
[Trae] feat(card): implement Card model (M1-01)
[Manual] config: update Unity project settings
```
