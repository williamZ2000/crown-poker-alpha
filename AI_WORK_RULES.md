# AI 行为准则

> 本文档同时约束 WorkBuddy 和 Trae 两个 AI 平台。
> 违反本文档中的核心规则 = 严重违规。

---

## 1. 决策权归张总（优先级最高）

任何修改文件的操作前，AI 必须执行以下五步闭环：

```
1. 出方案  → 说明：改什么文件、改什么内容、为什么改、影响范围
2. 列清单  → 给出本次改动的完整文件清单
3. 等审批  → 张总明确说"OK/同意/执行"之前，不动任何文件
4. 执行    → 严格按审批通过的方案执行，不做额外改动
5. 汇报    → 逐项对照方案确认完成，附变更摘要
```

**此规则优先级高于所有其他规则。**

---

## 2. 分工铁律

| AI 平台 | 可以操作 | 禁止操作 |
|---------|----------|----------|
| WorkBuddy | .md 文档、项目管理文件、方案设计 | .cs 代码、Unity 场景/资源、MCP 调用 |
| Trae | .cs 代码、.unity 场景、.asset 配置、MCP | 设计文档、项目管理文件、架构决策 |

---

## 3. 启动必读

每次会话开始，AI 必须按以下顺序读取文件：

```
1. AI_WORK_RULES.md          ← 本文档（行为准则）
2. COLLABORATION.md          ← 协作协议
3. .workbuddy/memory/MEMORY.md   ← 项目长期记忆 + 核心铁律
4. .workbuddy/session/CONTEXT.md ← 最新上下文
5. TASK_CLAIM.md             ← 当前任务声明
```

---

## 4. 决策留痕与联动更新

每个重要决策必须完整留痕，通过以下文件形成闭环：

| 步骤 | 文件 | 操作 |
|------|------|------|
| 1 | `.workbuddy/memory/decisions.md` | 追加决策记录（日期/背景/决策/原因/影响） |
| 2 | 受影响的设计/开发文档 | 同步更新相关内容 |
| 3 | `docs/CHANGELOG.md` | 追加变更条目（关联决策编号） |
| 4 | `.workbuddy/session/CONTEXT.md` | 更新"最近决策"摘要 |
| 5 | `docs/trace/doc-code-map.md` | 确认映射关系仍正确 |

---

## 5. 最小修改原则

- 能用最小的代码/文档更改解决问题就不做大改
- 不重构不需要重构的代码
- 不创建不需要的文件

---

## 6. 代码注释（Trae）

- 所有 .cs 文件必须包含适当注释，说明类/方法的用途
- 注释使用中文
- 命名使用 PascalCase（C# 惯例）

---

## 7. 不确定就反问

- 不猜测张总的意图
- 不替张总做决定
- 遇到歧义、缺失信息、多种可行方案时，先问再动

---

## 8. Git 提交规范（Trae）

格式：`[平台标识] 类型(范围): 描述 (任务ID)`

示例：
```
[Trae] feat(card): implement Card model with suit/rank/score (M1-01)
[Trae] feat(combat): implement hand evaluation for all 8 combos (M2-01)
[Trae] fix(ui): correct card display alignment in battle scene
```

---

## 9. 文档-代码依赖链

```
设计文档 (docs/design/*.md)
    → 开发文档 (docs/dev/*.md)
        → 代码实现 (Assets/Scripts/**/*.cs)
```

- 写代码前，确认对应的开发文档已存在
- 开发文档不存在 → 标记阻塞，通知张总
- 不能在开发文档中写示例代码（只应描述方案和规范）
- 代码实现不能偏离开发文档的描述
