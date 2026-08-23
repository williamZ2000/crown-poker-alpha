# 开发问题记录（issue-log）

> 创建：2026-08-24（源自 M1 原型试玩反馈）
> 用途：**技术问题**的发现/根因/修法/状态四段式记录。规则与数值类问题走 `decisions.md` + `design.md`，不在此重复（实现层与设计文档不一致的缺陷除外）。
> 状态流转：待修 → 已修（附 commit 号，从"待修"区移入"已修"区）。

## 待修

### ISSUE-001 牌型判定引擎 A 只作顶端，与 design.md A2345 条款不一致

- **发现**：2026-08-24 张总试玩关卡 1-1，选 A+2+3+4+5 被判"未构成有效牌型"
- **根因**：`PokerHandEvaluator` 实现时采用斗地主式 A 只作顶端，未对齐 design.md §3.2 既有"A 低牌顺（A2345）按数字档"条款；#D38 已定稿收窄边界（A 两用仅限顺子/同花顺）
- **修法**：`Evaluate` 支持-wheel：当 A 在选牌内且 2/3/4/5 齐时把 A 视作 1 参与连续性判断，KeyRank 取顺内最高顺位牌（A2345 → 5）；连对/连三张不适用。补单测：A2345 顺、A2345 同花顺、A 在连对中仍无效（AA22）
- **状态**：待修（随 S4 一起改，`Assets/Scripts/Domain/Card/PokerHandEvaluator.cs`）

### ISSUE-002 弃牌模式在次数耗尽后卡死，无法再选牌

- **发现**：2026-08-24 张总试玩：弃牌次数用完后，点击任何手牌都无法选中
- **根因**：`HandView._discardMode` 在弃牌次数归零时未自动复位；且弃牌按钮在次数为 0 时被 `GUI.enabled=false` 禁用，无法点击退出弃牌模式 → 之后每次点牌都走 `DiscardCard` 失败分支，`ToggleSelect` 永远不触发
- **修法**：弃牌成功后或 `DiscardsLeft == 0` 时自动置 `_discardMode = false`（约两行，`Assets/Scripts/UI/HandView.cs`）
- **状态**：待修（随 S4 一起改）

## 已修

（修完从上方移入，格式：### ISSUE-XXX 标题 + 修法摘要 + commit 号）
