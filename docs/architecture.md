# Architecture Guide

本文是 UnityDemo_MMORPG 的工程化架构指南，聚焦“稳定的最小运行链路 + 清晰的职责边界 + 可持续扩展”。配套：
- 最小链路清单：docs/minimal-runtime-checklist.md
- 新模块接入模板：docs/module-integration-template.md

## 目录
- 层次与原则
- 关键运行链路
- 模块职责矩阵
- 目录结构（摘要）
- Actor 行为模型
- 导航策略
- 存档与恢复
- 常量与资源规范
- UI 系统规范
- 调试与工具
- 测试与验收
- 提交与工程规范
- 术语与边界

---

## 层次与原则

- Framework（基础设施，MMORPG.Framework）
  - 职责：通用能力与可复用基础设施，不含业务规则
  - 组成：UI 框架（BasePanel/UIManager/UIMainPages/UIRouteNames）、事件总线（EventBus）、资源加载（ResourceManager）、数据与 JSON（DataManager/JsonMgr）、常量（AssetPaths/UINames/UIStrings/ObjectNames/NavigationConsts）
  - 依赖原则：精简引用；禁止引用 MMORPG.Game
- Game（业务域，MMORPG.Game）
  - 职责：业务逻辑、流程控制、实体模型、UI 页面、场景装配、角色控制与输入
  - 组成：CharacterControl、GameScene（Entry/Assemblers）、Player（Config/Data/Factory/Assemblers/Runtime/Navigation/Save）、Monster（Config/Factory/Runtime/Navigation/Save/Spawner/Assembler/Debug）、Navigation（Contracts/Events/Runtime{PathSolver/Registry/Components}）、Map（Manager/Runtime/UI）、Flow、UI（Controllers/Panels）
  - 依赖：Cinemachine / Unity.InputSystem / Unity.TextMeshPro
- Editor（编辑器扩展，MMORPG.Editor）
  - 职责：工具与工作流（配置编辑器、场景菜单等），仅在 Editor 编译

边界约束：Framework 不得依赖 Game 类型；跨层交互通过事件、接口或路由+反射（例如 ConfirmPanel 的显示与数据注入）。

---

## 关键运行链路

BeginScene → 创角/读档 → GameScene → PlayerArmature / Camera / UI

- 启动与初始化
  - Game/Boot：AppRuntimeInitializer（BeforeSceneLoad 初始化 DataManager、UIManager、RoleDataManager、CreateRoleFlowController、RoleUIController）、GameBootstrapper（兜底）、StartGame（打开 BeginPanel）
- 创角流程
  - CreateRolePanel 发布 CreateRoleRequestEvent
  - CreateRoleFlowController：PlayerFactory 生成 PlayerData → GamePlayerDataService.SavePlayerDataToSlot → 注入当前玩家与槽位 → SceneNavigator 进入 GameScene
- 场景装配
  - GameSceneEntry → PlayerRuntimeService.CreateRuntimePlayer（装配→Init→ApplySnapshot→SetAgentId→Register）→ CameraRigAssembler 绑定 → 打开 MainPanel
- 角色详情
  - MainPanel 点击头像 → 发布 OpenRoleInfoPanelEvent → RoleUIController 显示 RoleInfoPanel 并填充

最小依赖详见最小链路清单。

---

## 模块职责矩阵

- UI 框架（Framework/UI）
  - BasePanel：OnCreate/OnShow/OnHide/OnDestroyPanel 生命周期
  - UIManager：画布/分层/弹窗栈/主页面（ShowPanel/ShowMainPage/Hide/Destroy）
  - UIDialogService：路由+反射调用 ConfirmPanel.SetData
  - UIMainPages / UIRouteNames：主页面注册与路由常量
- 数据与设置
  - DataManager（Framework）：设置数据持久化、槽位编号与文件名、基础 IO；不再持有 PlayerData 业务对象
  - GamePlayerDataService（Game/Player/Save）：当前玩家内存态、存档读写、摘要列表（复用 DataManager 槽位/文件名与 JsonMgr）
  - RoleDataManager（Game/Player/Config）：职业配置读取
  - PlayerSaveMetaMapper（Game/Player/Save/Mapper）：PlayerData → PlayerSaveMetaData
- 角色控制（Game/CharacterControl）
  - Input：StarterAssets.inputactions（唯一 GUID）、StarterAssetsInputs
  - ThirdPerson：ThirdPersonController、BasicRigidBodyPush
