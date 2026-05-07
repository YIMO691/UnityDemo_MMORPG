# Architecture

UnityDemo_MMORPG 工程化架构说明。

本文档对应分支：`feat/combat-hit-verify-respawn-overlay`。

文档目标不是描述“理想设计”，而是准确说明当前工程已经落地的结构、职责边界、运行链路、数据流和已知工程问题，便于后续继续迭代。

---

## 1. 架构目标

当前工程的架构目标可以概括为四点：

1. **最小主链路可运行**：Begin → 创角/读档 → GameScene → 战斗 → 死亡 → 复活 → 存档
2. **模块职责清晰**：输入、导航、AI、战斗、掉落、存档、UI 各有明确边界
3. **工程化可收口**：路径常量、运行时生命周期、两阶段场景装配、配置驱动
4. **扩展空间保留**：为技能 UI、装备、任务、网络同步等后续能力留接口

---

## 2. 分层设计

### 2.1 分层结构

```text
Framework
  ├─ 常量、UI 框架、事件总线、资源管理、对象池、数据管理
  └─ 不承载具体游戏规则

Game
  ├─ 启动流程
  ├─ 场景装配
  ├─ Player / Monster / Navigation / Combat / Battle / Skill / Inventory / UI
  └─ 承载业务规则与运行时逻辑

Resources
  ├─ UI 预制体
  ├─ 角色/怪物/地图资源
  └─ JSON 配置
```

### 2.2 依赖原则

- `Game` 可以依赖 `Framework`
- `Framework` 不应依赖 `Game` 业务模块
- `Editor` 仅用于编辑器工具，不进入运行时主逻辑
- 场景对象装配尽量通过 `Assembler / RuntimeService` 完成，而不是在面板或零散 MonoBehaviour 中硬编码

---

## 3. 工程目录与模块职责

## 3.1 Framework 层

### `Framework/Consts`
统一定义路径、对象名、场景名、文案。

核心文件：
- `AssetPaths.cs`
- `ObjectNames.cs`
- `SceneNames.cs`
- `NavigationConsts.cs`
- `UIStrings.cs`

工程意义：
- 避免魔法字符串
- 降低资源重命名、场景切换、对象查找时的脆弱性
- 为资源校验工具和路径扫描留基础

### `Framework/Event`
`EventBus` 是当前工程的轻量消息中心。

使用方式：
- 业务模块发布事件
- 控制器/UI/运行时服务订阅事件
- 降低模块间直接引用

当前典型事件：
- `OpenPanelEvent`
- `OpenMainPageEvent`
- `CreateRoleRequestEvent`
- `NavigationMoveRequestEvent`
- `DamageAppliedEvent`
- `DeathEvent`
- `PlayerDeadEvent`
- `InventoryChangedEvent`
- `PlayerHpChangedEvent`
- `PlayerStaminaChangedEvent`

### `Framework/Managers`
- `UIManager`：统一 Panel 生命周期与层级管理
- `DataManager`：设置数据与槽位语义管理
- `ResourceManager`：统一资源加载入口
- `AudioManager`：音频与设置联动
- `AssetBundleManager`：AB 依赖加载与释放

### `Framework/Resource`
通过 `IResLoader` 抽象资源来源。

当前实现：
- `ResourcesLoader`
- `AssetBundleLoader`

运行策略：
- Editor 默认 `Resources`
- 非 Editor 默认 `AssetBundle`
- `AssetBundleLoader` 在宏 `ENABLE_ASSETBUNDLE_RUNTIME` 打开时先尝试 AB，失败回退 `Resources`

### `Framework/Pool`
对象池用于降低瞬时实例化开销。

当前已接入：
- 飘字 `DamageText`
- 地图路径段 `MapPathSegment`
- Debug 面板统计

### `Framework/UI`
提供 Panel 基类、路由、分层与对话服务。

关键约束：
- 页面类继承 `BasePanel`
- Panel 生命周期统一为 `OnCreate / OnShow / OnHide / OnDestroyPanel / OnRefresh`
- Popup 使用栈管理并与遮罩联动

---

## 3.2 Game 层

### `Boot`
负责应用级生命周期与模块初始化编排。

