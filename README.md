# UnityDemo_MMORPG

基于 Unity 2022.3.62f3 的 MMORPG 客户端原型，强调“最小可运行主链路 + 可持续扩展”。采用 Framework / Game / Editor 三层架构，主流程为：BeginScene → 创角/读档 → GameScene → PlayerArmature / Camera / UI。

## 亮点特性

- 分层架构可演进
  - Framework：UI 框架、事件总线、资源与设置、路径常量
  - Game：角色控制、场景装配、业务流程与页面、存档服务
  - Editor：配置编辑与场景工具
- UI 与路由
  - 分层渲染（Bottom/Normal/Popup/Top），主页面独占呈现
  - 路由常量与主页面注册（UIRouteNames、UIMainPages）
  - 通用确认弹窗通过路由+反射解耦
- 角色控制与输入
  - StarterAssets 驱动：ThirdPersonController、StarterAssetsInputs、BasicRigidBodyPush
  - 唯一 StarterAssets.inputactions（GUID 固定）绑定到 PlayerInput
- 场景装配与相机降级
  - GameSceneEntry + Assemblers（角色/相机）
  - 无虚拟相机或绑定失败时，主相机自动降级为跟随 LookAt
- 存档体系
  - GamePlayerDataService 管理当前玩家与槽位读写
  - PlayerSaveMetaMapper 构建 Continue 列表摘要
- 文档完善
  - 最小链路清单与模块接入模板，保障后续开发制度化

## 快速开始

1) 打开项目（Unity 2022.3.62f3）  
2) 安装/确认包  
   - Cinemachine、Input System、TextMeshPro  
3) 设置 Build Settings  
   - 加入 Assets/Scenes/BeginScene.unity 与 Assets/Scenes/GameScene.unity  
4) 校验输入绑定  
   - PlayerArmature 的 PlayerInput.m_Actions 指向  
     Assets/Scripts/Game/CharacterControl/Input/StarterAssets.inputactions  
   - GUID 为 4419d82f33d36e848b3ed5af4c8da37e  
5) 运行 BeginScene  
   - 开始游戏 → 创角 → 自动存档并进入 GameScene  
   - 继续游戏 → 打开 ContinuePanel 选择存档  
6) 头像详情  
   - 在 MainPanel 点击头像，弹出 RoleInfoPanel（职业详情）

最小运行依赖与自检清单：  
- [docs/minimal-runtime-checklist.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/minimal-runtime-checklist.md)

## 场景与核心脚本

- BeginScene  
  - 启动入口：[StartGame.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Boot/StartGame.cs)  
  - 主页面：BeginPanel（创建/继续/设置/关于/退出）
- GameScene  
  - 场景入口：[GameSceneEntry.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/GameScene/Entry/GameSceneEntry.cs)  
  - 装配器：  
    - [PlayerCharacterAssembler.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/GameScene/Assemblers/PlayerCharacterAssembler.cs)  
    - [CameraRigAssembler.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/GameScene/Assemblers/CameraRigAssembler.cs)
- 流程与服务  
  - 创角流程：[CreateRoleFlowController.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Flow/CreateRoleFlowController.cs)  
  - 存档服务：[GamePlayerDataService.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Save/GamePlayerDataService.cs)、[PlayerSaveService.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Save/PlayerSaveService.cs)

## 目录速览