- 场景（Game/GameScene）
  - Entry：GameSceneEntry（TryCreate Camera → TryAssemble Player → TryBind/降级 → Show MainPanel）
  - Assemblers：PlayerCharacterAssembler、CameraRigAssembler
- 流程（Game/Flow）
  - CreateRoleFlowController：订阅创角事件，写存档、注入内存，切换场景
  - RoleUIController：订阅 OpenRoleInfoPanelEvent，显示并填充 RoleInfoPanel
- 怪物（Game/Monster）
  - RuntimeRegistry：Register/Unregister/Get/HasAlive/CountAliveBySpawnPoint
  - RuntimeService：CreateFromSpawnPoint/RestoreFromSave（恢复后 SpawnPosition=SpawnPoint）
  - Module：场景门面（Restore→InitSpawnPoints）
  - SpawnPoint：点位配置与补怪入口（按 Registry 统计活怪）

- 地图系统（Game/Map）
  - MapConfig：每张地图世界边界与图片资源标识（worldMin/Max X/Z, mapImage）
  - MapDataManager：从 Resources/Config/MapConfig.json 加载
  - MapService：获取当前场景、玩家位置、当前地图配置（runtime.Scene 为空时回退 activeScene）
  - MapPanel：显示地图名称、图片、玩家标点；标点依据 MapConfig 世界边界与 MapImage RectTransform 左下角原点映射
  - PlayerLocator（Game/Player/Runtime）：注册/获取玩家 Transform/PlayerEntity，优先实时位置

- 自动寻路（Game/Navigation）
  - Contracts：INavigationAgent、NavigationMoveRequest
  - Events：NavigationMoveRequestEvent、NavigationStopRequestEvent
  - Runtime：NavigationPathSolver（NavMesh.SamplePosition/CalculatePath）、NavigationRegistry（代理注册表）、Components/BaseNavigator（通用路径驱动）
  - Player 导航：Game/Player/Navigation/PlayerNavigator（将路径转为 StarterAssetsInputs 移动；支持取消保护时间与手动输入接管）
  - UI：MapClickReceiver（点击地图 → 世界坐标转换 → 发布 NavigationMoveRequest）

---

## 依赖与命名规范

- 路由与资源
  - 所有 UI 打开使用 UIRouteNames；窗口预制位于 Resources/UI/Windows/<PanelName>.prefab，脚本同名
  - 公用资源路径集中到 AssetPaths（统一来源），便于重构与资产替换
- 事件与跨层
  - 通用事件（UI 路由等）放在 Framework/EventDefine；业务事件放在 Game/Events
  - 通过 EventBus 解耦模块，禁止直接跨层持对象引用
- 数据模型命名
  - PlayerXxxData、XxxConfig、XxxRequest、XxxMetaData（摘要）
  - 业务模型放 Game/Entity/<Domain>/Data/{Base/Progress/Runtime}；摘要映射放 Game/Save/Mapper
- 程序集引用
  - MMORPG.Framework：不引用 Game；保留最小依赖
  - MMORPG.Game：按需引用 Cinemachine/Input System/TextMeshPro
  - 导航相关：依赖 Unity NavMesh（需在场景中 Bake 可走区域）

---

## 资源与输入规范

- 角色与相机（Resources/Role/PlayerAmature）
  - PlayerArmature：需含 CharacterController、ThirdPersonController、StarterAssetsInputs、PlayerInput、PlayerCameraRoot、CinemachineCameraTarget
  - MainCamera：Camera、AudioListener、CinemachineBrain（如安装）
  - PlayerFollowCamera：CinemachineVirtualCamera（运行时绑定 Follow/LookAt）
- 输入资产
  - 仅保留 Game/CharacterControl/Input/StarterAssets.inputactions（GUID: 4419d82f33d36e848b3ed5af4c8da37e）；PlayerInput.m_Actions 必须指向此资产
  - 光标策略：应用获得焦点时解锁并显示鼠标，保证 UI 可点击；纯移动/战斗场景按需锁定

---

## 存档与恢复

