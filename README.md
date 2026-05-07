# UnityDemo_MMORPG

基于 **Unity 2022.3.62f3c1** 的 MMORPG 客户端原型工程。

当前分支：`feat/combat-hit-verify-respawn-overlay`

本项目已经具备从 **BeginScene → 创角/读档 → GameScene → 玩家/怪物运行时 → 战斗/死亡/掉落/背包/成长/复活** 的最小可运行闭环，整体方向明确偏向“**先打通主链路，再逐步工程化收口**”。

---

## 1. 项目定位

UnityDemo_MMORPG 不是一套完整商用 MMORPG 客户端，而是一个围绕以下目标持续演进的原型工程：

- 打通最小可运行主链路
- 明确 Framework / Game 分层边界
- 将资源路径、对象名、路由名等统一常量化
- 把场景装配、战斗结算、死亡副作用、存档恢复等高频逻辑从“散点脚本”收口为可维护模块
- 为后续技能 UI、装备系统、任务系统、服务端同步等扩展留出结构空间

---

## 2. 当前完成度概览

### 已完成的功能

#### 基础启动与场景流转
- `BeginScene` 启动入口可用
- `GameManager` 常驻并负责应用级初始化
- `RuntimeLifecycleRegistry` 已用于统一编排核心模块初始化
- 支持从开始界面进入创角流程
- 支持多存档继续游戏
- 支持进入 `GameScene` 后完成玩家、相机、主界面、小地图、怪物模块装配
- 支持从游戏内返回开始界面

#### UI Framework 与主界面骨架
- 已实现 `BasePanel + UIManager + UILayer + PopupStack + UIMask`
- 已区分 MainPage / Popup 的展示语义
- `BeginPanel / CreateRolePanel / ContinuePanel / MainPanel / MapPanel / InventoryPanel / SettingPanel / AboutPanel / MessageTipPanel / ConfirmPanel / RoleInfoPanel / ItemDetailPopup` 均已接入
- 主界面已具备名称、等级、HP、体力、小地图、背包、地图、角色详情、设置入口

#### 创角与角色数据
- 已支持职业切换、角色命名、职业详情查看
- 已支持按职业配置生成 `PlayerData`
- 已支持创角后自动分配存档槽位并进入 `GameScene`
- `RoleClassConfig.json` 已提供职业基础属性、成长字段与初始技能字段

#### 玩家运行时
- `PlayerRuntimeService` 已作为玩家运行时创建统一入口
- 已完成玩家实体、输入代理、移动控制、体力系统、导航代理、定位注册的装配
- 已支持从存档位置恢复玩家位置与朝向
- 已支持玩家 HP / 体力实时事件广播，主界面同步刷新

#### 导航与地图
- 已基于 `NavMesh` 实现寻路
- 已支持玩家点击导航、导航停止、路径可视化状态维护
- 已支持自动导航期间自由转镜，WASD / 左摇杆输入打断自动导航
- 已支持小地图相机绑定与地图坐标映射
- 已支持 `MapConfig.json` 驱动的地图显示

#### 怪物系统
- 已形成 `MonsterEntity / MonsterBrain / MonsterLocomotionExecutor / MonsterNavigator / MonsterRuntimeService / MonsterRuntimeRegistry / MonsterModule` 分层
- 已支持刷怪点初始化、按权重刷怪、最大存活数控制、定时重生
- 已支持怪物状态流转：`Idle → Chase → Attack → Return → Idle`
- 已支持怪物从存档恢复
- 已支持攻击动画事件回调与失败保护超时
- 已支持怪物真实命中判定（`SphereCast + 前向角度门禁 + Gizmos`）

#### 战斗、伤害与死亡
- 已形成统一攻击路径：`ICombatSource / IDamageReceiver / DamageRequest / BattleDamageService`
- 已支持统一阵营校验、基础伤害公式、暴击、闪避、命中反馈事件
- 已支持玩家普攻与怪物攻击统一进入 `BattleDamageService`
- 已支持飘字对象池与受击反馈
- 已支持统一死亡事件 `DeathEvent`
- 已支持怪物死亡后的导航清理、注册表移除、碰撞关闭、延迟销毁
- 已支持玩家死亡事件转 UI 流程

