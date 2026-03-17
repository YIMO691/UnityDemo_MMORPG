# UnityDemo_MMORPG

一个基于 Unity 的 MMORPG 客户端原型项目，围绕“最小可运行主链路”构建：BeginScene → 创角/读档 → GameScene → PlayerArmature / Camera / UI。项目采用 Framework / Game / Editor 三层程序集划分，业务与基础能力解耦，支持多存档与角色控制模块化。

## 功能特性

- 分层 UI 与路由
  - 分层管理（Bottom/Normal/Popup/Top），根画布通过 Resources 自动实例化
  - 统一生命周期：OnCreate / OnShow / OnHide / OnDestroyPanel
  - 主页面单例显示，弹窗栈与遮罩 UIMask
  - 路由清单与主页面注册：UIRouteNames、UIMainPages
- 多存档与摘要
  - 保存/读取/删除槽位、查询空槽位/最大已用槽位、任意存档判断
  - PlayerSaveMetaMapper 统一构建摘要列表
- 事件与流程
  - 轻量 EventBus 发布/订阅
  - CreateRoleFlowController 打通创角 → 存档 → 注入当前玩家/槽位 → 进入 GameScene
  - RoleUIController 负责打开职业详情
- 角色控制与输入
  - CharacterControl 模块：StarterAssetsInputs、ThirdPersonController、BasicRigidBodyPush
  - Input System 绑定唯一 StarterAssets.inputactions（Game 下）
- 场景与相机
  - 入口/装配器：GameSceneEntry、PlayerCharacterAssembler、CameraRigAssembler
  - 相机降级：缺虚拟相机时主相机跟随，保证有画面
- 资源与数据
  - ResourceManager 统一加载；DataManager 管理设置与多存档；RoleDataManager 读职业配置

## 目录结构（核心）

```
Assets
├─ Resources
│  ├─ UI
│  │  ├─ Root
│  │  │  ├─ PanelCanvas.prefab
│  │  │  └─ UIMask.prefab
│  │  └─ Windows
│  │     ├─ BeginPanel.prefab
│  │     ├─ CreateRolePanel.prefab
│  │     ├─ ContinuePanel.prefab
│  │     ├─ MainPanel.prefab
│  │     ├─ MessageTipPanel.prefab
│  │     ├─ RoleInfoPanel.prefab
│  │     ├─ ConfirmPanel.prefab
│  │     └─ AboutPanel.prefab
│  ├─ Config
│  │  └─ RoleClassConfig.json
│  └─ Role/PlayerAmature
│     ├─ PlayerArmature.prefab
│     ├─ MainCamera.prefab
│     └─ PlayerFollowCamera.prefab
└─ Scripts
   ├─ Framework（基础设施）
   │  ├─ Consts / EventBus / UIManager / ResourceManager / DataManager / JsonMgr
   │  ├─ UI（Base / Services / UIMainPages / UIRouteNames 等）
   │  └─ MMORPG.Framework.asmdef
   ├─ Game（业务）
   │  ├─ Boot（AppRuntimeInitializer / GameBootstrapper / StartGame）
   │  ├─ CharacterControl
   │  │  ├─ Input（StarterAssets.inputactions / StarterAssetsInputs.cs）
   │  │  └─ ThirdPerson（ThirdPersonController / BasicRigidBodyPush）
   │  ├─ GameScene（Entry / Assemblers）
   │  ├─ Entity（RoleClass / Factory / Data/Runtime / Save/Mapper）
   │  ├─ UI（Controllers / Panels：Pages / Popups / Items）
   │  └─ MMORPG.Game.asmdef
   └─ Editor（工具）
      ├─ Config（RoleClassConfigEditorWindow）
      ├─ Scene（SceneMenu）
      └─ MMORPG.Editor.asmdef
```

## 快速开始

1) 打开工程并加载场景  
- 推荐从 Assets/Scenes/BeginScene.unity 启动  
- 确认 Build Settings 已包含 BeginScene 与 GameScene  

2) 运行游戏  
- 启动入口 [StartGame.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Boot/StartGame.cs) 初始化基础服务与路由，显示 BeginPanel  
- “开始游戏”进入创建角色流程 → 创角后立即保存至槽位并注入当前玩家与槽位 → 进入 GameScene  
- “继续游戏”在有存档时打开 ContinuePanel  
- 主界面点击头像，打开 RoleInfoPanel 显示职业详情

最小运行链路与必备依赖见：  
- [docs/minimal-runtime-checklist.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/minimal-runtime-checklist.md)

## UI 与事件

- 面板基类：[BasePanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Base/BasePanel.cs)
- UI 管理器：[UIManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/UIManager.cs)
- 对话服务（通用确认）：[UIDialogService.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Services/UIDialogService.cs)
- 常用事件：OpenPanelEvent / OpenMainPageEvent / OpenRoleInfoPanelEvent / CreateRoleRequestEvent（位于 Framework 与 Game 的事件定义中）

## 创建角色与数据

- 流程控制：[CreateRoleFlowController.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Flow/CreateRoleFlowController.cs)
- 数据工厂：[PlayerFactory.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Entity/Factory/PlayerFactory.cs)
- 职业配置加载：[RoleDataManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Entity/RoleClass/Manager/RoleDataManager.cs)（来源：Resources/Config/RoleClassConfig.json）
- 存档摘要映射：[PlayerSaveMetaMapper.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Save/Mapper/PlayerSaveMetaMapper.cs)

## 场景与相机

- 入口：[GameSceneEntry.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/GameScene/Entry/GameSceneEntry.cs)
- 装配器：[PlayerCharacterAssembler.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/GameScene/Assemblers/PlayerCharacterAssembler.cs)、[CameraRigAssembler.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/GameScene/Assemblers/CameraRigAssembler.cs)
- 降级策略：缺虚拟相机或绑定失败时，主相机自动放置为跟随 LookAt 角色目标

## 常见问题（FAQ）

- 没有 UI  
  - 确认 Resources/UI/Root/PanelCanvas.prefab 存在且 UIManager 已初始化
- 相机无画面  
  - 确认 Resources/Role/PlayerAmature 下 MainCamera 与 PlayerFollowCamera 存在；或依赖降级由运行时创建主相机
- 头像详情无信息  
  - 确认 RoleDataManager 已加载配置；创角后已写入当前玩家与槽位
- 输入无效  
  - 确认 PlayerArmature 上 PlayerInput 的 Actions 指向 Game/CharacterControl/Input/StarterAssets.inputactions

## 规划

- 迁移 Player* 数据模型到 Game，并轻改 DataManager 接口以稳定数据流
- 清理 StarterAssets 未使用资源并产出清单
- 引入 Play Mode/编辑器测试覆盖主链路
