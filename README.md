# UnityDemo_MMORPG

基于 Unity 2022.3.62f3 的 MMORPG 客户端原型。目标：最小可运行主链路 + 明确职责边界 + 数据/资源常量化 + 可持续扩展。

**总体流程**
- BeginScene → 创角/读档 → 进入 GameScene → 角色/相机/UI 装配 → 导航/地图/怪物模块运行

**架构分层**
- Framework：UI 框架、事件总线、资源/路径常量、对象池、通用管理器
- Game：业务域（启动/流程/场景装配/存档/导航/地图/怪物/页面）
- Resources：UI、配置、怪物 Prefab、动画/AnimatorController

---

## 快速开始
- 打开 Unity 2022.3.62f3
- 必要包：Input System、Cinemachine、TextMeshPro
- Build Settings：添加 [BeginScene.unity](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scenes/BeginScene.unity) 与 [GameScene.unity](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scenes/GameScene.unity)
- 输入资产：在玩家对象 PlayerInput 的 Actions 指向 [StarterAssets.inputactions](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/CharacterControl/Input/StarterAssets.inputactions)
- 运行 BeginScene：开始游戏（创角→自动存档→进入游戏）或继续游戏（选择存档）

---

## 目录结构（已标准化）
```
Assets
├─ Scenes
│  ├─ BeginScene.unity
│  └─ GameScene.unity
├─ Resources
│  ├─ UI
│  │  ├─ Root（DebugCanvas 等）
│  │  └─ Windows（页面/弹窗）
│  ├─ Map
│  │  └─ Main（地图图片）
│  ├─ Monster（Zombie/Z1~Z4 等 Prefab）
│  ├─ AnimatorController/Monster（基础移动/攻击等控制器）
│  └─ Config（RoleClassConfig.json、MapConfig.json、MonsterConfig.json）
└─ Scripts
   ├─ Framework（UI/Event/Managers/Json/Consts/Pool）
   └─ Game
      ├─ Boot（GameManager、StartGame）
      ├─ CharacterControl（ThirdPersonController、StarterAssetsInputs、输入资产）
      ├─ GameScene（Entry/Assemblers/MiniMap/RuntimeSceneCommitter）
      ├─ Runtime（GameRuntime）
      ├─ Navigation
      │  ├─ Contracts（INavigationAgent、NavigationMoveRequest）
      │  └─ Runtime（NavigationPathSolver、NavigationRegistry、Components/BaseNavigator）
      ├─ Player
      │  ├─ Config（RoleClassConfig、RoleClassConfigList、PlayerVisualConfig）
      │  ├─ Data（Base/Progress/Attribute/Runtime）
      │  ├─ Factory（PlayerFactory）
      │  ├─ Assemblers（PlayerCharacterAssembler）
      │  ├─ Runtime（PlayerEntity、PlayerLocator、PlayerInputProxy、PlayerLocomotionBrain、PlayerRuntimeService）
      │  ├─ Navigation（PlayerNavigator）
      │  └─ Save（GamePlayerDataService、PlayerSaveService、Mapper/PlayerSaveMetaMapper）
      ├─ Monster（Config/Assembler/Navigation/Runtime/Factory/Save/Spawner/Debug）
      ├─ Map（Manager/Runtime/UI）
      ├─ Flow（CreateRoleFlowController 等）
      └─ UI（Panels/Pages/Popups 等）
```

---

## 启动与场景装配
- 应用级管理器：[GameManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Boot/GameManager.cs)
  - 单例 + DontDestroyOnLoad
  - Initialize：核心管理器→怪物配置→进入 Begin 流程
  - ReturnToBeginFlow：清理小地图等→跳转 BeginScene