#### 复活链路
- 已支持玩家死亡后黑屏 Overlay
- 已支持倒计时后自动复活，以及按钮立即复活
- 已支持复活时停止旧导航、恢复满血满体力、传送回出生点、恢复控制、清理死亡去重标记
- 已支持清空怪物仇恨目标

#### 掉落、背包与物品
- 已支持 `DropTableConfig.json` 驱动的最小掉落表
- 已支持怪物死亡后解析掉落并直接入包
- 已支持背包槽位、堆叠、容量限制、读档保留
- 已支持背包面板、格子展示、物品详情弹窗
- 已支持物品使用与物品丢弃
- 已支持部分可用物品效果：回血

#### 成长与经验
- 已支持击杀怪物获得经验
- 已支持统一经验公式 `Base100 + 每级+50`
- 已支持连续升级
- 已支持升级后恢复满血满体力
- 已支持技能解锁列表随等级刷新

#### 技能系统（服务层）
- 已提供 `SkillConfig.json`、`SkillConfigManager`、`PlayerSkillService`、`PlayerSkillTargetResolver`
- 已支持技能释放的解锁、目标、距离、冷却校验
- 已支持伤害型技能统一进入 `BattleDamageService`
- 已支持当前战斗目标 `CurrentTarget` 与“最近怪物”回退策略
- 已提供 `PlayerSkillDebugController` 用于数字键调试技能释放

#### 存档、恢复与资源加载
- 已支持多槽位玩家存档
- 已支持玩家运行时快照保存
- 已支持怪物运行态保存与恢复
- 已支持 Schema 修复与旧档兼容（如 progressData / inventoryData / runtimeData / stamina）
- 已提供 `Resources` 加载与 `AssetBundle` 运行时加载双通道
- 已提供 AssetBundle 命名与打包编辑器工具

---

### 待完善的功能

#### 体验层
- 主界面技能按钮尚未正式接入 `PlayerSkillService`，当前按钮点击仍是“开发中”提示
- 技能表现目前以服务逻辑和调试入口为主，缺少正式技能栏、冷却遮罩、施法表现和动画联动
- 装备系统尚未接入，`ItemConfig` 中的 Equipment 物品仍处于数据占位阶段
- 世界掉落物、拾取交互、拖拽拆分、快捷栏等背包深层功能尚未实现
- 玩家受击反馈、无敌帧、硬直/禁控等战斗体验机制尚未系统化

#### 工程层
- 现有 EditMode 测试文件存在，但代码整体处于注释状态，自动化测试尚未真正启用
- `Build Settings` 中仍包含 `SampleScene`，项目发布前建议清理无关场景入口
- AssetBundle 运行时链路已具备，但仍以 `Resources` 为默认运行路径，缺少完整发布校验与资源校验工具
- 日志仍以 `Debug.Log` 为主，缺少统一日志级别、埋点与开关体系
- 目前尚未形成 asmdef 分层、CI 检查、代码扫描与预提交校验

#### 已确认的当前问题/缺口
- `MainPanel` 技能按钮尚未接到正式技能释放入口
- `PlayerSkillService` 中 `Heal / RestoreStamina` 技能效果仍为预留分支
- `ItemUseEffectSystem` 注册的是 `restore_stamina`，但 `ItemConfig.json` 中有体力恢复物品使用 `stamina_up`，两者未完全对齐，体力恢复类道具存在接线风险
- About 面板中的说明文案仍停留在较早阶段，和当前分支实际能力不一致

---

## 3. 运行环境

- Unity Editor: `2022.3.62f3c1`
- 主要包：
  - `com.unity.ai.navigation`
  - `com.unity.cinemachine`
  - `com.unity.inputsystem`
  - `com.unity.render-pipelines.universal`
  - `com.unity.textmeshpro`
  - `com.unity.ugui`

---

## 4. 快速开始

### 4.1 打开工程
1. 使用 Unity Hub 打开项目目录
2. 确认编辑器版本为 `2022.3.62f3c1`
3. 等待 Package 与资源导入完成