```
Assets
├─ Scenes
│  ├─ BeginScene.unity
│  └─ GameScene.unity
│
├─ Resources
│  ├─ UI
│  │  ├─ Root
│  │  │  ├─ PanelCanvas.prefab
│  │  │  └─ UIMask.prefab
│  │  └─ Windows
│  │     ├─ Pages
│  │     │  ├─ BeginPanel.prefab
│  │     │  ├─ CreateRolePanel.prefab
│  │     │  ├─ ContinuePanel.prefab
│  │     │  └─ MainPanel.prefab
│  │     ├─ Popups
│  │     │  ├─ RoleInfoPanel.prefab
│  │     │  ├─ MessageTipPanel.prefab
│  │     │  └─ ConfirmPanel.prefab
│  │     └─ AboutPanel.prefab
│  │
│  ├─ Role
│  │  └─ PlayerAmature
│  │     ├─ PlayerArmature.prefab
│  │     ├─ MainCamera.prefab
│  │     └─ PlayerFollowCamera.prefab
│  │
│  └─ Config
│     └─ RoleClassConfig.json
│
└─ Scripts
   ├─ Framework                          # 基础设施（无业务模型）
   │  ├─ UI
   │  │  ├─ Base
   │  │  │  └─ BasePanel.cs
   │  │  ├─ Services
   │  │  │  └─ UIDialogService.cs
   │  │  ├─ UIMainPages.cs
   │  │  └─ UIRouteNames.cs
   │  ├─ Event
   │  │  ├─ EventBus.cs
   │  │  └─ EventDefine
   │  │     └─ UIEvents/*.cs
   │  ├─ Managers
   │  │  ├─ UIManager.cs
   │  │  ├─ ResourceManager.cs
   │  │  ├─ AudioManager.cs
   │  │  └─ DataManager.cs                # 设置/槽位/文件名/基础 IO
   │  ├─ Json
   │  │  └─ JsonMgr.cs
   │  └─ Consts
   │     ├─ AssetPaths.cs
   │     └─ UIPaths.cs
   │
   ├─ Game                                # 业务域
   │  ├─ Boot
   │  │  ├─ AppRuntimeInitializer.cs
   │  │  ├─ GameBootstrapper.cs
   │  │  └─ StartGame.cs
   │  │
   │  ├─ CharacterControl
   │  │  ├─ Input
   │  │  │  ├─ StarterAssets.inputactions
   │  │  │  └─ StarterAssetsInputs.cs
   │  │  └─ ThirdPerson
   │  │     ├─ ThirdPersonController.cs
   │  │     └─ BasicRigidBodyPush.cs
   │  │
   │  ├─ GameScene
   │  │  ├─ Entry
   │  │  │  └─ GameSceneEntry.cs
   │  │  └─ Assemblers
   │  │     ├─ PlayerCharacterAssembler.cs
   │  │     └─ CameraRigAssembler.cs
   │  │
   │  ├─ Entity
   │  │  ├─ Player
   │  │  │  └─ Data
   │  │  │     ├─ Base
   │  │  │     │  ├─ PlayerData.cs
   │  │  │     │  ├─ PlayerBaseData.cs
   │  │  │     │  └─ PlayerAttributeData.cs
   │  │  │     ├─ Progress
   │  │  │     │  └─ PlayerProgressData.cs
   │  │  │     └─ Runtime
   │  │  │        └─ PlayerRuntimeData.cs
   │  │  └─ RoleClass
   │  │     ├─ Data
   │  │     │  ├─ RoleClassConfig.cs
   │  │     │  └─ RoleClassConfigList.cs
   │  │     └─ Manager
   │  │        └─ RoleDataManager.cs
   │  │
   │  ├─ Save
   │  │  ├─ Mapper
   │  │  │  └─ PlayerSaveMetaMapper.cs
   │  │  ├─ GamePlayerDataService.cs
   │  │  └─ PlayerSaveService.cs
   │  │
   │  ├─ Flow
   │  │  └─ CreateRoleFlowController.cs
   │  │
   │  └─ UI
   │     ├─ Controllers
   │     │  └─ RoleUIController.cs
   │     └─ Panels
   │        ├─ Pages
   │        │  ├─ BeginPanel.cs
   │        │  ├─ CreateRolePanel.cs
   │        │  ├─ ContinuePanel.cs
   │        │  ├─ MainPanel.cs
   │        │  └─ SettingPanel.cs
   │        ├─ Popups
   │        │  ├─ RoleInfoPanel.cs
   │        │  ├─ MessageTipPanel.cs
   │        │  └─ AboutPanel.cs
   │        └─ Items
   │           └─ ContinueSlotItem.cs
   │
   └─ Editor                              # 编辑器扩展
      ├─ Config
      │  └─ RoleClassConfigEditorWindow.cs
      └─ Scene
         └─ SceneMenu.cs
```

## UI 与事件清单

- UI 基础  
  - 基类：[BasePanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Base/BasePanel.cs)  
  - 管理器：[UIManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/UIManager.cs)  
  - 路由/主页面：[UIRouteNames.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/UIRouteNames.cs)、[UIMainPages.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/UIMainPages.cs)  
  - 通用确认：[UIDialogService.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Services/UIDialogService.cs)
- 常用事件  
  - 打开/关闭面板：OpenPanelEvent / ClosePanelEvent / OpenMainPageEvent  
  - 创角请求：CreateRoleRequestEvent  
  - 职业详情：OpenRoleInfoPanelEvent

## 数据与存档

- 职业配置  
  - 管理器：[RoleDataManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Entity/RoleClass/Manager/RoleDataManager.cs)  
  - 数据源：Resources/Config/RoleClassConfig.json  
- 玩家数据模型（Game 层）  
  - Base/Progress/Runtime/Attribute 位于 Game/Entity/Player/Data 下  
- 存档读写  
  - 槽位与文件名：DataManager（Framework）  
  - 业务数据读写：GamePlayerDataService（Game/Save）  
  - 摘要映射：[PlayerSaveMetaMapper.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Save/Mapper/PlayerSaveMetaMapper.cs)

## 输入与相机

- 输入  
  - 资产：Assets/Scripts/Game/CharacterControl/Input/StarterAssets.inputactions（GUID: 4419d82f33d36e848b3ed5af4c8da37e）  
  - 代码：[StarterAssetsInputs.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/CharacterControl/Input/StarterAssetsInputs.cs)
- 相机  
  - 跟随相机使用 CinemachineVirtualCamera；失败则降级  
  - 降级逻辑见 CameraRigAssembler.FallbackBind

## 架构与接入规范

- 架构详解：  
  - [docs/architecture.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/architecture.md)
- 新模块接入模板：  
  - [docs/module-integration-template.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/module-integration-template.md)

## 常见问题（FAQ）

- 启动后没有 UI  
  - 检查 Resources/UI/Root/PanelCanvas 是否存在；UIManager.Init 是否执行  
- 相机无画面  
  - 检查 MainCamera 与 PlayerFollowCamera 预制是否存在；或依赖降级由运行时自动创建主相机  
- 头像详情为空  
  - 确认 RoleDataManager 已加载配置；创角成功后已 SetCurrentPlayerData / SetCurrentSlotId  
- 输入无效  
  - 确认 PlayerInput 绑定的 Actions 指向唯一的 StarterAssets.inputactions（GUID 匹配）

## 路线图

- Player* 数据完全归口 Game，PlayerSaveMetaData 迁至 Game/Save  
- Addressables/AssetBundle 接入抽象 ResourceManager  
- Play Mode/编辑器测试覆盖主链路与关键模块  
- 背包/装备/技能等系统，按接入模板制度化接入