关键文件：
- `StartGame.cs`
- `GameManager.cs`
- `RuntimeLifecycleRegistry.cs`

职责：
- 启动时创建并初始化 `GameManager`
- 统一注册系统初始化项
- 进入 Begin 流程
- 返回 BeginScene 时处理场景清理和 UI 恢复

### `Flow`
处理创角等业务流程。

关键文件：
- `CreateRoleFlowController.cs`

职责：
- 监听创角请求事件
- 调用 `PlayerFactory` 生成角色数据
- 分配新存档槽位
- 落盘并设置当前会话玩家
- 进入 `GameScene`

### `GameScene`
负责 `GameScene` 的两阶段装配与提交。

关键文件：
- `GameSceneEntry.cs`
- `RuntimeSceneCommitter.cs`
- `CameraRigAssembler.cs`

职责：
- 检查会话中是否存在当前玩家数据
- 组装相机、玩家、小地图、调试层
- 在提交阶段打开主页面、写入运行态上下文、初始化怪物模块、接通战斗运行时

### `Player`
玩家模块是当前项目最核心的业务模块之一。

职责拆分：
- `PlayerFactory`：生成 `PlayerData`
- `PlayerCharacterAssembler`：装配玩家预制体与运行时组件
- `PlayerRuntimeService`：运行态创建统一入口
- `PlayerEntity`：玩家运行时实体、属性、目标、受击、快照
- `PlayerInputProxy`：统一原始输入与导航输入覆盖
- `PlayerLocomotionBrain`：输入门禁与控制状态裁剪
- `PlayerNavigator`：自动导航代理
- `PlayerStaminaSystem`：跑步消耗与恢复
- `PlayerRespawnRuntimeService`：玩家复活执行
- `PlayerProgressionService`：经验与升级
- `PlayerExpRuntimeService`：击杀经验发放
- `PlayerSaveService` / `GamePlayerDataService`：会话数据与存档

### `Monster`
怪物模块已经从“单脚本逻辑”收敛为多层结构。

职责拆分：
- `MonsterEntity`：怪物实体数据、状态、血量、身份、保存快照
- `MonsterBrain`：AI 决策与命令生成
- `MonsterLocomotionExecutor`：命令执行与动画/导航协调
- `MonsterNavigator`：路径执行与速度采样
- `MonsterAnimatorDriver`：Animator 参数写入
- `MonsterRuntimeService`：从刷怪点或存档恢复创建怪物
- `MonsterRuntimeRegistry`：怪物运行时注册表
- `MonsterModule`：场景级门面
- `MonsterSpawnPoint`：刷怪点、自增重生、激活距离控制
- `MonsterSaveService`：怪物场景快照与恢复
- `MonsterAggroService`：统一清仇恨

### `Navigation`
导航模块把“点击移动”和“路径执行”分离。

职责拆分：
- `NavigationService`：事件入口与寻路结果下发
- `NavigationPathSolver`：NavMesh 采样与路径计算
- `NavigationRegistry`：导航代理注册表
- `BaseNavigator`：导航公共逻辑基类
- `PlayerNavigator` / `MonsterNavigator`：角色侧路径执行

### `Combat` 与 `Battle`
当前工程将“攻击入口”和“伤害结算”明确拆开。

- `Combat`：负责把某次攻击组织成标准请求
- `Battle`：负责统一结算公式、结果事件与死亡触发

### `Drop / Inventory / Item`
负责怪物掉落、背包、物品使用。

- `DropTableConfigManager`：掉落表配置加载
- `LootResolver`：按权重抽取掉落结果
- `LootRuntimeService`：订阅死亡事件并发放掉落
- `InventoryService`：入包、堆叠、容量限制
- `InventoryActionService`：使用/丢弃物品
- `ItemUseEffectSystem`：具体物品效果实现

### `Skill`
当前技能系统分为两部分：

1. **正式服务链**：
   - `SkillConfigManager`
   - `PlayerSkillService`
   - `PlayerSkillTargetResolver`
   - `SkillCastResult`

2. **Prototype 原型链**：
   - 位于 `Game/Skill/Prototype`
   - 用于验证 Skill / Atom 对象模型，不直接进入当前主战斗链