- 文件命名：DataManager.GetPlayerSlotFileName(slotId) → "player_<id>"
- 写入流程：GamePlayerDataService.SavePlayerDataToSlot → JsonMgr.SaveData
- 读档流程：GamePlayerDataService.LoadPlayerDataFromSlot → JsonMgr.LoadData
- 摘要：PlayerSaveMetaMapper 将 PlayerData 转为 PlayerSaveMetaData，用于 Continue 列表
- 版本：若数据结构变更，PlayerData 增加 version 字段，新增升级器（Upgrade Pipeline）在读档时迁移
 - 运行时更新：进入场景后写入 runtime.Scene 与当前坐标，避免首次“未知地图”
- Player：PlayerEntity.CaptureRuntimeSnapshot/ApplyRuntimeSnapshot（唯一快照入口）
- Monster：RuntimeService.RestoreFromSave（isDead/去重；SpawnPosition=SpawnPoint，当前位置=存档位置）

---

## 相机降级方案

- TryCreate：优先从 Resources 加载 MainCamera/PlayerFollowCamera；若缺失则动态创建主相机（Camera+AudioListener+可选 CinemachineBrain）
- TryBind：有虚拟相机则 Follow/LookAt 绑定 CinemachineCameraTarget
- FallbackBind：无虚拟相机或绑定失败时，将主相机放置在角色后上方并 LookAt，确保总有画面

---

## UI 系统规范

- UILayer：Bottom/Normal/Popup/Top
- 弹窗栈：Popup 入栈，UIMask 跟随栈顶显示；CloseByMask 控制遮罩点击关闭
- 主页面：ShowMainPage 保证同一时间只有一个主页面在显示（可配置是否隐藏旧主页面）

---

## 目录结构（摘要）

```
Assets/Scripts
├─ Framework/                 # 基础设施（无业务模型）
│  ├─ UI/{Base,Services,UIMainPages,UIRouteNames}
│  ├─ Event/{EventBus,EventDefine}
│  ├─ Managers/{UIManager,ResourceManager,AudioManager,DataManager}
│  ├─ Json/{JsonMgr}
│  └─ Consts/{AssetPaths,UINames,UIStrings,ObjectNames,NavigationConsts}
├─ Game/                      # 业务域
│  ├─ Boot/{AppRuntimeInitializer,GameBootstrapper,StartGame}
│  ├─ CharacterControl/{Input,ThirdPerson}
│  ├─ GameScene/{Entry,Assemblers,RuntimeSceneCommitter}
│  ├─ Runtime/{GameRuntime}
│  ├─ Navigation/
│  │  ├─ Contracts/{INavigationAgent,NavigationMoveRequest}
│  │  └─ Runtime/{NavigationPathSolver,NavigationRegistry,Components/{BaseNavigator}}
│  ├─ Player/
│  │  ├─ Config/{RoleClassConfig,RoleClassConfigList,PlayerVisualConfig}
│  │  ├─ Data/{Base/{PlayerData,PlayerBaseData},Progress/{PlayerProgressData},Attribute/{PlayerAttributeData},Runtime/{PlayerRuntimeData}}
│  │  ├─ Factory/{PlayerFactory}
│  │  ├─ Assemblers/{PlayerCharacterAssembler}
│  │  ├─ Runtime/{PlayerEntity,PlayerLocator,PlayerInputProxy,PlayerLocomotionBrain,PlayerRuntimeService}
│  │  ├─ Navigation/{PlayerNavigator}
│  │  └─ Save/{GamePlayerDataService,PlayerSaveService,Mapper/{PlayerSaveMetaMapper}}
│  ├─ Monster/
│  │  ├─ Config/{MonsterConfig,MonsterConfigList,MonsterConfigManager}
│  │  ├─ Factory/{MonsterFactory}
│  │  ├─ Runtime/{MonsterEntity,MonsterBrain,MonsterLocomotionExecutor,MonsterAnimatorDriver,MonsterAnimationEvents,MonsterStateType,MonsterRuntimeRegistry,MonsterRuntimeService,MonsterModule}
│  │  ├─ Navigation/{MonsterNavigator,MonsterAgentId}
│  │  ├─ Save/{MonsterSaveData,MonsterSaveService}
│  │  ├─ Spawner/{MonsterSpawnPoint,MonsterSpawner}
│  │  ├─ Assembler/{MonsterAssembler}
│  │  └─ Debug/{MonsterDamageDebugInput}
│  ├─ Map/
│  │  ├─ Manager/{MapDataManager}
│  │  ├─ Runtime/{MiniMapService,MiniMapAssembler,MiniMapCameraController,MapService}
│  │  └─ UI/{MapPathSegmentItem}
│  ├─ Flow/{CreateRoleFlowController,RoleAnimationEventReceiver}
│  └─ UI/{Controllers,Panels/{Pages,Popups,Items}}
└─ Editor/{Config,Scene}
```