- Game 场景入口：[GameSceneEntry.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/GameScene/Entry/GameSceneEntry.cs)
  - 两阶段：TryAssembleScene(...) → CommitScene(...)
  - Assemble：PlayerRuntimeService.CreateRuntimePlayer（组装→Init→ApplySnapshot→SetAgentId→Register）
  - Commit：打开主页面、写入场景上下文、小地图绑定、MonsterModule.InitForScene（先恢复再初始化刷怪点）、EnsureBattleRuntime、DebugCanvas
- 小地图装配：[MiniMapAssembler.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Map/Runtime/MiniMapAssembler.cs)
- 场景运行时提交：[RuntimeSceneCommitter.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/GameScene/RuntimeSceneCommitter.cs)
- 常量
  - 资源路径：[AssetPaths.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Consts/AssetPaths.cs)
  - 对象名：[ObjectNames.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Consts/ObjectNames.cs)
  - 导航 Agent 常量：[NavigationConsts.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Consts/NavigationConsts.cs)
  - UI 名称常量：[UINames.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/UINames.cs)
  - UI 文案常量：[UIStrings.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Consts/UIStrings.cs)

---

## UI 系统
- 基类：[BasePanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Base/BasePanel.cs)
- 管理器：[UIManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/UIManager.cs)
- 路由与主页面：[UIRouteNames.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/UIRouteNames.cs)、[UIMainPages.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/UIMainPages.cs)
- 对话服务：[UIDialogService.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Services/UIDialogService.cs)
- 层级约定：Bottom/Normal/Popup/Top；主页面独占呈现

---

## 存档与数据
- 当前玩家与槽位： [GamePlayerDataService.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Player/Save/GamePlayerDataService.cs)
- 存档执行： [PlayerSaveService.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Player/Save/PlayerSaveService.cs)
- 槽位策略： [DataManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/DataManager.cs) 采用“尾部追加”（MaxUsedSlotId+1）
- 摘要映射： [PlayerSaveMetaMapper.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Player/Save/Mapper/PlayerSaveMetaMapper.cs)

---

## 导航与地图
- 求路/注册
  - NavigationPathSolver：基于 NavMesh 求路
  - NavigationRegistry：登记/注销 INavigationAgent
- 代理实现
  - PlayerNavigator（玩家）：[Game/Player/Navigation/PlayerNavigator.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Player/Navigation/PlayerNavigator.cs)
  - MonsterNavigator（怪物）：内部求路、平面转向、角点推进、速度采样
- 路径下发约束
  - Attack/Dead/Stun 门禁拒收
  - stopDistance 建议小值（如 0.2）避免“未移动即判到达”
- 地图与 UI
  - MapPanel：坐标映射与可视化
  - 静态图路径使用 AssetPaths.MapImageRoot

---

## 怪物系统
- 配置
  - JSON：[Resources/Config/MonsterConfig.json](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Resources/Config/MonsterConfig.json)
  - 管理器：[MonsterConfigManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Config/MonsterConfigManager.cs)
- 运行与职责分层
  - 实体：[MonsterEntity.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Runtime/MonsterEntity.cs)：仅持数据/身份/血量/死亡/存档
  - AI 脑：[MonsterBrain.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Runtime/MonsterBrain.cs)：只做状态判断与产生命令
  - 执行器：[MonsterLocomotionExecutor.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Runtime/MonsterLocomotionExecutor.cs)：统一执行命令（Navigator.MoveTo/Stop、Animator.SetIdle/SetChase/TriggerAttack），每帧按速度刷新动画并带意图保持
  - 导航代理：[MonsterNavigator.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Navigation/MonsterNavigator.cs)：内部求路（NavigationPathSolver），平面转向与速度采样
  - 动画驱动与事件：
    - 驱动器：[MonsterAnimatorDriver.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Runtime/MonsterAnimatorDriver.cs)
    - 事件转发：[MonsterAnimationEvents.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Runtime/MonsterAnimationEvents.cs) → MonsterBrain