### `UI`
Game/UI 承载业务页面、业务控制器与运行时 UI。

控制器：
- `InventoryUIController`
- `ItemDetailUIController`
- `PlayerDeathUIController`
- `RoleUIController`

页面/弹窗：
- `BeginPanel`
- `CreateRolePanel`
- `ContinuePanel`
- `MainPanel`
- `MapPanel`
- `InventoryPanel`
- `SettingPanel`
- `RoleInfoPanel`
- `ItemDetailPopup`
- `MessageTipPanel`
- `AboutPanel`

运行时 UI：
- `RespawnOverlayRuntime`

---

## 4. 运行时生命周期编排

当前工程不是把所有模块初始化散落在 `Awake/Start`，而是引入了轻量级的生命周期注册机制：

```text
RuntimeLifecycleBootstrap.RegisterDefaults()
  → RuntimeLifecycleRegistry.Register(name, init, resetSession, shutdown)
  → InitAll()
```

当前默认注册项包括：
- `DataManager`
- `UIManager`
- `RoleDataManager`
- `MapDataManager`
- `CreateRoleFlowController`
- `RoleUIController`
- `NavigationService`
- `ItemConfigManager`
- `DropTableConfigManager`
- `SkillConfigManager`
- `LootRuntimeService`
- `DeathRuntimeService`
- `PlayerExpRuntimeService`
- `PlayerDeathUIController`
- `AssetBundleManager`（仅在宏打开时）

### 工程价值
- 初始化入口统一
- 模块重置/关闭有固定语义
- 降低 BeginScene / GameScene 来回切换时的脏状态概率

### 当前局限
- 仍未形成完整的全模块 `ResetSession` 策略
- 不是所有 Game-only Controller 都进入生命周期注册，`GameManager.InitGameOnlyManagers()` 仍做了一部分显式初始化

---

## 5. 启动与场景装配

## 5.1 启动链路

```text
StartGame.Start()
  → 若无 GameManager，则创建
  → GameManager.Initialize()
      → RegisterDefaults()
      → InitAll()
      → InitGameOnlyManagers()
      → MonsterConfigManager.Init()
      → EnterBeginFlow()
```

### 说明
- `GameManager` 通过 `DontDestroyOnLoad` 常驻
- 场景切换后由 `sceneLoaded` 钩子辅助恢复 Begin 流程
- 返回标题时会清理小地图绑定并重新进入 BeginScene

## 5.2 GameScene 两阶段装配

`GameSceneEntry` 采用 **TryAssemble → Commit** 两阶段结构。

### 阶段一：`TryAssembleScene()`
目标：只做对象创建与依赖绑定，不做副作用提交。

内容包括：
- 创建主相机 / 跟随相机
- 通过 `PlayerRuntimeService` 创建玩家运行态
- 绑定 Cinemachine Follow / LookAt
- 初始化小地图对象引用

### 阶段二：`CommitScene()`
目标：在依赖全部成立后再提交场景副作用。

内容包括：
- 打开 `MainPanel`
- 小地图绑定玩家目标
- 写入场景上下文到 `PlayerData.runtimeData`
- 注入 DebugCanvas
- 初始化怪物模块
- 确保 BattleRuntime 可用

### 工程价值
- 装配失败时可统一回滚，不提前污染场景状态
- 降低“半初始化”对象残留风险

---

## 6. 数据模型与存档模型

## 6.1 玩家数据模型

当前 `PlayerData` 主要由以下部分组成：

```text
PlayerData
├─ baseData        # 角色基础信息（id、名字、职业）
├─ progressData    # 等级、经验、解锁技能
├─ attributeData   # 最大生命、攻击、防御、速度、暴击等
├─ runtimeData     # 当前HP、当前体力、位置、朝向、死亡状态
├─ inventoryData   # 背包槽位与物品列表
└─ monsterData     # 当前场景怪物存档快照
```

## 6.2 Schema 修复策略

`GamePlayerDataService.EnsurePlayerDataSchema()` 负责旧档兼容与运行前归一化，主要修复：
- `progressData` 缺失
- `skillIds` 缺失
- `inventoryData` 缺失
- `runtimeData` 缺失
- `attributeData.maxStamina` 等职业字段缺失
- `currentHp / currentStamina` 越界

