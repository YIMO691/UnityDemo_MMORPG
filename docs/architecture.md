# Architecture Guide

UnityDemo_MMORPG 工程化架构指南

**核心目标**：稳定的最小运行链路 + 清晰的职责边界 + 可持续扩展

---

## 目录

- [架构原则](#架构原则)
- [关键运行链路](#关键运行链路)
- [模块职责矩阵](#模块职责矩阵)
- [目录结构](#目录结构)
- [Actor 行为模型](#actor-行为模型)
- [战斗系统](#战斗系统)
- [导航策略](#导航策略)
- [UI 系统规范](#ui-系统规范)
- [存档与恢复](#存档与恢复)
- [常量与资源规范](#常量与资源规范)
- [动画集成规范](#动画集成规范)
- [调试与工具](#调试与工具)
- [性能与测试](#性能与测试)
- [错误处理与健壮性](#错误处理与健壮性)
- [工程化治理蓝图](#工程化治理蓝图)
- [术语与边界](#术语与边界)
- [提交与工程规范](#提交与工程规范)

---

## 架构原则

### 分层原则

| 层级 | 职责 | 约束 |
|------|------|------|
| **Framework** | 基础设施，不含业务规则 | 不得引用 Game 层 |
| **Game** | 业务逻辑与流程控制 | 可引用 Framework |
| **Editor** | 编辑器工具 | 不得进入 Runtime 程序集 |

### 核心约束

1. **事件解耦**：跨模块通信用 EventBus，禁止直接持有跨层对象
2. **常量化**：路径、对象名、AgentId 统一常量引用
3. **单一职责**：单一位移源、动画写入单一来源
4. **生命周期统一**：Init → ResetSession → Shutdown

---

## 关键运行链路

### 应用启动链路

```
GameManager.Initialize()
    → 核心管理器初始化
    → 怪物配置加载
    → 进入 BeginScene
```

### 游戏场景链路

```
GameSceneEntry
    → TryAssembleScene()
        → PlayerRuntimeService.CreateRuntimePlayer
        → 相机装配
    → CommitScene()
        → 打开主页面
        → 写入场景上下文
        → 小地图绑定
        → MonsterModule.InitForScene
```

### 最小链路清单

详见：[minimal-runtime-checklist.md](minimal-runtime-checklist.md)

---

## 模块职责矩阵

| 模块 | 职责 | 关键文件 |
|------|------|----------|
| **Boot** | 应用级生命周期 | GameManager, AppRuntimeInitializer |
| **GameScene** | 场景装配与提交 | GameSceneEntry, Assemblers |
| **Player** | 玩家数据/控制/存档 | PlayerEntity, PlayerLocomotionBrain, PlayerRuntimeService |
| **Monster** | 怪物数据/行为/生成 | MonsterEntity, MonsterBrain, MonsterLocomotionExecutor |
| **Navigation** | 寻路与导航执行 | NavigationPathSolver, NavigationRegistry |
| **Combat** | 战斗入口与目标解析 | CombatTargetResolver, PlayerAttackService |
| **Battle** | 伤害计算与死亡处理 | BattleDamageService, DeathRuntimeService |
| **Skill** | 技能释放与冷却 | SkillConfigManager, PlayerSkillService |
| **Map** | 地图配置与显示 | MapDataManager, MapPanel |
| **UI** | 界面管理 | UIManager, BasePanel |

---

## 目录结构

```
Assets/Scripts
├─ Framework/                 # 基础设施层
│  ├─ UI/                     # UI框架
│  │  ├─ Base/                # BasePanel
│  │  ├─ Services/            # UIDialogService
│  │  ├─ UIMainPages.cs       # 主页面注册
│  │  └─ UIRouteNames.cs      # 路由常量
│  ├─ Event/                  # 事件总线
│  │  ├─ EventBus.cs
│  │  └─ EventDefine.cs
│  ├─ Managers/               # 管理器
│  │  ├─ UIManager.cs
│  │  ├─ ResourceManager.cs
│  │  ├─ AudioManager.cs
│  │  └─ DataManager.cs
│  ├─ Json/                   # JSON处理
│  │  └─ JsonMgr.cs
│  └─ Consts/                 # 常量定义
│     ├─ AssetPaths.cs
│     ├─ UINames.cs
│     ├─ UIStrings.cs
│     ├─ ObjectNames.cs
│     └─ NavigationConsts.cs
│
├─ Game/                      # 业务层
│  ├─ Boot/                   # 启动入口
│  │  ├─ AppRuntimeInitializer.cs
│  │  ├─ GameBootstrapper.cs
│  │  └─ GameManager.cs
│  │
│  ├─ GameScene/              # 场景装配
│  │  ├─ Entry/
│  │  ├─ Assemblers/
│  │  └─ RuntimeSceneCommitter.cs
│  │
│  ├─ CharacterControl/       # 角色控制
│  │  ├─ Input/
│  │  └─ ThirdPerson/
│  │
│  ├─ Player/                 # 玩家系统
│  │  ├─ Config/              # 配置
│  │  ├─ Data/                # 数据模型
│  │  ├─ Factory/             # 工厂
│  │  ├─ Runtime/             # 运行时
│  │  ├─ Navigation/          # 导航
│  │  └─ Save/                # 存档
│  │
│  ├─ Monster/                # 怪物系统
│  │  ├─ Config/
│  │  ├─ Factory/
│  │  ├─ Runtime/
│  │  ├─ Navigation/
│  │  ├─ Save/
│  │  ├─ Spawner/
│  │  └─ Assembler/
│  │
│  ├─ Navigation/             # 导航系统
│  │  ├─ Contracts/
│  │  └─ Runtime/
│  │
│  ├─ Combat/                 # 战斗入口
│  │  ├─ CombatTargetResolver.cs
│  │  ├─ CombatRequestFactory.cs
│  │  └─ PlayerAttackService.cs
│  │
│  ├─ Battle/                 # 战斗运行时
│  │  ├─ Runtime/
│  │  │  ├─ BattleDamageService.cs
│  │  │  └─ DeathRuntimeService.cs
│  │  ├─ Data/
│  │  │  └─ DamageResult.cs
│  │  └─ Events/
│  │     └─ DeathEvent.cs
│  │
│  ├─ Map/                    # 地图系统
│  │  ├─ Manager/
│  │  ├─ Runtime/
│  │  └─ UI/
│  │
│  ├─ Flow/                   # 流程控制
│  │  ├─ CreateRoleFlowController.cs
│  │  └─ RoleAnimationEventReceiver.cs
│  │
│  ├─ Common/                 # 公共组件
│  │  └─ Runtime/
│  │     └─ ActorControlCommand.cs
│  │
│  └─ UI/                     # UI面板
│     ├─ Controllers/
│     └─ Panels/
│
└─ Editor/                    # 编辑器工具
   ├─ Config/
   └─ Scene/
```

---

## Actor 行为模型

### 命令语义

```csharp
public enum ActorCommandType
{
    None,
    MoveToPosition,
    Stop,
    AttackTarget,
    Idle
}
```

### 控制主线

```
Command → Brain → Executor → Navigator
```

| 角色 | 命令产生 | 命令执行 |
|------|----------|----------|
| **Player** | PlayerLocomotionBrain | ThirdPersonController |
| **Monster** | MonsterBrain | MonsterLocomotionExecutor |

### 优先级

```
Attack > Stop > Move
```

### 关键约束

- 动画写入单一来源（Executor）
- 首帧速度空窗：执行器按帧刷新 + 意图保持

---

## 战斗系统

### 战斗主线

```
Actor Capability → DamageRequest → BattleDamageService → DamageResult
```

### 关键接口

| 接口 | 职责 |
|------|------|
| `IActorIdentity` | 身份标识 |
| `IFactionProvider` | 阵营判断 |
| `ICombatSource` | 攻击者能力 |
| `IDamageReceiver` | 受击者能力 |
| `IAttributeProvider` | 属性提供（攻击/防御/暴击/闪避） |

### 伤害公式

```
基础伤害 = rawDamage + Attack - Defense
最终伤害 = 基础伤害 × 暴击倍率（若暴击）
```

### 死亡主线

```
DeathEvent → DeathRuntimeService → 清理/表现/销毁
```

---

## 技能系统

### 原则
- 技能 = 配置 + 原子效果列表（非脚本化技能）
- 释放与结算分离：技能管释放、目标与冷却；结算统一走 BattleDamageService
- 统一目标源：玩家仅有一个当前战斗目标 PlayerEntity.CurrentTarget

### 数据模型
- SkillEffectType / SkillTargetType / SkillEffectData / SkillConfig / SkillConfigList
- SkillConfigManager：读取 AssetPaths.SkillConfig（Resources/Config/SkillConfig.json）

### 释放与效果
```
PlayerSkillService.CastSkill
    → 校验（施法者/解锁/冷却/目标/射程）
    → 遍历 effects 执行
        - Damage → PlayerAttackService.Attack(..., DamageSourceType.Skill)
        - Heal/RestoreStamina（预留）
```
返回值：SkillCastResult（success/failReason）

### 目标统一
- PlayerEntity：CurrentTarget / SetTarget / ClearTarget
- PlayerAttackService：在解析受击者后写入 SetTarget
- PlayerSkillTargetResolver：优先 CurrentTarget（活体），否则回退最近怪

---

## 资源加载与 AssetBundle

### 命名策略
- 以 `Assets/Resources` 为根，目录转小写、`/`→`.`，加前缀 `ab.`：
  - UI/Windows → ab.ui.windows
  - Map/Main → ab.map.main
  - Monster → ab.monster
  - Config → ab.config

### 编辑器工具
- 目录映射 AssetBundleName：Tools/AssetBundle/1 刷新；2 清空
- 一键打包：Tools/AssetBundle/3 Windows；4 Android；5 iOS

### 运行时加载
- AssetBundleManager：依赖加载、默认根 `Assets/StreamingAssets/<Platform>/`
- AssetBundleLoader：优先从 AB 加载，失败回退 Resources（由 `ENABLE_ASSETBUNDLE_RUNTIME` 宏控制）
- 生命周期：RuntimeLifecycleRegistry 可选注册（宏包裹）

### 版本控制
- `.gitignore` 忽略 `/AssetBundleOutput/` 与 `/Assets/StreamingAssets/<Platform>/`

---

## 导航策略

### 事件流

```
NavigationPathSolver → corners（NavMeshPath）
Navigator.SetPath(corners, stopDistance)
```

### 门禁规则

- Navigator 不读取业务状态（除 IsDead）
- Attack/Dead 状态拒收路径

### Sprint 与体力

- PlayerNavigator 的 autoSprint 需叠加 `PlayerStaminaSystem.CanSprint()`，体力不足时自动降为走路
- 体力恢复到阈值后自动允许再次跑步，保证自动导航不“卡死”

### 距离与停止策略

- `stopDistance` 建议小值（如 0.2），避免"未移动即判到达"
- 路线角点仅高度差时推进到下一角点，避免原地抖动

### 动画融合

- Speed 使用 CurrentSpeed（真实位移速度）
- 转向仅平面旋转，避免俯仰导致"趴下"

---

## UI 系统规范

### 层级定义

| 层级 | 用途 |
|------|------|
| **Bottom** | 底层背景 |
| **Normal** | 普通页面 |
| **Popup** | 弹窗 |
| **Top** | 顶层提示 |

### 弹窗栈

- Popup 入栈
- UIMask 跟随栈顶显示
- CloseByMask 控制遮罩点击关闭

### 主页面

- ShowMainPage 保证同一时间只有一个主页面显示
- 可配置是否隐藏旧主页面

### HUD 事件化（玩家血量/体力）

- **数据源**：PlayerEntity/PlayerStaminaSystem 修改运行态数据后发布事件
- **事件**：PlayerHpChangedEvent、PlayerStaminaChangedEvent
- **订阅方**：MainPanel 在 OnShow 订阅，在 OnHide 反订阅；收到事件更新 hpFill/staminaFill
- **规则边界**：体力系统只控制 sprint（禁跑不禁走），移动输入与导航不被“体力为 0”阻断

---

## 存档与恢复

### 文件结构

```
player_<slotId>.json     # 玩家数据正文
player_index.json        # 槽位索引（建议）
```

### 接口语义

| 方法 | 返回值 |
|------|--------|
| `LoadData<T>` | 文件缺失时返回 `new T()`（待优化） |
| `TryLoadData<T>`（建议） | 显式返回成功/失败 |

### 存档流程

```
创建角色 → 自动分配槽位 → 写入初始数据
游戏运行 → 定期/手动保存 → 更新数据
读档 → 解析数据 → 恢复运行态
```

---

## 常量与资源规范

### 常量类

| 类 | 用途 | 示例 |
|----|------|------|
| `AssetPaths` | 资源路径 | `UI/Root/DebugCanvas` |
| `ObjectNames` | 对象名 | `MiniMapCamera` |
| `NavigationConsts` | 导航常量 | `PlayerAgentId` |
| `UINames` | UI名称 | `MainPanel` |
| `UIStrings` | UI文案 | 按钮文本等 |

### 规范要求

- **禁止魔法字符串**
- 新增资源与对象名必须先补常量
- 统一通过 `ResourceManager.Instance.Load<T>(AssetPaths.Xxx)` 加载

---

## 动画集成规范

### 参数约定

| 参数 | 类型 | 说明 |
|------|------|------|
| `Speed` | Float | 0–正值，由真实速度驱动 |
| `IsChasing` | Bool | 追击期为 true |
| `Dead` | Bool | 死亡为 true |
| `Attack` | Trigger | 由逻辑或事件触发 |

### 过渡与事件

- Attack → Locomotion：勾选 Has Exit Time 或增加条件
- 攻击末帧添加 `AttackOver` 事件

### Root Motion

- 目前不启用 Root Motion，位移由导航驱动
- 若启用，必须移除脚本位移，保持单一位移源

---

## 调试与工具

### DebugCanvas

- 资源路径：`AssetPaths.DebugCanvas`
- PoolMonitorPanel：`AssetPaths.PoolMonitorPanel`
- 自动接入：`GameSceneEntry.EnsureDebugCanvas`

### 导航调试

- 记录每次路径下发与拒收原因
- 关键日志：收到请求、路径求解状态、角点数量

### 怪物调试

- K 键最近怪伤害：`MonsterDamageDebugInput`
- 状态机切换日志：Idle/Chase/Attack/Return/Dead

### 路径扫描

- Editor/Tools/PathUsageScanner（Tools/Path Scanner 菜单）

---

## 性能与测试

### 性能预算

| 项目 | 建议 |
|------|------|
| 导航求解 | `pathInterval ≥ 0.3s` |
| 动画 | Speed 采用真实速度，避免过多触发器抖动 |
| 事件总线 | 高频事件合并或降采样 |
| 对象池 | Map 路径段、临时指示器进入 Pool |

### 测试分层

| 类型 | 范围 |
|------|------|
| **Unit/EditMode** | DataManager、Mapper、Flow、Event |
| **Integration/EditMode** | 资源路径与装配前置条件 |
| **PlayMode Smoke** | Gold Path 主链路 |
| **PlayMode Scenario** | 导航门禁、怪物状态、存档恢复 |

### 必测回归集合

1. BeginScene 创角进入 GameScene
2. ContinuePanel 读档并进入 GameScene
3. MainPanel / RoleInfoPanel 路由正确
4. 地图点击触发导航并可停止
5. 怪物死亡与重生逻辑正确
6. Save → Load 后状态恢复

---

## 错误处理与健壮性

### 资源加载失败

- ResourceManager.Load 返回 null 时早退并记录错误
- 必要时提供降级（相机 Fallback）

### 事件处理

- 订阅方需判空与状态早退
- 避免抛异常阻断总线

### 导航求解失败

- PathSolver 返回 Invalid/Partial 时记录日志并不下发
- 调用方可重试或降级走向目标点

### 怪物状态机

- AttackOver 未触发：使用失败保护超时自动解锁
- 日志提示 Animator 配置缺失

### 存档与版本

- 字段缺失或版本不匹配：升级器迁移
- 保留旧文件备份并打印版本信息

---

## 工程化治理蓝图

### 架构约束规则化

| 约束 | 检查方式 |
|------|----------|
| 层次约束 | Editor 菜单扫描 + CI 规则脚本 |
| 资源路径约束 | 禁止直接写 `Resources/...` 字符串 |
| 事件约束 | 订阅必须可追踪到反订阅点 |
| 占位实现约束 | Runtime 禁止长期存在固定返回值 stub |

### 生命周期架构

```csharp
interface IRuntimeServiceLifecycle
{
    void Init();          // 进程级初始化（一次）
    void ResetSession();  // 会话级重置（切号/回 Begin）
    void Shutdown();      // 进程退出清理
}
```

### 场景提交里程碑

```
PlayerAssembled → CameraBound → UIReady → RuntimeContextWritten → MonstersInitialized → BattleRuntimeReady
```

### 新模块接入流程

详见：[module-integration-template.md](module-integration-template.md)

---

## 术语与边界

| 术语 | 定义 |
|------|------|
| **App** | 应用级生命周期（GameManager、场景切换） |
| **Scene Entry** | 场景装配入口，两阶段（组装→提交） |
| **Assembler** | 装配器，只负责构建与绑定，不做业务决策 |
| **Service** | 跨模块能力（RuntimeService/UIDialogService） |
| **Registry** | 运行态注册表（NavigationRegistry/MonsterRuntimeRegistry） |
| **Agent** | 导航代理，只接受路径与停止指令 |
| **Event** | 领域事件，用于解耦发布/订阅 |

---

## 测试策略细化

### Play Mode
- 黄金链路：Begin→创角→GameScene→MainPanel 打开
- 怪物行为：Idle→Chase→Attack→Return→Idle 循环；AttackOver 正常触发；失败保护生效
- 导航门禁：Attack/Dead 拒收路径；stopDistance 正确导致抵达

### Edit Mode
- 配置加载：Role/Map/Monster JSON 正确解析；AssetPaths 引用存在
- 常量化检查：随机扫描代码避免魔法字符串；CI 提示异常引用

---

## 提交与工程规范

### 分批提交

- 逻辑/资源/场景/AnimatorController/Prefab 分离

### 信息格式

```
模块名 + 简述 + 关键影响
```

### 发布前检查

- [ ] 无未提交更改
- [ ] README/architecture 同步
- [ ] 最小链路回归通过
- [ ] Console 无跨层引用/脚本错误

---

## 工程化治理蓝图（Architecture Governance Blueprint）

本节用于把"架构原则"转化为"可执行工程制度"，避免文档只停留在理念层。

### 架构约束到规则（Rule-as-Code）

建议将以下约束纳入自动检查：

1. **层次约束**
   - Framework 不得引用 Game。
   - Editor 代码不得进入 Runtime 程序集。
2. **资源路径约束**
   - 禁止在业务代码中直接写 `Resources/...` 字符串。
   - 必须通过 `AssetPaths` 常量访问。
3. **事件约束**
   - EventBus 订阅必须可追踪到反订阅点。
   - 禁止"只订阅不释放"的静态监听器。
4. **占位实现约束**
   - Runtime 程序集中禁止长期存在"固定返回值 stub"。

> 落地方式：Editor 菜单扫描 + CI 规则脚本双保险。

### 生命周期架构（Lifecycle Architecture）

建议统一生命周期接口：

```text
interface IRuntimeServiceLifecycle {
  void Init();          // 进程级初始化（一次）
  void ResetSession();  // 会话级重置（切号/回 Begin）
  void Shutdown();      // 进程退出清理
}
```

并引入 `RuntimeLifecycleOrchestrator` 统一编排：

- Init 阶段：Data/UI/Flow/Navigation/Map 等依赖顺序初始化
- SessionReset 阶段：清理 Runtime Registry、清空当前玩家上下文、关闭残留 UI
- Shutdown 阶段：统一反订阅与资源释放，防止退出期异常

### 存档架构（Save Architecture）

#### 文件与索引分层
- player 正文：`player_<slotId>.json`
- 槽位索引：`player_index.json`（记录 active/deleted/createTime/updateTime/version）

#### 接口语义分层
- `TryLoadPlayer(slotId)` 返回三态：NotFound / Invalid / Success
- 读取失败必须携带错误上下文：文件名、异常类型、版本号

#### 迁移管线
- `PlayerData.version`
- `UpgradePipeline`: v1->v2->v3 逐级迁移，禁止跨级硬跳

#### 一致性写入
- 临时文件写入成功后再原子替换目标文件
- 防止崩溃/断电导致半写入状态

### 场景提交模型（Scene Commit Model）

建议把 `GameSceneEntry` 的提交阶段拆成显式里程碑事件：

1. `PlayerAssembled`
2. `CameraBound`
3. `UIReady`
4. `RuntimeContextWritten`
5. `MonstersInitialized`
6. `BattleRuntimeReady`

好处：
- 明确故障位置（哪个里程碑失败）
- 便于埋点统计与回归对比
- 便于增量模块“挂接里程碑”而不是侵入主流程

### E. 测试架构（Test Architecture）

#### E1. 测试分层
- **Unit/EditMode**：纯逻辑（DataManager、Mapper、Flow、Event）
- **Integration/EditMode**：资源路径与装配前置条件
- **PlayMode Smoke**：Gold Path 主链路
- **PlayMode Scenario**：导航门禁、怪物状态、存档恢复

#### E2. 必测回归集合（Minimum Regression Pack）
1. BeginScene 创角进入 GameScene
2. ContinuePanel 读档并进入 GameScene
3. MainPanel / RoleInfoPanel 路由正确
4. 地图点击触发导航并可停止
5. 怪物死亡与重生逻辑正确
6. Save->Load 后位置/场景/怪物状态恢复

#### E3. PR 门禁建议
- 每个 PR 至少执行：
  - 编译检查
  - EditMode tests
  - 规则扫描
- 合并前额外执行：
  - Gold Path PlayMode smoke

### F. 可观测性架构（Observability Architecture）

#### F1. 事件日志标准
统一字段：
- `timestamp`
- `module`
- `event`
- `entityId/agentId`
- `result`
- `reason`

#### F2. 指标建议
- Navigation
  - RequestCount
  - PathSolveFailRate
  - AverageCornerCount
- Save/Load
  - SaveSuccessRate
  - LoadFailCountByReason
- Combat
  - DamageEventCount
  - DeathEventCount

#### F3. 调试 UI
在 DebugCanvas 下增加：
- 指标实时面板
- 最近错误事件列表
- 一键导出日志快照

### G. 发布治理（Release Governance）

每次里程碑发布前建议执行以下 Gate：

1. **Functional Gate**
   - Gold Path 全链路通过
   - UI 路由与遮罩行为通过
2. **Data Gate**
   - Save/Load 兼容性通过
   - 版本迁移回归通过
3. **Stability Gate**
   - Console 无新增 Error
   - 关键场景切换无异常残留对象
4. **Docs Gate**
   - README / architecture / checklist 同步

### H. ADR 制度（Architecture Decision Record）

建议在 `docs/adr/` 维护决策记录，模板建议字段：
- Context（背景）
- Decision（决策）
- Status（Proposed/Accepted/Superseded）
- Consequences（影响）
- Alternatives（备选方案）

优先补齐 ADR 主题：
1. 单例服务生命周期统一方案
2. 存档槽位策略（append-only vs first-gap）
3. 导航门禁策略与 stopDistance 基准
4. 场景提交里程碑事件化方案

### I. 七天落地目标（可量化，含 AB 与手机打包）

- Day1 完成系统拆分与工单（战斗/掉落/背包/AB/移动端）。
- Day3 前完成“击杀 -> 掉落 -> 入包 -> 存档恢复”闭环。
- Day4 完成 AB 构建脚本与资源分组规范，产出可加载 AB 包。
- Day5 完成 Android 真机打包与黄金链路验证。
- Day6 完成一次完整回归（EditMode + PlayMode smoke）。
- Day7 发布周版本并输出复盘与下周迭代计划。

### J. 后续流程化生产模型（持续迭代）

- **Iteration Cadence**：1 周一个迭代，固定“计划-开发-验证-发布-复盘”节奏。
- **System Order**：掉落/背包 -> 技能 -> 成长 -> 装备 -> 经济与任务。
- **Build Pipeline**：代码构建 -> 测试 -> AB 构建 -> Android 打包 -> Smoke -> 发布。
- **Quality Gates**：PR Gate（编译+EditMode+规则扫描）；Release Gate（PlayMode+真机+存档兼容）。
- **Metrics Driven**：按崩溃率、回归失败率、存档失败率、加载耗时驱动优先级。

---