### 4.2 场景
项目 Build Settings 当前包含：
- `Assets/Scenes/BeginScene.unity`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Scenes/GameScene.unity`

实际主流程使用：
- `BeginScene`
- `GameScene`

### 4.3 推荐运行方式
1. 打开 `BeginScene`
2. 点击 Play
3. 在开始界面选择：
   - 开始游戏：进入创角，再进入 GameScene
   - 继续游戏：选择已有存档槽位
4. 进入 `GameScene` 后验证：
   - 主界面显示
   - 小地图正常绑定
   - 玩家与怪物正常运行
   - 战斗、掉落、背包、复活链路可用

### 4.4 可选：AssetBundle 运行时
如需测试 AB 运行时加载：
1. 通过编辑器菜单生成 AssetBundleName
2. 执行目标平台打包
3. 将产物放入 `Assets/StreamingAssets/<Platform>/`
4. 打开宏 `ENABLE_ASSETBUNDLE_RUNTIME`

未启用宏时，运行时默认仍走 `Resources`。

---

## 5. 核心运行链路

### 5.1 启动链路

```text
StartGame
  → GameManager.Initialize()
  → RuntimeLifecycleBootstrap.RegisterDefaults()
  → RuntimeLifecycleRegistry.InitAll()
  → Init Game-only Controllers
  → MonsterConfigManager.Init()
  → EnterBeginFlow()
```

### 5.2 创角链路

```text
BeginPanel
  → CreateRolePanel
  → CreateRoleRequestEvent
  → CreateRoleFlowController
  → PlayerFactory.CreatePlayerData()
  → GamePlayerDataService.SavePlayerDataToSlot()
  → SceneNavigator.EnterGameScene()
```

### 5.3 读档链路

```text
BeginPanel
  → ContinuePanel
  → GamePlayerDataService.LoadPlayerDataFromSlot(slotId)
  → SceneNavigator.EnterGameScene()
```

### 5.4 场景装配链路

```text
GameSceneEntry.Start()
  → TryAssembleScene()
      → CameraRigAssembler.TryCreate()
      → PlayerRuntimeService.CreateRuntimePlayer()
      → MiniMapAssembler.TryInitObjects()
  → CommitScene()
      → OpenMainPanel()
      → WriteSceneContext()
      → Bind MiniMap
      → MonsterModule.InitForScene()
      → EnsureBattleRuntime()
```

### 5.5 战斗与死亡链路

```text
PlayerAttackService / MonsterBrain.OnAttackEvent
  → CombatRequestFactory.CreateBasicDamage()
  → BattleDamageService.ApplyDamage()
  → DamageAppliedEvent
  → DeathEvent (if killed)
  → DeathRuntimeService
```

### 5.6 玩家死亡与复活链路

```text
PlayerEntity.ReceiveDamage()
  → DeathEvent
  → DeathRuntimeService
  → PlayerDeadEvent
  → PlayerDeathUIController
  → RespawnOverlayRuntime.Show()
  → PlayerRespawnRuntimeService.RespawnNow()
```

### 5.7 掉落与背包链路

```text
DeathEvent
  → LootRuntimeService
  → LootResolver
  → InventoryService.TryAddItem()
  → InventoryChangedEvent
  → InventoryPanel / ItemDetailPopup 刷新
```

### 5.8 经验成长链路

```text
DeathEvent
  → PlayerExpRuntimeService
  → PlayerProgressionService.AddExpToCurrentPlayer()
  → LevelUp()
  → RefreshUnlockedSkillsForCurrentPlayer()
```

---

## 6. 目录结构

```text
Assets/
├─ Editor/                          # 编辑器工具（AssetBundle 命名与打包）
├─ Resources/                       # 运行时资源
│  ├─ Config/                       # Role / Monster / Map / Item / Drop / Skill 配置
│  ├─ Map/
│  ├─ Monster/
│  ├─ Portrait/
│  ├─ Role/
│  └─ UI/
├─ Scenes/
│  ├─ BeginScene.unity
│  └─ GameScene.unity
├─ Scripts/
│  ├─ Framework/                    # 基础设施层
│  │  ├─ Consts/
│  │  ├─ Event/
│  │  ├─ Json/
│  │  ├─ Managers/
│  │  ├─ Pool/
│  │  ├─ Resource/
│  │  ├─ Scene/
│  │  └─ UI/
│  └─ Game/                         # 业务层
│     ├─ Battle/
│     ├─ Boot/
│     ├─ CharacterControl/
│     ├─ Combat/
│     ├─ Drop/
│     ├─ Flow/
│     ├─ GameScene/
│     ├─ Inventory/
│     ├─ Item/
│     ├─ Map/
│     ├─ Monster/
│     ├─ Navigation/
│     ├─ Player/
│     ├─ Save/
│     ├─ Skill/
│     └─ UI/
└─ Tests/                           # 当前存在 EditMode 测试骨架，但尚未启用

