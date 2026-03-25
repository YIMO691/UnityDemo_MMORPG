# 工程改进建议（2026-03-25）

## 评审范围
本次评审聚焦当前 Unity MMORPG 原型的运行时架构、生命周期管理、存档读写可靠性、导航/战斗边界解耦与可测试性。

## 执行摘要
项目已经具备较好的模块化意图（Framework 与 Game 分层）、较完整的主流程文档和明确的职责边界。当前最主要风险为：

1. **运行稳定性风险**：关键路径存在静默失败分支（`null/false` 直接返回）以及占位方法。
2. **回归风险**：自动化测试基本失效（现有 EditMode 测试均被注释）。
3. **生命周期一致性风险**：静态单例服务的初始化与清理策略不统一。
4. **数据一致性风险**：存档槽位增长策略与 JSON 接口语义（文件缺失返回 `new T()`）容易导致误判。

## 当前做得较好的点
- Player / Monster / Navigation / Map / UI 领域拆分较清晰。
- `GameSceneEntry + CameraRigAssembler` 具备实用的相机降级策略。
- 怪物运行时分层（Entity/Brain/Executor/Navigator）职责界线明确。
- 常量集中管理（`AssetPaths`、`UINames`、`NavigationConsts`）并有配套文档。

## 高优先级建议（P0）

### P0-1：清理运行时入口中的占位代码
**原因**：`GameSceneEntry` 中仍保留多个固定返回值的占位方法，容易误导维护者并在后续改造中被误用。

**现状观察：**
- `TryCreatePlayerCharacter() => false`
- `GetRoleVisualPath(...) => null`
- `ValidatePlayerComponents(...) => false`
- `TryCreateCamera() => false`
- `TryBindCamera() => false`

**建议：**
- 若已废弃，直接删除；若仍有规划，需实现并接入真实调用链。
- 增加轻量规则（架构测试 / Roslyn 分析器 / CI grep）禁止运行时程序集长期保留占位 stub。

**收益：** 降低场景引导代码的长期维护风险。

---

### P0-2：恢复并升级自动化测试
**原因**：当前关键管理器缺少可执行回归保护。

**现状观察：**三份 EditMode 测试文件均为注释状态。

**建议：**
- 恢复 `NUnit` 与可执行测试，优先覆盖：
  - `DataManager` 槽位/文件名相关接口
  - `CreateRoleFlowController` 事件处理
  - `UIManager` 初始化幂等与可见性行为
- 补充至少 1 条 PlayMode Gold Path 冒烟测试：
  - BeginScene -> 创角/读档 -> GameScene -> MainPanel 可见。
- 在 CI 加入测试通过门禁。

**收益：** 显著降低重构引入回归的概率。

---

### P0-3：统一服务生命周期契约（Init/Clear/Dispose）
**原因**：当前服务多为静态单例，但生命周期接口并不一致。

**现状观察：**
- `NavigationService` 使用 `Init()/Dispose()`。
- `UIManager` 与流程控制器使用 `Init()/Clear()`。
- `AppRuntimeInitializer` 负责集中初始化，但全局清理缺乏统一编排。

**建议：**
- 引入统一生命周期接口（例如 `IRuntimeServiceLifecycle`）：`Init()`、`ResetSession()`、`Shutdown()`。
- 增加生命周期编排器，在场景切换与应用退出时统一驱动。
- 系统性审计 EventBus 订阅，确保“有订阅必有确定性反订阅”。

**收益：** 避免跨会话残留状态与悬挂订阅。

---

### P0-4：修正 JSON 读取接口语义（区分文件缺失与解析失败）
**原因**：`JsonMgr.LoadData<T>` 在文件不存在时返回 `new T()`，会混淆“缺失文件”与“空对象”。

**建议：**
- 新增 `TryLoadData<T>(string fileName, out T data)`，显式返回成功/失败。
- 旧接口保留兼容，但新模块禁止继续使用该语义。
- 在存档服务中显式区分：文件不存在、解析失败、读取成功。

**收益：** 减少隐蔽数据问题和错误回退逻辑。

## 中优先级建议（P1）

### P1-1：优化槽位分配与保留策略
当前策略始终在最大槽位后追加，不复用中间空洞，长期会导致 ID 稀疏与扫描成本上升。

**建议：**
- 引入槽位索引文件（元数据注册表）记录 active/deleted。
- 支持可配置分配策略（append-only / first-gap）。
- 设计兼容迁移流程，保证既有存档可用。

### P1-2：提升导航可观测性并控制日志噪声
`NavigationService` 在常规路径中输出较多日志，线上定位与性能排查可读性一般。

**建议：**
- 引入分级日志封装（Info/Debug/Trace + 构建开关）。
- 补充指标：寻路成功率、平均角点数量、请求拒绝次数。

### P1-3：以领域事件替代部分直接静态调用
当前部分编排仍依赖静态单例直接调用，模块协作边界不够显式。

**建议：**
- 增加场景提交阶段里程碑事件（如 PlayerReady、UIReady、MonstersInitialized）。
- 仅在确定性、低层 API 保留直接调用。

### P1-4：明确玩家运行时注册责任归属
`PlayerRuntimeService` 与 `GameSceneEntry` 都涉及玩家定位/导航初始化，存在责任重复迹象。

**建议：**
- 明确唯一 owner：要么 RuntimeService 负责到底，要么 SceneEntry 负责到底。
- 通过注释契约与测试进行约束。

## 低优先级建议（P2）

### P2-1：命名空间一致性与编译卫生
当前部分脚本使用全局命名空间风格，建议与 asmdef 对齐，统一 namespace，降低符号冲突并提升 IDE 可发现性。

### P2-2：增加轻量静态检查
建议增加以下检查：
- 运行时程序集中的无用私有方法
- 硬编码资源路径（在现有扫描器基础上扩展）
- MonoBehaviour 序列化字段缺少空引用保护

### P2-3：补充架构决策记录（ADR）
对关键决策（槽位策略、单例生命周期、导航门禁）在 `docs/adr/` 中维护简短 ADR，保证长期可追溯。

## 建议推进节奏（30/60/90 天）

### 0–30 天
- 清理/实现运行时 stub。
- 恢复现有 EditMode 测试并跑通。
- 引入 `TryLoadData` 并迁移玩家存档主流程。

### 31–60 天
- 完成生命周期编排器与统一接口落地。
- 增加 1 条 Gold Path PlayMode 冒烟测试。
- 实装日志分级并收敛默认噪声。

### 61–90 天
- 落地槽位元数据注册表与迁移方案。
- 引入场景提交里程碑事件化。
- 建立静态检查基线与 ADR 基线。

## 验收标准（建议）
- 运行时生产程序集不再存在入口占位 stub。
- 自动化测试不少于 15 条（EditMode + PlayMode）且 CI 强制执行。
- 存档读写路径具备显式成功/失败语义。
- 生命周期清单覆盖“场景切换 + 应用退出”并实际执行。
- Gold Path 冒烟测试纳入发布前检查。