---

## 新模块接入流程

新增系统前，复制并填写“新增模块接入模板”。关键检查点：
- 分层定位（Framework / Game）与理由
- 运行态初始化与生命周期
- 持久化版本与文件命名
- UI 路由、层级与预制路径
- 事件契约与发布/订阅位置
- 资源路径常量是否更新
- 输入与相机需求
- 数据模型归口目录
- asmdef 依赖更新与循环依赖检查
- 最小链路回归通过

---

## 测试与验收

- BeginScene → 创角/读档 → GameScene 全链路运行通过
- 主页面/弹窗按路由正常打开/关闭，遮罩行为正确
- 存档写入/读取/摘要列表展示正确
- Console 无跨层引用/脚本错误
- README/architecture/模板文档已同步

---

## 提交与工程规范

- 分批提交：逻辑/资源/场景/AnimatorController/Prefab 分离
- 信息格式：模块名 + 简述 + 关键影响
- 发布前检查：无未提交更改；README/architecture/模板同步

---

## Actor 行为模型

- 命令语义：ActorControlCommand（moveDirection/lookDirection/jump/sprint/attack/stop）
- Player：PlayerLocomotionBrain（命令）→ ThirdPersonController（执行）；PlayerControlCommand 提供与 Actor 命令的互转
- Monster：MonsterBrain（命令）→ MonsterLocomotionExecutor（执行：Navigator.MoveTo/Stop、Animator.SetIdle/SetChase/TriggerAttack）
- 优先级：Attack > Stop > Move；动画写入单一来源（Executor）
- 首帧速度空窗：执行器按帧刷新 + 意图保持，避免“移动首帧仍为 Idle”

---

## 导航策略

- 事件流
  - 求解：NavigationPathSolver → corners（NavMeshPath）
  - 执行：Navigator.SetPath(corners, stopDistance)
- 门禁规则
  - Navigator 不读取业务状态（除 IsDead）
- 距离与停止策略
  - stopDistance 建议小值（如 0.2），避免“未移动即判到达”
  - 路线角点仅高度差时推进到下一角点，避免原地抖动
- 动画融合
  - Speed 使用 CurrentSpeed（真实位移速度），避免用配置常量
  - 转向仅平面旋转，避免俯仰导致“趴下”

---

## 调试与工具

- DebugCanvas
  - 资源路径：AssetPaths.DebugCanvas
  - PoolMonitorPanel：AssetPaths.PoolMonitorPanel
  - GameSceneEntry.EnsureDebugCanvas 自动接入
- 导航调试
  - 记录每次路径下发与拒收原因（Attack/Dead/门禁冷却）
  - 关键日志：收到请求、路径求解状态（PathComplete/Partial/Invalid）、角点数量
- 怪物调试
  - K 键最近怪伤害：MonsterDamageDebugInput（测试 Die/OnDead/重生链路）
  - 状态机切换日志：Idle/Chase/Attack/Return/Dead
- 路径硬编码扫描：Editor/Tools/PathUsageScanner（Tools/Path Scanner 菜单）

---

## 常量与资源规范

- 资源路径常量：AssetPaths
  - 示例：UI/Root/DebugCanvas、UI/Windows/PoolMonitorPanel、Map/Main/、Monster/
- 对象名常量：ObjectNames
  - 示例：MiniMapCamera、PlayerCameraRoot
- 导航 Agent 常量：NavigationConsts
  - 示例：PlayerAgentId
- 规范
  - 禁止魔法字符串；新增资源与对象名必须先补常量
  - 统一通过 ResourceManager.Instance.Load<T>(AssetPaths.Xxx) 加载

---

## 动画集成规范

- 参数约定
  - Speed：0–正值；由真实速度驱动
  - IsChasing：布尔；追击期为 true，Attack/Idle 为 false
  - Dead：布尔；死亡为 true
  - Attack：触发器；由逻辑定时或事件触发
- 过渡与事件
  - Attack → Locomotion：勾选 Has Exit Time 或增加条件（如 IsChasing=true）
  - 在攻击剪辑末帧添加 AttackOver 事件，调用 MonsterAnimationEvents.AttackOver → MonsterEntity.OnAttackOver