### 工程价值
- 降低数据结构迭代对旧档的破坏性
- 把“初始化默认值”与“修复旧档”合并成一处入口

## 6.3 存档语义

### 玩家存档
- 文件命名：`player_<slotId>.json`
- 当前槽位由 `DataManager` 维护
- 玩家真正数据由 `GamePlayerDataService` 维护

### 怪物存档
- 保存时由 `RuntimeSaveService.SaveMonsters()` 抓取当前场景怪物列表
- 每个怪物保存：
  - runtimeId
  - configId
  - 位置/朝向
  - HP / isDead
  - spawnPointId

### 槽位策略
- 新创角使用“最大槽位 + 1”策略追加
- ContinuePanel 打开时先执行 `CompactPlayerSlots()`，把空洞压缩到前部

---

## 7. 玩家工程化设计

## 7.1 玩家运行时创建

玩家运行时创建通过 `PlayerRuntimeService.CreateRuntimePlayer()` 统一完成。

内部逻辑可概括为：

```text
PlayerCharacterAssembler.TryAssemble()
  → 实例化玩家预制体
  → 加载职业外观
  → 挂接/获取 InputProxy、Brain、Stamina、Navigator、Entity
  → Controller.InitCameraReference()
  → Entity.Init(data, runtimeId)
  → ApplyRuntimeSnapshot()
  → Navigator.SetAgentId()
  → Register 到 PlayerLocator / NavigationRegistry
```

### 工程价值
- 避免 BeginScene / GameScene / 读档流程各自拼装玩家
- 运行态创建入口统一，便于扩展网络同步或多人角色生成

## 7.2 输入模型

玩家输入分为两类：

1. **原始输入**：来自 `StarterAssetsInputs`
2. **导航覆盖输入**：由 `PlayerNavigator` 生成并写入 `PlayerInputProxy`

`PlayerInputProxy.CurrentCommand` 的规则：
- 无导航覆盖时，读取原始输入
- 有导航覆盖时，使用导航移动命令，但保留原始 look 输入

### 工程价值
- 让“点击移动”和“自由转镜”可以共存
- 导航不直接操作角色控制器，而是通过统一输入口覆盖

## 7.3 控制门禁

`PlayerLocomotionBrain` 持有玩家当前控制门禁：
- movementEnabled
- jumpEnabled
- lookEnabled
- sprintEnabled
- attackEnabled
- stopEnabled

当前主要用于：
- 玩家死亡锁控制
- 体力不足时只关 sprint，不关 movement
- 复活前后控制恢复

## 7.4 体力系统

`PlayerStaminaSystem` 的原则是：
- 跑步消耗体力
- 行走和 Idle 恢复体力
- 体力归零时禁止 Sprint，但不阻断移动
- 自动导航与 `CanSprint()` 联动，避免体力空时卡住导航

### 工程价值
- 把移动能力裁剪放在体力系统与 Brain 层，而不是散落在 UI 或控制器里

---

## 8. 怪物工程化设计

## 8.1 分层结构

```text
MonsterEntity
  ├─ 运行时数据、身份、HP、目标、状态
  ├─ 保存/恢复快照
  └─ 不负责 AI 决策

MonsterBrain
  ├─ 状态判断
  ├─ 追击/攻击/回归切换
  ├─ 命中检测
  └─ 产生命令或攻击请求

MonsterLocomotionExecutor
  ├─ 执行移动/停止/攻击命令
  ├─ 协调 Animator 和 Navigator
  └─ 保持动画写入单一来源

MonsterNavigator
  ├─ 按路径角点移动
  ├─ 速度采样
  └─ 平面旋转
```

## 8.2 状态机

当前怪物状态：
- `Idle`
- `Chase`
- `Attack`
- `Return`

状态流转：

```text
Idle
  →（玩家进入 detectRange）→ Chase
Chase
  →（进入 attackRange）→ Attack
  →（玩家脱离 detectRange）→ Return
Attack
  →（AttackOver 且目标离开）→ Chase
  →（目标丢失）→ Idle
Return
  →（回到出生点）→ Idle
  →（玩家重新进入 detectRange）→ Chase
```

