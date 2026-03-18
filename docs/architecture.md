# 架构总规（Architecture Specification, Detailed）

本文定义 UnityDemo_MMORPG 的整体架构、边界与接入规范，确保“最小运行链路稳定 + 模块化扩展一致”。配套文档：
- 最小链路清单：[docs/minimal-runtime-checklist.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/minimal-runtime-checklist.md)
- 新模块接入模板：[docs/module-integration-template.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/module-integration-template.md)

---

## 1. 分层模型与程序集

- Framework（基础设施，MMORPG.Framework）
  - 职责：通用能力与可复用基础设施，不含业务规则
  - 组成：UI 框架（BasePanel/UIManager/UIMainPages/UIRouteNames）、事件总线（EventBus）、资源加载（ResourceManager）、设置与基础存储（DataManager/JsonMgr）、常量（AssetPaths/UIPaths）
  - 依赖原则：精简引用；禁止引用 MMORPG.Game
- Game（业务域，MMORPG.Game）
  - 职责：业务逻辑、流程控制、实体模型、UI 页面、场景装配、角色控制与输入
  - 组成：CharacterControl、GameScene（Entry/Assemblers）、Entity（含 Player/RoleClass 等）、Flow、UI（Controllers/Panels）、Save（Mapper/Service）
  - 依赖：Cinemachine / Unity.InputSystem / Unity.TextMeshPro
- Editor（编辑器扩展，MMORPG.Editor）
  - 职责：工具与工作流（配置编辑器、场景菜单等），仅在 Editor 编译

边界约束：Framework 不得依赖 Game 类型；跨层交互通过事件、接口或路由+反射（例如 ConfirmPanel 的显示与数据注入）。

---

## 2. 黄金链路（Minimal Gold Path）

BeginScene → 创角/读档 → GameScene → PlayerArmature / Camera / UI

- 启动与初始化
  - Game/Boot：AppRuntimeInitializer（BeforeSceneLoad 初始化 DataManager、UIManager、RoleDataManager、CreateRoleFlowController、RoleUIController）、GameBootstrapper（兜底）、StartGame（打开 BeginPanel）
- 创角流程
  - CreateRolePanel 发布 CreateRoleRequestEvent
  - CreateRoleFlowController：PlayerFactory 生成 PlayerData → GamePlayerDataService.SavePlayerDataToSlot → 注入当前玩家与槽位 → SceneNavigator 进入 GameScene
- 场景装配
  - GameSceneEntry → PlayerCharacterAssembler（实例化骨架、挂载职业外观、设置相机目标）→ CameraRigAssembler（创建/绑定相机；失败降级为主相机跟随）→ 打开 MainPanel
- 角色详情
  - MainPanel 点击头像 → 发布 OpenRoleInfoPanelEvent → RoleUIController 显示 RoleInfoPanel 并填充

最小依赖详见最小链路清单。

---

## 3. 模块职责矩阵

- UI 框架（Framework/UI）
  - BasePanel：OnCreate/OnShow/OnHide/OnDestroyPanel 生命周期
  - UIManager：画布/分层/弹窗栈/主页面（ShowPanel/ShowMainPage/Hide/Destroy）
  - UIDialogService：路由+反射调用 ConfirmPanel.SetData
  - UIMainPages / UIRouteNames：主页面注册与路由常量
- 数据与设置
  - DataManager（Framework）：设置数据持久化、槽位编号与文件名、基础 IO；不再持有 PlayerData 业务对象
  - GamePlayerDataService（Game/Save）：当前玩家内存态、存档读写、摘要列表（复用 DataManager 槽位/文件名与 JsonMgr）
  - RoleDataManager（Game）：职业配置加载（Resources/Config/RoleClassConfig.json）
  - PlayerSaveMetaMapper（Game/Save/Mapper）：PlayerData → PlayerSaveMetaData
- 角色控制（Game/CharacterControl）
  - Input：StarterAssets.inputactions（唯一 GUID）、StarterAssetsInputs
  - ThirdPerson：ThirdPersonController、BasicRigidBodyPush
- 场景（Game/GameScene）
  - Entry：GameSceneEntry（TryCreate Camera → TryAssemble Player → TryBind/降级 → Show MainPanel）
  - Assemblers：PlayerCharacterAssembler、CameraRigAssembler
- 流程（Game/Flow）
  - CreateRoleFlowController：订阅创角事件，写存档、注入内存，切换场景
  - RoleUIController：订阅 OpenRoleInfoPanelEvent，显示并填充 RoleInfoPanel

---

## 4. 依赖与命名规范