- 运行时服务与注册表
  - 注册表：[MonsterRuntimeRegistry.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Runtime/MonsterRuntimeRegistry.cs)：Register/Unregister/Get/HasAlive/CountAliveBySpawnPoint
  - 服务：[MonsterRuntimeService.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Runtime/MonsterRuntimeService.cs)：CreateFromSpawnPoint/RestoreFromSave（恢复后归家点设为 SpawnPoint 位置）
  - 模块入口：[MonsterModule.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Runtime/MonsterModule.cs)：InitForScene（先恢复再初始化刷怪点）
- 装配与刷怪
  - 装配器：[MonsterAssembler.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Assembler/MonsterAssembler.cs)
  - 刷怪点：[MonsterSpawnPoint.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Spawner/MonsterSpawnPoint.cs)：按 spawnPointId 统计活怪补齐，不再维护本地列表

---

## 调试与工具
- DebugCanvas + PoolMonitorPanel
  - 资源：AssetPaths.DebugCanvas / AssetPaths.PoolMonitorPanel
  - 使用：GameSceneEntry.EnsureDebugCanvas()
- 怪物伤害调试（K 键）
  - [MonsterDamageDebugInput.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Monster/Debug/MonsterDamageDebugInput.cs)
  - 挂到玩家或任意空物体；按 K 对附近最近怪调用 TakeDamage()
- 路径硬编码扫描（Editor）
  - [PathUsageScanner.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Editor/Tools/PathUsageScanner.cs)
  - 菜单：Tools/Path Scanner/Scan Hardcoded Paths
  - 功能：扫描 Scripts 下的 .cs，报告疑似硬编码路径与 Resources/ResourceManager 的字面量路径使用（排除 AssetPaths.cs）

---

## 常量与资源规范
- 统一使用常量：AssetPaths（路径）/UINames（UI 名称）/UIStrings（UI 文案）/ObjectNames（运行期对象名）/PoolKey（对象池键）/NavigationConsts
- 禁止魔法字符串：新增资源/对象名先补常量再引用
- 资源加载：ResourceManager.Instance.Load<T>(AssetPaths.Xxx)
  - UI 窗口统一通过 AssetPaths.Window(UINames.Xxx) 访问

---

## 代码约定
- 单一移动源：避免脚本位移与 Root Motion 同时驱动
- 攻击控制：Attack 期间仅停止导航；退出由动画事件控制（AttackOver）
- 动画写入单一来源：由执行器统一写入；Stop 仅影响导航不强制改动画
- 导航门禁：Navigator 不读取业务状态（除 IsDead）；stopDistance 建议 0.2
- UI：主页面独占；弹窗经 UIDialogService；路由统一 UIRouteNames/UIMainPages；UI 名称统一来自 UINames

---

## 构建与运行自检
- 场景包含 BeginScene/GameScene；EventSystem 存在
- PlayerInput 指向唯一输入资产
- 资源路径与对象名引用均来自常量
- DebugCanvas 可加载；Pool 监控可显示

---

## 故障排查
- 收到路径但不动
  - stopDistance 过大；建议 0.2
  - Attack 状态拒收路径；先退出 Attack（AttackOver 事件）
- 攻击后一直不动
  - Animator 未触发 AttackOver 或过渡不正确；检查 Has Exit Time/条件
  - 失败保护超时触发后应恢复追击
- 转向“趴下/闪烁”
  - 使用平面转向（y=0）；Animator 使用真实速度驱动；SkinnedMeshRenderer bounds 合理

---

## 提交与规范
- 提交信息：模块/资源/场景修改分批说明（中文或英文均可）
- 资源大改与代码逻辑拆分为多次 commit 便于回溯
- 推送前确认场景与脚本无未提交更改

---

## 参考文档
- 最小链路自检：[docs/minimal-runtime-checklist.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/minimal-runtime-checklist.md)
- 架构说明：[docs/architecture.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/architecture.md)
- 模块接入模板（避免硬编码）：[docs/module-integration-template.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/module-integration-template.md)