## 8.3 真实命中判定

怪物攻击不是只按距离结算，而是增加了真实命中校验。

`MonsterBrain.DetectHitTarget()` 当前逻辑：
- 从怪物前方偏移位置作为检测原点
- 使用 `Physics.SphereCastAll`
- 按命中距离排序
- 解析 `IDamageReceiver`
- 结合前向夹角过滤目标
- 使用阵营校验排除友方或非法目标

### 工程价值
- 避免“攻击动画播了但目标明明不在前方仍然命中”
- 为后续武器形状、命中盒、技能命中区域扩展提供基础

## 8.4 动画与失败保护

当前怪物攻击依赖动画事件：
- `OnAttackEvent()`：真正出伤
- `OnAttackOver()`：解除攻击冻结

同时有 `attackFailSafeTimeout`：
- 防止动画事件漏接导致怪物永久卡在 Attack 状态

### 当前风险
- 动画事件配置仍是关键依赖点
- 若 Animator 资源和逻辑命名不一致，攻击链会失真

## 8.5 刷怪点与重生

`MonsterSpawnPoint` 负责：
- 场景初始化刷怪
- `maxAliveCount` 控制
- `respawnTime` 定时补足存活数
- `activationDistance` 可选距离激活
- 多怪权重配置

`MonsterRuntimeRegistry.CountAliveBySpawnPoint()` 提供当前活怪统计，这是刷怪点判断是否补怪的核心依据。

---

## 9. 导航工程化设计

## 9.1 导航主链

```text
UI / Input
  → NavigationMoveRequestEvent
  → NavigationService
  → NavigationPathSolver.TryBuildPath()
  → NavigationRegistry.TryGet(agent)
  → agent.SetPath(corners, stopDistance)
```

## 9.2 设计要点

### 寻路与执行分离
- `NavigationPathSolver` 只负责算路径
- `BaseNavigator` 负责执行路径
- `NavigationService` 只负责组织事件和下发结果

### 代理注册机制
- 玩家和怪物导航代理都实现 `INavigationAgent`
- 用 `agentId` 在 `NavigationRegistry` 查找目标
- 玩家固定 `NavigationConsts.PlayerAgentId`

### 取消自动导航的条件
`PlayerNavigator` 当前约定：
- WASD / 左摇杆 / 跳跃会打断自动导航
- Look 输入不会打断自动导航
- 新路径刚下发有短暂保护窗口，避免同帧误取消

### 体验层优化
- `stopDistance` 使用较小值，避免“还没走就认为到达”
- 平面旋转防止俯仰干扰
- 保持真实速度采样驱动动画，减少滑步

---

## 10. UI 工程化设计

## 10.1 UI 层级

```text
Bottom
Normal
Popup
Top
```

当前约定：
- Begin / Main / CreateRole 属于主页面
- 设置、继续游戏、角色详情、背包详情等属于 Popup

## 10.2 BasePanel 生命周期

```text
OnCreate      # 首次实例化
OnShow        # 每次显示
OnRefresh     # 主动刷新
OnHide        # 每次隐藏
OnDestroyPanel# 销毁前解绑
```

### 工程价值
- 避免事件绑定/解绑散落在任意函数里
- 统一处理面板复用与刷新语义

## 10.3 主页面与弹窗语义

### 主页面
通过 `UIManager.ShowMainPage()` 展示，强调“当前只有一个主页面处于主视图”。

### 弹窗
Popup 进入栈管理，遮罩跟随栈顶弹窗控制显示与点击关闭。

### 当前注意点
`UIMainPages.IsMainPage()` 目前把 `SettingPanel` 和 `ContinuePanel` 也纳入了 MainPage 判断，但这两个面板实际又定义为 Popup。文档上应以运行时 Layer 行为为准，后续建议统一语义。

## 10.4 玩家状态 HUD

主界面通过事件驱动更新：
- `PlayerHpChangedEvent`
- `PlayerStaminaChangedEvent`

`MainPanel` 在 `OnShow` 订阅，在 `OnHide` 取消订阅。