ProjectSettings/
Packages/
docs/
daily_logs/
```

---

## 7. 重点模块说明

### Framework

| 模块 | 作用 |
|---|---|
| `UIManager` | Panel 加载、分层、Popup 栈、主页面切换 |
| `EventBus` | 模块间事件通信 |
| `DataManager` | 设置数据与存档槽位管理 |
| `ResourceManager` | 统一资源加载入口 |
| `AssetBundleManager` | AB 依赖加载与资源获取 |
| `PoolManager` | 飘字、路径段等对象池 |
| `AssetPaths / ObjectNames / SceneNames / UIStrings` | 统一常量定义 |

### Game

| 模块 | 作用 |
|---|---|
| `Boot` | 应用初始化、生命周期编排 |
| `Flow` | 创角流程与业务事件处理 |
| `GameScene` | 场景装配与运行态上下文写入 |
| `Player` | 玩家实体、控制、导航、复活、成长、存档 |
| `Monster` | 怪物实体、AI、执行器、刷怪、恢复 |
| `Navigation` | 寻路构建、代理注册、点击移动 |
| `Combat / Battle` | 统一攻击入口、伤害结算、死亡处理 |
| `Drop / Inventory / Item` | 掉落、入包、物品使用与丢弃 |
| `Skill` | 数据驱动技能服务与调试入口 |
| `Map / UI` | 小地图、地图页、主界面与弹窗系统 |

---

## 8. 配置文件

位于 `Assets/Resources/Config/`：

- `RoleClassConfig.json`：职业基础属性与成长字段
- `MonsterConfig.json`：怪物属性、掉落表、经验奖励、Prefab 路径
- `MapConfig.json`：场景地图配置
- `ItemConfig.json`：物品配置、堆叠、用途、描述
- `DropTableConfig.json`：掉落表
- `SkillConfig.json`：技能配置

---

## 9. 调试与开发辅助

### 已有调试入口
- `K` 键附近最近怪伤害调试（怪物死亡/掉落/复活链路验证）
- `1` 键技能调试释放（`PlayerSkillDebugController`）
- `F6 / F7` 经验调试（成长链路验证）
- 开发模式下可注入 `DebugCanvas + PoolMonitorPanel`
- 怪物命中区支持 Gizmos 可视化

### 建议重点验证
- 创角后自动入场
- ContinuePanel 读档/删档
- 玩家导航与镜头不互相打断
- 玩家死亡 Overlay 与复活恢复
- 击杀怪物后的经验、掉落、背包变化
- 物品使用与丢弃
- 存档后再次读档是否恢复玩家位置与怪物状态

---

## 10. 已知工程约束

- `Framework` 只承载基础设施，不应写具体业务规则
- 场景副作用尽量集中在 `CommitScene()` 阶段执行
- 动画写入尽量由单一执行者负责，避免 Brain / Navigator / Animator 多头写入
- 资源路径、对象名、路由名禁止硬编码，统一收口到常量类
- 玩家与怪物的运行态创建应尽量统一走 RuntimeService / Assembler，不直接在场景脚本里散点生成

---

## 11. 文档

- 架构文档：`docs/architecture.md`
- 开发日志：`daily_logs/`
- 工程建议：`docs/engineering-recommendations-2026-03-25.md`
- 运行最小检查清单：`docs/minimal-runtime-checklist.md`

---

## 12. 建议的下一步

本分支最适合优先补齐的，不是“再加更多系统”，而是把现有闭环继续做实：

1. 正式接通技能栏 UI 与 `PlayerSkillService`
2. 修正体力恢复类道具配置/效果映射
3. 启用并补齐 EditMode / PlayMode 测试
4. 清理发布前无关场景与陈旧文案
5. 为背包、装备、技能和成长建立更稳定的数据与事件边界