- 路由与资源
  - 所有 UI 打开使用 UIRouteNames；窗口预制位于 Resources/UI/Windows/<PanelName>.prefab，脚本同名
  - 公用资源路径集中到 AssetPaths/UIPaths，便于重构与资产替换
- 事件与跨层
  - 通用事件（UI 路由等）放在 Framework/EventDefine；业务事件放在 Game/Events
  - 通过 EventBus 解耦模块，禁止直接跨层持对象引用
- 数据模型命名
  - PlayerXxxData、XxxConfig、XxxRequest、XxxMetaData（摘要）
  - 业务模型放 Game/Entity/<Domain>/Data/{Base/Progress/Runtime}；摘要映射放 Game/Save/Mapper
- 程序集引用
  - MMORPG.Framework：不引用 Game；保留最小依赖
  - MMORPG.Game：按需引用 Cinemachine/Input System/TextMeshPro

---

## 5. 资源与输入规范

- 角色与相机（Resources/Role/PlayerAmature）
  - PlayerArmature：需含 CharacterController、ThirdPersonController、StarterAssetsInputs、PlayerInput、PlayerCameraRoot、CinemachineCameraTarget
  - MainCamera：Camera、AudioListener、CinemachineBrain（如安装）
  - PlayerFollowCamera：CinemachineVirtualCamera（运行时绑定 Follow/LookAt）
- 输入资产
  - 仅保留 Game/CharacterControl/Input/StarterAssets.inputactions（GUID: 4419d82f33d36e848b3ed5af4c8da37e）；PlayerInput.m_Actions 必须指向此资产

---

## 6. 存档与版本策略

- 文件命名：DataManager.GetPlayerSlotFileName(slotId) → "player_<id>"
- 写入流程：GamePlayerDataService.SavePlayerDataToSlot → JsonMgr.SaveData
- 读档流程：GamePlayerDataService.LoadPlayerDataFromSlot → JsonMgr.LoadData
- 摘要：PlayerSaveMetaMapper 将 PlayerData 转为 PlayerSaveMetaData，用于 Continue 列表
- 版本：若数据结构变更，PlayerData 增加 version 字段，新增升级器（Upgrade Pipeline）在读档时迁移

---

## 7. 相机降级方案

- TryCreate：优先从 Resources 加载 MainCamera/PlayerFollowCamera；若缺失则动态创建主相机（Camera+AudioListener+可选 CinemachineBrain）
- TryBind：有虚拟相机则 Follow/LookAt 绑定 CinemachineCameraTarget
- FallbackBind：无虚拟相机或绑定失败时，将主相机放置在角色后上方并 LookAt，确保总有画面

---

## 8. UI 分层与遮罩

- UILayer：Bottom/Normal/Popup/Top
- 弹窗栈：Popup 入栈，UIMask 跟随栈顶显示；CloseByMask 控制遮罩点击关闭
- 主页面：ShowMainPage 保证同一时间只有一个主页面在显示（可配置是否隐藏旧主页面）

---

## 9. 工程目录（摘要）

```
Assets/Scripts
├─ Framework/                 # 基础设施（无业务模型）
│  ├─ UI/{Base,Services,UIMainPages,UIRouteNames}
│  ├─ Event/{EventBus,EventDefine}
│  ├─ Managers/{UIManager,ResourceManager,AudioManager,DataManager}
│  ├─ Json/{JsonMgr}
│  └─ Consts/{AssetPaths,UIPaths}
├─ Game/                      # 业务域
│  ├─ Boot/{AppRuntimeInitializer,GameBootstrapper,StartGame}
│  ├─ CharacterControl/{Input,ThirdPerson}
│  ├─ GameScene/{Entry,Assemblers}
│  ├─ Entity/
│  │  ├─ Player/Data/{Base,Progress,Runtime}
│  │  └─ RoleClass/{Data,Manager}
│  ├─ Save/{Mapper,GamePlayerDataService,PlayerSaveService}
│  ├─ Flow/{CreateRoleFlowController}
│  └─ UI/{Controllers,Panels/{Pages,Popups,Items}}
└─ Editor/{Config,Scene}
```

---

## 10. 新模块接入流程

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

## 11. 验收与回归

- BeginScene → 创角/读档 → GameScene 全链路运行通过
- 主页面/弹窗按路由正常打开/关闭，遮罩行为正确
- 存档写入/读取/摘要列表展示正确
- Console 无跨层引用/脚本错误
- README/architecture/模板文档已同步

---

## 12. 后续演进

- 将 PlayerSaveMetaData 迁至 Game/Save（当依赖关系更明确时）
- Addressables/Bundle 化资源加载，ResourceManager 抽象加载后端
- 引入 Play Mode + Edit Mode 测试覆盖关键流程