### 工程价值
- UI 不主动轮询运行态数据
- HP / 体力变化可以跨物品使用、受击、升级、复活等路径复用

## 10.5 复活 Overlay

`RespawnOverlayRuntime` 是场景外常驻运行时 UI，不依赖预制体。

特点：
- 首次访问时动态构建 Canvas、黑屏、倒计时文本、复活按钮
- 通过 `Show(duration, callback)` 驱动倒计时
- 倒计时结束或按钮点击后执行回调

### 工程价值
- 死亡 UI 不依赖某个具体场景预制体
- 易于后续替换成正式 Prefab 化实现

---

## 11. 战斗工程化设计

## 11.1 统一能力接口

当前战斗入口依赖以下接口组合：

- `IActorIdentity`
- `IFactionProvider`
- `IAttributeProvider`
- `ICombatSource`
- `IDamageReceiver`

### 工程意义
- 玩家与怪物通过同一协议进入战斗系统
- 不需要在 `BattleDamageService` 内写“如果是玩家...如果是怪物...”的硬编码分支

## 11.2 攻击入口

### 玩家攻击
`PlayerAttackService.Attack()`：
- 解析受击目标
- 写入 `PlayerEntity.CurrentTarget`
- 阵营校验
- 组装 `DamageRequest`
- 调用 `BattleDamageService.ApplyDamage()`

### 怪物攻击
`MonsterBrain.OnAttackEvent()`：
- 先做真实命中检测
- 组装 `DamageRequest`
- 调用 `BattleDamageService.ApplyDamage()`

## 11.3 伤害结算

`BattleDamageService.ApplyDamage()` 的当前规则：

```text
1. 请求与目标合法性检查
2. 同阵营拦截
3. 读取 attacker/target 属性
4. 闪避判定
5. 暴击判定
6. 基础伤害 = rawDamage + Attack - Defense
7. finalDamage >= 1
8. target.ReceiveDamage(finalDamage)
9. 发布 DamageAppliedEvent
10. 若死亡，发布 DeathEvent
```

### 当前特点
- 结算逻辑集中，未分散在玩家或怪物对象里
- 支持暴击、闪避、受击飘字
- `HitRate` 已进入结果快照，但当前命中逻辑主要用于属性记录，未形成完整命中率对抗公式

### 当前局限
- 缺少护盾、持续伤害、减伤 Buff、元素伤害、控制效果等更复杂战斗层
- 技能结算仍只有 Damage 真正落地，Heal / RestoreStamina 仍为预留

---

## 12. 死亡与复活工程化设计

## 12.1 死亡总线

`DeathEvent` 是死亡副作用统一入口。

来源：
- `BattleDamageService` 击杀后发布
- `PlayerEntity.ReceiveDamage()` 在本地死亡时也会发布

## 12.2 死亡副作用收口

`DeathRuntimeService` 当前负责：

### 对怪物
- 去重处理，防止重复死亡清理
- 停止导航
- 从 `NavigationRegistry` 注销
- 从 `MonsterRuntimeRegistry` 注销
- 写入死亡动画
- 关闭碰撞
- 延迟销毁对象

### 对玩家
- 打印死亡日志
- 清空怪物目标
- 发布 `PlayerDeadEvent`

### 工程价值
- 把“死亡后的副作用”统一管理，而不是分散在各个实体内部
- 玩家和怪物可以共享同一死亡事件机制，再在 runtime service 中分流处理

## 12.3 玩家复活执行

`PlayerRespawnRuntimeService.RespawnNow()` 当前顺序：

```text
1. 防重入检查
2. 锁控制
3. 停导航
4. ReviveFull()
5. 传送到出生点
6. CaptureRuntimeSnapshot()
7. 清 DeathRuntime 去重标记
8. 关闭 Overlay
9. 延迟恢复控制
```

### 当前已落地效果
- 复活位置不会被旧导航拉回
- 复活后状态会同步到玩家运行态快照
- 下次死亡仍可被正常处理

### 当前缺口
- 代码中未看到明确的“复活短暂无敌”实现，后续若需要应在此模块集中补充

---

## 13. 掉落、背包与物品工程化设计

## 13.1 掉落链路

