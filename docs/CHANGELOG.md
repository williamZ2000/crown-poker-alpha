# 项目变更日志

## 2026-07-02

- **项目初始化** — 创建项目管理基础设施（15 个文件）
  - 协作协议（COLLABORATION.md、AI_WORK_RULES.md、TASK_CLAIM.md）
  - 项目记忆（MEMORY.md、decisions.md、CONTEXT.md）
  - 任务看板（tasks.md，含 M0-M5 里程碑）
  - 设计文档迁移（design.md、architecture.md）
  - 文档代码映射（doc-code-map.md）
  - 关联决策: #D01

- **角色分工细化** — WB 新增代码审核职责，取消"会话结束"依赖
  - 审核流程：Trae commit → 张总通知 → WB 审核 → 更新状态
  - 即时更新 + 启动扫描兜底
  - 关联决策: #D02

- **Trae 规则配置** — 创建 `.trae/rules/ai-project-rules.md` (Always Apply)
  - 6 条核心铁律 + 启动必读清单 + 代码规范
  - 关联决策: #D03

- **里程碑** — M0 项目初始化完成（5/5 任务），M1 核心系统启动