- Root Motion
  - 目前不启用 Root Motion；位移由导航驱动
  - 若启用 Root Motion，必须移除脚本位移，保持单一位移源

---

## 代码风格与行为约束

- 单一位移源：避免脚本位移与 Root Motion 双驱动
- 攻击控制：Attack 期间仅停止导航；不使用“固定时间 + 强制回位”
- 冷却与防抖：Return→Chase 冷却；进入追击冷却避免同帧进攻
- 事件解耦：跨模块通信用 EventBus；禁止直接持有跨层对象
- 常量化：路径、对象名、AgentId 统一常量引用

---

## 性能与测试

- 对象池：Framework/Pool（IPoolable、ObjectPool、PoolManager）
- 导航频率：路径下发节流（pathInterval），避免高频求解
- 测试
  - Play Mode：黄金链路、导航下发门禁、怪物状态机
  - Edit Mode：配置加载、常量引用、ResourceManager

---

## 术语与边界
- App：应用级生命周期（GameManager、场景切换）
- Scene Entry：场景装配入口（GameSceneEntry），两阶段（组装→提交）
- Assembler：装配器（相机/角色/小地图等），只负责构建与绑定，不做业务决策
- Service：跨模块能力（RuntimeService/UIDialogService/PlayerSaveService）
- Registry：运行态注册表（NavigationRegistry/MonsterRuntimeRegistry/PlayerLocator）
- Agent：导航代理（PlayerNavigator/MonsterNavigator），只接受路径与停止指令
- Event：领域事件（NavigationMoveRequestEvent 等），用于解耦发布/订阅

---

## 22. 错误处理与健壮性策略
- 资源加载失败
  - ResourceManager.Load 返回 null 时早退并记录错误；必要时提供降级（相机 Fallback）
- 事件处理
  - 订阅方需判空与状态早退（IsDead/Attack 门禁）；避免抛异常阻断总线
- 导航求解失败
  - PathSolver 返回 Invalid/Partial 时记录日志并不下发；调用方可重试或降级走向目标点
- 怪物状态机
  - AttackOver 未触发：使用失败保护超时自动解锁；日志提示 Animator 配置缺失
- 存档与版本
  - 若字段缺失或版本不匹配：升级器迁移；保留旧文件备份并打印版本信息

---

## 23. Animator 配置检查清单
- 控制器
  - 基础移动与攻击控制器存在；怪物 Prefab 正确挂载 Animator 且控制器引用有效
- 参数
  - Speed（float）、IsChasing（bool）、Dead（bool）、Attack（trigger）
- 过渡
  - Attack → Locomotion：Has Exit Time 或条件（IsChasing）
  - Dead：进入后保持，退出需重生流程重置
- 事件
  - 攻击末帧添加 AttackOver，接口指向 MonsterAnimationEvents.AttackOver
- Root Motion
  - 若未使用 Root Motion，则关闭 Apply Root Motion；避免与脚本位移冲突

---

## 24. 配置与版本管理规范
- JSON 文件
  - 所有配置位于 Resources/Config，统一通过 JsonMgr 读取
  - 推荐包含 version 字段，便于将来迁移
- 路径与命名
  - 资源路径常量化（AssetPaths），配置文件名与类名一致
- 升级流程
  - 读档时根据 version 选择升级器；升级后写回新版本，旧版本保留备份

---

## 25. 性能预算与节流建议
- 导航
  - 路径求解节流（pathInterval ≥ 0.3s）；批量怪物时分批下发请求
- 动画
  - Speed 采用真实速度，Blend 节点数量合理；避免过多触发器抖动
- 事件总线
  - 高频事件合并或降采样；订阅方尽量无阻塞执行
- 对象池
  - Map 路径段、临时指示器进入 Pool；避免频繁 Instantiate/Destroy

---

## 26. 测试策略细化
- Play Mode
  - 黄金链路：Begin→创角→GameScene→MainPanel 打开
  - 怪物行为：Idle→Chase→Attack→Return→Idle 循环；AttackOver 正常触发；失败保护生效
  - 导航门禁：Attack/Dead 拒收路径；stopDistance 正确导致抵达
- Edit Mode
  - 配置加载：Role/Map/Monster JSON 正确解析；AssetPaths 引用存在
  - 常量化检查：随机扫描代码避免魔法字符串；CI 提示异常引用