```text
DeathEvent
  → LootRuntimeService.OnDeath()
  → 判断 deadEntity 是否 MonsterEntity
  → 读取 MonsterConfig.dropTableId
  → LootResolver.Roll(dropTable)
  → InventoryService.TryAddItem(currentPlayerData, itemId, count)
```

### 特点
- 直接入包，不生成地面掉落物
- 当前掉落为“抽一条”最小策略
- 适合作为最小闭环，但不适合复杂掉落设计

## 13.2 背包模型

```text
InventoryData
├─ slotCount
└─ slots: List<InventorySlotData>

InventorySlotData
├─ slotIndex
├─ itemId
└─ count
```

### 当前规则
- 同 itemId 优先堆叠
- 堆满后再开新格子
- 超容量直接失败
- 移除物品后会重建 `slotIndex`

### 当前 UI 模型
- `InventoryPanel` 只读展示全部槽位
- 点击槽位会发布 `OpenItemDetailPopupEvent`
- `ItemDetailPopup` 承担使用/丢弃操作

## 13.3 物品效果模型

`ItemUseEffectSystem` 当前通过字符串到效果处理器映射的方式实现：
- `heal_hp`
- `restore_stamina`

### 工程价值
- 物品效果不直接硬编码在 UI 或背包里
- 后续可继续扩展为策略表 / ScriptableObject / 数据驱动效果链

### 当前问题
`ItemConfig.json` 中存在 `stamina_up` 类型，但 `ItemUseEffectSystem` 未注册该 key，说明配置与实现尚未完全对齐。

---

## 14. 成长与技能工程化设计

## 14.1 成长系统

### 经验发放
`PlayerExpRuntimeService` 订阅 `DeathEvent`：
- 仅当 deadEntity 是怪物、killer 是玩家时发经验
- 经验值来自 `MonsterConfig.expReward`

### 升级逻辑
`PlayerProgressionService`：
- 支持经验累计与连跳级
- 升级后增长基础属性
- 升级后恢复满血与满体力
- 发布 HP / Stamina UI 事件
- 自动刷新技能解锁列表

### 当前特点
- 成长系统已经从 UI 独立成服务层
- 经验公式集中在 `PlayerProgressionFormula`

## 14.2 技能系统

### 当前正式技能链

```text
Main logic / Debug input
  → PlayerSkillService.CastSkill()
      → 施法者合法性检查
      → 技能配置读取
      → 是否已解锁
      → 冷却检查
      → target / range 校验
      → ExecuteEffect()
          → Damage → PlayerAttackService.Attack(..., DamageSourceType.Skill)
```

### 目标策略
`PlayerSkillTargetResolver`：
1. 优先使用 `PlayerEntity.CurrentTarget`
2. 若当前目标不存在或已死，则搜索范围内最近怪物

### 当前状态评估
- 技能服务层已成型
- 技能与普攻已经共享 Battle 结算链
- 但 UI、动画、特效、正式输入接线尚未完成

### Prototype 模块
`Game/Skill/Prototype` 仍保留一套独立技能原子系统原型，用于验证对象模型，不属于当前正式主链。

---

## 15. 资源与 AssetBundle 工程化

## 15.1 资源组织

当前资源主要放在 `Assets/Resources/` 下：
- `Config/`
- `UI/`
- `Monster/`
- `Map/`
- `Role/`
- `Portrait/`

运行时资源路径全部应通过 `AssetPaths` 获取。

## 15.2 AssetBundle 命名策略

通过 `AssetBundleNameByDirectoryEditor`：
- 以 `Assets/Resources` 为根
- 按目录映射 bundle name
- 规则示例：`ab.ui.windows`、`ab.config`、`ab.monster`

## 15.3 AB 打包与加载

编辑器工具：
- `Tools/AssetBundle/1. 刷新 AssetBundleName`
- `Tools/AssetBundle/2. 清空所有 AssetBundleName`
- `Tools/AssetBundle/3. 打包 Windows`
- `Tools/AssetBundle/4. 打包 Android`
- `Tools/AssetBundle/5. 打包 iOS`

运行时：
- `AssetBundleManager.Initialize()` 加载 manifest 与依赖
- `AssetBundleLoader` 优先尝试 AB
- 失败后回退 `Resources`

