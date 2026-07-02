# 跨平台协作协议

## 角色分工

| 角色 | 技能 | 禁止 |
|------|------|------|
| 张总 | 决策者、审核者 | — |
| WorkBuddy | 项目管理、文档编写、方案设计、进度追踪 | 不写 .cs 代码、不操作 Unity、不调用 MCP |
| Trae | C# 代码编写、Unity 场景编辑、MCP 实施 | 不修改设计文档、不编写项目管理文件、不做方案决策 |

## 会话启动协议（每次启动必须执行）

```
1. 按顺序读取：
   AI_WORK_RULES.md → COLLABORATION.md → .workbuddy/memory/MEMORY.md
   → .workbuddy/session/CONTEXT.md → TASK_CLAIM.md
2. git pull（拉取对方的最新变更）
3. 开始工作
```

## 会话结束协议（每次结束前必须执行）

```
1. 更新 TASK_CLAIM.md（释放已完成的任务）
2. git add + git commit（符合提交规范）
3. 更新 .workbuddy/session/CONTEXT.md
4. 如有可能：git push
```

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
