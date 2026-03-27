# UnityDemo_MMORPG

基于 Unity 2022.3.62f3 的 MMORPG 客户端原型。

**目标**：最小可运行主链路 + 明确职责边界 + 数据/资源常量化 + 可持续扩展

---

## 目录

- [快速开始](#快速开始)
- [总体流程](#总体流程)
- [目录结构](#目录结构)
- [核心系统](#核心系统)
  - [启动与场景装配](#启动与场景装配)
  - [UI 系统](#ui-系统)
  - [导航与地图](#导航与地图)
  - [怪物系统](#怪物系统)
  - [存档与数据](#存档与数据)
- [常量与资源规范](#常量与资源规范)
- [代码约定](#代码约定)
- [故障排查](#故障排查)
- [战斗与掉落/背包](#战斗与掉落背包)
- [工程化改进计划](#工程化改进计划)
- [参考文档](#参考文档)

---

## 快速开始

### 环境要求
- Unity 2022.3.62f3
- 必要包：Input System、Cinemachine、TextMeshPro

### 运行步骤
1. 打开 Unity 项目
2. Build Settings：添加 `BeginScene.unity` 与 `GameScene.unity`
3. 输入资产：PlayerInput 的 Actions 指向 `StarterAssets.inputactions`
4. 运行 BeginScene：开始游戏（创角→自动存档→进入游戏）或继续游戏（选择存档）

---

## 总体流程

```
BeginScene → 创角/读档 → GameScene → 角色/相机/UI 装配 → 导航/地图/怪物模块运行
```

### 架构分层

| 层级 | 职责 | 组成 |
|------|------|------|
| **Framework** | 基础设施，不含业务规则 | UI框架、事件总线、资源加载、数据管理、常量 |
| **Game** | 业务逻辑与流程控制 | CharacterControl、GameScene、Player、Monster、Navigation、Map、Flow、UI |
| **Resources** | 运行时资源 | UI、配置、怪物Prefab、动画/AnimatorController |

---

## 目录结构

```
Assets/
├─ Scenes/
│  ├─ BeginScene.unity          # 开始场景（主菜单/创角/读档）
│  └─ GameScene.unity           # 游戏场景
├─ Resources/
│  ├─ UI/
│  │  ├─ Root/                  # DebugCanvas 等
│  │  └─ Windows/               # 页面/弹窗预制体
│  ├─ Map/                      # 地图图片资源
│  ├─ Monster/                  # 怪物 Prefab（Zombie/Z1~Z4）
│  ├─ AnimatorController/       # 动画控制器
│  └─ Config/                   # JSON 配置文件
│     ├─ RoleClassConfig.json
│     ├─ MapConfig.json
│     └─ MonsterConfig.json
└─ Scripts/
   ├─ Framework/                # 基础设施层
   │  ├─ UI/                    # UI框架
   │  ├─ Event/                 # 事件总线
   │  ├─ Managers/              # 管理器
   │  ├─ Json/                  # JSON处理
   │  └─ Consts/                # 常量定义
   └─ Game/                     # 业务层
      ├─ Boot/                  # 启动入口
      ├─ CharacterControl/      # 角色控制与输入
      ├─ GameScene/             # 场景装配
      ├─ Navigation/            # 导航系统
      ├─ Player/                # 玩家系统
      ├─ Monster/               # 怪物系统
      ├─ Map/                   # 地图系统
      ├─ Combat/                # 战斗系统
      ├─ Battle/                # 战斗运行时
      ├─ Flow/                  # 流程控制
      └─ UI/                    # UI面板
```

---

## 核心系统

### 启动与场景装配

**应用级管理器**：[GameManager.cs](Assets/Scripts/Game/Boot/GameManager.cs)
- 单例 + DontDestroyOnLoad
- Initialize：核心管理器 → 怪物配置 → 进入 Begin 流程
- ReturnToBeginFlow：清理小地图等 → 跳转 BeginScene

**Game 场景入口**：[GameSceneEntry.cs](Assets/Scripts/Game/GameScene/Entry/GameSceneEntry.cs)
- 两阶段：`TryAssembleScene(...)` → `CommitScene(...)`
- Assemble：PlayerRuntimeService.CreateRuntimePlayer（组装→Init→ApplySnapshot→SetAgentId→Register）
- Commit：打开主页面、写入场景上下文、小地图绑定、MonsterModule.InitForScene

**常量定义**：
- 资源路径：[AssetPaths.cs](Assets/Scripts/Framework/Consts/AssetPaths.cs)
- 对象名：[ObjectNames.cs](Assets/Scripts/Framework/Consts/ObjectNames.cs)
- 导航常量：[NavigationConsts.cs](Assets/Scripts/Framework/Consts/NavigationConsts.cs)
- UI名称：[UINames.cs](Assets/Scripts/Framework/UI/UINames.cs)
- UI文案：[UIStrings.cs](Assets/Scripts/Framework/Consts/UIStrings.cs)

---

### UI 系统

| 组件 | 文件 | 职责 |
|------|------|------|
| 基类 | [BasePanel.cs](Assets/Scripts/Framework/UI/Base/BasePanel.cs) | 生命周期管理（OnCreate/OnShow/OnHide/OnDestroyPanel） |
| 管理器 | [UIManager.cs](Assets/Scripts/Framework/Managers/UIManager.cs) | 画布/分层/弹窗栈/主页面管理 |
| 路由 | [UIRouteNames.cs](Assets/Scripts/Framework/UI/UIRouteNames.cs) | 路由常量 |
| 主页面 | [UIMainPages.cs](Assets/Scripts/Framework/UI/UIMainPages.cs) | 主页面注册 |
| 对话服务 | [UIDialogService.cs](Assets/Scripts/Framework/UI/Services/UIDialogService.cs) | 弹窗服务 |

**层级约定**：Bottom / Normal / Popup / Top

---

### 导航与地图

**导航系统**：
- [NavigationPathSolver](Assets/Scripts/Game/Navigation/Runtime/NavigationPathSolver.cs)：基于 NavMesh 求路
- [NavigationRegistry](Assets/Scripts/Game/Navigation/Runtime/NavigationRegistry.cs)：代理注册表
- [PlayerNavigator](Assets/Scripts/Game/Player/Navigation/PlayerNavigator.cs)：玩家导航代理
- [MonsterNavigator](Assets/Scripts/Game/Monster/Navigation/MonsterNavigator.cs)：怪物导航代理

**地图系统**：
- [MapDataManager](Assets/Scripts/Game/Map/Manager/MapDataManager.cs)：地图配置加载
- [MapPanel](Assets/Scripts/Game/Map/UI/MapPanel.cs)：地图UI显示

---

### 怪物系统

**职责分层**：

| 组件 | 职责 |
|------|------|
| [MonsterEntity](Assets/Scripts/Game/Monster/Runtime/MonsterEntity.cs) | 数据/身份/血量/死亡/存档 |
| [MonsterBrain](Assets/Scripts/Game/Monster/Runtime/MonsterBrain.cs) | 状态判断与命令产生 |
| [MonsterLocomotionExecutor](Assets/Scripts/Game/Monster/Runtime/MonsterLocomotionExecutor.cs) | 命令执行（导航/动画） |
| [MonsterNavigator](Assets/Scripts/Game/Monster/Navigation/MonsterNavigator.cs) | 路径执行与速度采样 |
| [MonsterRuntimeRegistry](Assets/Scripts/Game/Monster/Runtime/MonsterRuntimeRegistry.cs) | 注册表 |
| [MonsterRuntimeService](Assets/Scripts/Game/Monster/Runtime/MonsterRuntimeService.cs) | 创建/恢复服务 |

---

### 存档与数据

| 服务 | 文件 | 职责 |
|------|------|------|
| 玩家数据服务 | [GamePlayerDataService.cs](Assets/Scripts/Game/Player/Save/GamePlayerDataService.cs) | 当前玩家与槽位管理 |
| 存档执行 | [PlayerSaveService.cs](Assets/Scripts/Game/Player/Save/PlayerSaveService.cs) | 存档读写 |
| 槽位策略 | [DataManager.cs](Assets/Scripts/Framework/Managers/DataManager.cs) | 尾部追加策略 |
| 摘要映射 | [PlayerSaveMetaMapper.cs](Assets/Scripts/Game/Player/Save/Mapper/PlayerSaveMetaMapper.cs) | PlayerData → 摘要 |

---

## 常量与资源规范

### 规范要求
- **禁止魔法字符串**：新增资源/对象名先补常量再引用
- **统一加载入口**：`ResourceManager.Instance.Load<T>(AssetPaths.Xxx)`

### 常量类

```csharp
// 资源路径
AssetPaths.Window(UINames.Xxx)

// 对象名
ObjectNames.MiniMapCamera

// 导航常量
NavigationConsts.PlayerAgentId
```

---

## 代码约定

### 单一职责
- **单一移动源**：避免脚本位移与 Root Motion 同时驱动
- **动画写入单一来源**：由执行器统一写入

### 导航门禁
- Navigator 不读取业务状态（除 IsDead）
- stopDistance 建议 0.2

### UI规范
- 主页面独占呈现
- 弹窗经 UIDialogService
- 路由统一 UIRouteNames/UIMainPages

---

## 故障排查

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 收到路径但不动 | stopDistance 过大 | 建议设为 0.2 |
| 攻击后一直不动 | Animator 未触发 AttackOver | 检查 Has Exit Time/条件 |
| 转向"趴下/闪烁" | 使用了俯仰旋转 | 使用平面转向（y=0） |

---

## 战斗与掉落/背包

**统一入口**
- 攻击请求：CombatTargetResolver/FactionHelper/CombatRequestFactory/PlayerAttackService
- 结算：BattleDamageService（raw+Atk-Def 基础公式，叠加暴击/闪避）
- 死亡：DeathEvent 广播；DeathRuntimeService 统一清理（去重、延迟销毁）

**掉落链**
- 配置：Resources/Config/DropTableConfig.json（AssetPaths.DropTableConfig）
- 管理器：DropTableConfigManager（JsonUtility 读取）
- 解析：LootResolver（按权重抽一条，产出 itemId/count）
- 服务：LootRuntimeService（订阅 DeathEvent → 解析 → 入包/日志）
- 怪物配置：MonsterConfig.dropTableId（0 表示无掉落）

**背包链**
- 数据：PlayerData.inventoryData = InventoryData(slotCount + slots)
- 兼容：GamePlayerDataService.EnsurePlayerDataSchema（旧档补齐，默认 20/50 格）
- 物品：ItemConfig/ItemConfigManager（AssetPaths.ItemConfig）
- 入包：InventoryService.TryAddItem（先叠堆后新格，受 slotCount 限制）

**最小验证**
1) 在 MonsterConfig.json 为测试怪配置 `"dropTableId": 1001`
2) 配置 DropTableConfig.json（示例已提供 id=1001）
3) 击杀怪物 → Console 日志显示 `drop resolved ... addToInventory=true`
4) 保存并读档后，inventoryData.slots 保留

**UI（事件接线）**
- 路由：UIRouteNames.InventoryPanel
- 事件：OpenInventoryPanelEvent
- 控制器：InventoryUIController（订阅事件→ShowPanel<InventoryPanel>().Refresh()）
- 面板与格子：待接入（V1 仅展示文字）

**玩家状态（HUD 事件化）**
- 事件：PlayerHpChangedEvent / PlayerStaminaChangedEvent
- 订阅方：MainPanel OnShow 订阅、OnHide 反订阅；收到事件实时更新 hpFill/staminaFill
- 体力规则：只禁跑不禁走；体力不足自动降为走路，恢复到阈值后可再次跑

---

## 工程化改进计划

> 目标：将"可运行原型"升级为"可持续演进工程"

### P0（必须优先完成）
- [ ] 运行时入口去占位化
- [ ] 自动化测试恢复
- [ ] 生命周期统一契约
- [ ] 存档读取语义修复

### P1（建议下一阶段完成）
- [ ] 槽位索引文件与可配置分配策略
- [ ] 导航可观测性升级
- [ ] 场景提交里程碑事件化

### P2（工程卫生与长期治理）
- [ ] namespace 与 asmdef 规范统一
- [ ] 静态检查规则
- [ ] ADR 文档化关键决策

---

## 参考文档

| 文档 | 说明 |
|------|------|
| [architecture.md](docs/architecture.md) | 架构指南 |
| [minimal-runtime-checklist.md](docs/minimal-runtime-checklist.md) | 最小链路自检清单 |
| [module-integration-template.md](docs/module-integration-template.md) | 新模块接入模板 |
| [engineering-recommendations-2026-03-25.md](docs/engineering-recommendations-2026-03-25.md) | 工程改进建议 |

---

## 构建与运行自检

- [ ] 场景包含 BeginScene/GameScene；EventSystem 存在
- [ ] PlayerInput 指向唯一输入资产
- [ ] 资源路径与对象名引用均来自常量
- [ ] DebugCanvas 可加载；Pool 监控可显示

---

## 提交与规范

- **分批提交**：逻辑/资源/场景/AnimatorController/Prefab 分离
- **信息格式**：模块名 + 简述 + 关键影响
- **发布前检查**：无未提交更改；README/architecture 同步

---

## 工程化改进计划（增强版）

> 目标：将“可运行原型”升级为“可持续演进工程”，重点提升**稳定性、可回归、可观测、可发布**四个维度。

### 1) P0 / P1 / P2 优先级治理清单

#### P0（必须优先完成）
- **运行时入口去占位化**
  - 清理 GameScene 入口路径中的遗留 stub（固定 `return false/null` 的私有方法）。
  - 原则：运行时程序集不得存在“长期占位实现”。
- **自动化测试恢复**
  - 恢复并启用 EditMode 测试（DataManager / CreateRoleFlowController / UIManager）。
  - 新增 1 条 Gold Path PlayMode 冒烟测试（BeginScene→创角/读档→GameScene→MainPanel）。
- **生命周期统一契约**
  - 引入统一服务接口（建议 `Init / ResetSession / Shutdown`）。
  - 由 App 级 orchestrator 统一编排，禁止模块“各自管理自己生命周期”。
- **存档读取语义修复**
  - 避免 `LoadData<T>` 在缺文件时 `new T()` 的歧义。
  - 新增 `TryLoadData<T>(out T data)` 样式接口，显式区分：缺文件 / 解析失败 / 成功。

#### P1（建议下一阶段完成）
- 槽位索引文件（metadata registry）与可配置分配策略（append-only / first-gap）。
- 导航可观测性升级（成功率、角点数、失败原因分桶）。
- 场景提交里程碑事件化（PlayerReady / UIReady / MonstersInitialized）。
- 明确 PlayerRuntimeService 与 GameSceneEntry 的职责归属，避免双处注册。

#### P2（工程卫生与长期治理）
- namespace 与 asmdef 规范统一。
- 静态检查规则（无用私有方法、硬编码路径、序列化字段空引用防护）。
- ADR（Architecture Decision Record）文档化关键决策。

### 2) 推荐 CI/CD 基线（Unity 项目）

- **PR 阶段（快速反馈）**
  1. C# 编译校验（Unity batchmode）
  2. EditMode Tests（必跑）
  3. 关键规则扫描（硬编码路径、禁用 stub、禁用 TODO/FIXME 泄漏）
- **主干合并前（质量门禁）**
  1. PlayMode Smoke（Gold Path）
  2. 资源完整性检查（关键 prefab/config/inputactions）
  3. Docs 一致性检查（README / architecture / checklist 同步）
- **发版阶段（可靠性保障）**
  1. Save/Load 兼容性回归
  2. Scene 切换与返回 Begin 流程回归
  3. 性能快照（CPU/GC/导航请求频率）

### 3) 生命周期治理建议（Service Lifecycle）

统一状态机建议：`NotInitialized -> Initialized -> SessionActive -> SessionReset -> Shutdown`

- **Init**：进程级只执行一次（注册事件、装配不可变依赖）
- **ResetSession**：切号/返回 Begin/重开局时执行（清空运行态缓存、注册表）
- **Shutdown**：应用退出时执行（反订阅、释放资源）

建议建立 `RuntimeLifecycleOrchestrator`，集中调度以下服务：
- UIManager
- NavigationService
- Flow Controllers（CreateRoleFlowController / RoleUIController）
- Runtime registries（PlayerLocator / NavigationRegistry / MonsterRuntimeRegistry）

### 4) 存档工程化建议（可靠性 + 可演进）

- **文件层**
  - player 数据与 meta 数据分离（正文 + 索引）
  - 保存时先写临时文件再原子替换，避免异常中断导致损坏
- **模型层**
  - PlayerData 增加 `version`
  - 引入 `UpgradePipeline`：读取旧版本时分步迁移
- **接口层**
  - `TryLoad` 返回结构化结果（状态码 + 数据 + 错误信息）
  - 失败日志必须包含：slotId / fileName / exception type

### 5) 观测与调试（Observability）

- **日志分级**：Error / Warn / Info / Debug / Trace（按构建开关裁剪）
- **关键指标**
  - 导航：请求总数、求解失败率、平均角点数
  - 战斗：伤害事件吞吐、死亡事件数
  - 存档：写入成功率、读取失败分桶
- **调试面板建议**
  - 在 DebugCanvas 扩展“运行指标页”和“最近错误页”
  - 支持一键导出最近 N 条运行事件

### 6) 质量门禁（Definition of Done）

一个功能 PR 达到可合并，至少满足：
- 代码：无新增硬编码资源路径；关键常量已补齐
- 测试：新增/修改逻辑有对应 EditMode 或 PlayMode 用例
- 文档：涉及架构或流程变化时同步 README / architecture / checklist
- 运行：Gold Path 不回归，Console 无新增 Error

### 7) 七天实施节奏（含系统落地 + AB 包 + 手机打包）

- **Day 1（闭环设计）**
  - 锁定首周目标：战斗→掉落→背包的最小可玩闭环。
  - 完成模块接入模板（分层、事件、存档、UI、测试）并拆分工单。
- **Day 2（战斗与掉落接线）**
  - 对接 DeathEvent 到掉落服务，补齐掉落表与基础概率逻辑。
  - 约束：掉落逻辑仅通过事件和接口，不破坏现有战斗结算链路。
- **Day 3（背包 MVP）**
  - 完成 InventoryData/InventoryService/InventoryPanel 最小实现。
  - 打通“击杀掉落 -> 入包 -> UI 显示 -> 存档恢复”。
- **Day 4（AB 包流程）**
  - 建立资源分组与命名规范（UI/Monster/Map/Config），输出 AB 构建脚本。
  - 先打开发包（Development AB）验证资源加载与版本号管理。
- **Day 5（手机打包链路）**
  - 生成 Android 包（建议先 APK，后续再 AAB），验证启动、场景切换、输入、存档权限。
  - 执行真机 smoke：Begin→创角/读档→GameScene→打怪→掉落入包。
- **Day 6（回归与性能）**
  - 跑 EditMode + PlayMode 冒烟；记录导航、战斗、存档关键日志。
  - 修复阻断问题（崩溃、卡死、黑屏、存档失败）。
- **Day 7（发布与复盘）**
  - 输出周发布说明（版本号、AB 版本、已知问题）。
  - 复盘并生成下一周生产计划（技能系统 / 成长系统 / 装备系统）。

### 8) 后续流程化生产模型（周迭代循环）

- **计划（Plan）**：每周一冻结范围，只允许 P0 缺陷插入。
- **开发（Build）**：小 PR、日合并，严格执行质量门禁。
- **验证（Verify）**：每日快速 smoke + 周五完整回归。
- **发布（Release）**：统一打 AB、统一打包、统一变更日志。
- **复盘（Review）**：指标复盘（崩溃率、回归失败率、存档失败率）驱动下周优先级。

---