### 工程价值
- 给项目保留了资源发布与热更方向的基础结构
- 但当前并未完全替代 `Resources`

---

## 16. 场景、地图与小地图

## 16.1 场景职责

### BeginScene
- 启动 UI
- 开始游戏 / 继续游戏 / 设置 / 关于 / 退出

### GameScene
- 玩家、相机、小地图、怪物运行时主场景
- 当前所有实际战斗与探索行为都发生在这里

## 16.2 小地图绑定

`MiniMapAssembler` 当前负责：
- 查找 `MiniMapCamera`
- 获取 `MiniMapCameraController`
- 加载 RenderTexture
- 注册到 `MiniMapService`
- 将玩家目标绑定为小地图跟随目标

### 工程意义
- 小地图相关逻辑不直接硬写在 `GameSceneEntry` 内部
- 场景对象查找与资源加载被单独收口

---

## 17. 测试、调试与可观测性

## 17.1 当前状态

项目存在 `Assets/Tests/EditMode/`：
- `DataManagerTests.cs`
- `CreateRoleFlowControllerTests.cs`
- `UIManagerTests.cs`

但这些测试代码目前整体被注释，不能视为已启用自动化测试。

## 17.2 当前主要依赖的验证手段
- 手工走主链路
- 控制台日志
- DebugCanvas / PoolMonitor
- 热键调试（技能、经验、怪物伤害）
- Gizmos 可视化命中区

## 17.3 工程建议

后续建议优先恢复以下自动化测试：
- 存档槽位与 Schema 修复
- 创角流程事件闭环
- `BattleDamageService` 的公式与死亡事件
- `InventoryService` 的堆叠/容量规则
- `PlayerProgressionService` 的升级与技能解锁

---

## 18. 当前已知工程问题与技术债

### 18.1 明确存在的问题
1. `MainPanel` 技能按钮仍未接入正式技能服务
2. `ItemConfig` 与 `ItemUseEffectSystem` 的体力恢复 key 不一致
3. EditMode 测试文件处于注释状态
4. AboutPanel 文案陈旧，与当前分支能力不一致
5. Build Settings 仍有 `SampleScene`

### 18.2 结构层面的待收口点
1. `GameManager` 仍承担一部分 Game-only Controller 初始化，生命周期注册尚未完全覆盖
2. `UI 主页面` 与 `Popup` 在部分路由定义上语义仍不够统一
3. 资源系统虽然已有 AB 工具链，但发布链和校验链还不完整
4. Debug 日志较多，缺少统一开关与分级
5. 技能正式链与 Prototype 链并存，后续需要明确是否保留双轨

---

## 19. 当前工程约束与维护规则

### 19.1 新功能接入建议
新增功能优先遵守以下规则：

- 新资源路径先补 `AssetPaths`
- 新对象查找名先补 `ObjectNames`
- 跨模块协作优先走 `EventBus`
- 运行时创建优先用 `Assembler / RuntimeService`
- 场景副作用优先放在 Commit 阶段
- 战斗逻辑优先接入 `BattleDamageService`
- 死亡副作用优先挂到 `DeathEvent` 后面扩展
- 存档结构变更同步补 `EnsurePlayerDataSchema()`

### 19.2 避免的做法
- 不要在 UI 里直接写战斗结算
- 不要在多个脚本里同时写 Animator 同一组参数
- 不要在场景脚本里重复实例化玩家/怪物而绕过 RuntimeService
- 不要新增硬编码资源路径

---

## 20. 总结

以当前分支状态看，UnityDemo_MMORPG 已经不再是只有界面和角色创建的早期 Demo，而是一个已经具备以下特征的原型工程：

- 运行主链路完整
- Player / Monster / Navigation / Battle / Inventory / Progression / Respawn 已形成初步模块边界
- 场景装配、生命周期、资源路径、存档恢复已开始工程化
- 技能系统、AssetBundle 工具链、测试体系虽未完全落地，但已经有可继续收口的结构基础

这意味着后续工作的重点，不应再是“从零搭系统”，而应是继续把当前链路做深、做稳、做一致。

