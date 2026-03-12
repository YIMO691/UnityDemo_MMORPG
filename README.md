# UnityDemo_MMORPG

一个基于 Unity 的 MMORPG 客户端原型项目，包含按层级管理的 UI 框架、事件总线解耦、创建角色流程、设置与音频联动、资源与数据驱动等基础能力。

## 功能特性

- UI 框架
  - 分层管理（Bottom/Normal/Popup/Top），根画布通过 Resources 自动实例化
  - 面板生命周期统一：OnCreate / OnShow / OnHide / OnDestroyPanel，内置淡入淡出动画与交互控制
  - 主页面与弹窗管理：主页面单例显示，弹窗支持栈式管理与遮罩
  - 遮罩 UIMask：支持点击遮罩关闭顶层弹窗（按需开启）
- 事件系统
  - 轻量事件总线 EventBus，支持 Publish/Subscribe/Unsubscribe/Clear
  - 事件定义规范化（如 OpenPanelEvent、OpenMainPageEvent、OpenRoleInfoPanelEvent、CreateRoleRequestEvent 等）
- 创建角色流程
  - BeginPanel → 相机转场 → CreateRolePanel → RoleInfoPanel（详情弹窗）→ CreateRoleRequest
  - RoleDataManager 从 JSON 配置加载职业数据，Portrait 头像按职业展示
  - CreateRoleFlowController 响应创角事件，PlayerFactory 生成 PlayerData 并写入 DataManager
- 资源与数据
  - ResourceManager 统一资源加载（编辑器使用 Resources，运行时预留 AssetBundle 入口）
  - JsonMgr 支持 LitJson/JsonUtility 双方案；SettingData 持久化至 persistentDataPath
- 设置与音频
  - SettingPanel 管理音乐/音效开关与音量，实时生效并持久化
  - AudioManager 初始化与默认 BGM 播放，开机自动套用设置

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
│  │     ├─ SettingPanel.prefab
│  │     ├─ CreateRolePanel.prefab
│  │     └─ RoleInfoPanel.prefab
│  ├─ Config
│  │  └─ RoleClassConfig.json
│  └─ Portrait
│     ├─ infantry_portrait.png
│     ├─ sniper_portrait.png
│     ├─ medic_portrait.png
│     └─ engineer_portrait.png
└─ Scripts
   ├─ Boot
   │  └─ StartGame.cs
   ├─ Framework
   │  ├─ Data
   │  │  └─ SettingData.cs
   │  ├─ Event
   │  │  ├─ EventBus.cs
   │  │  └─ EventDefine
   │  │     ├─ GameEvents
   │  │     │  └─ BeginPanelCameraEvent.cs
   │  │     └─ UIEvents
   │  │        ├─ OpenPanelEvent.cs
   │  │        ├─ ClosePanelEvent.cs
   │  │        ├─ OpenMainPageEvent.cs
   │  │        ├─ OpenRoleInfoPanelEvent.cs
   │  │        └─ CreateRoleRequestEvent.cs
   │  ├─ Managers
   │  │  ├─ UIManager.cs
   │  │  ├─ ResourceManager.cs
   │  │  ├─ AudioManager.cs
   │  │  └─ DataManager.cs
   │  ├─ Json
   │  │  └─ JsonMgr.cs
   │  └─ UI
   │     ├─ Base
   │     │  ├─ BasePanel.cs
   │     │  └─ UIMask.cs
   │     └─ Panels
   │        ├─ BeginPanel.cs
   │        ├─ SettingPanel.cs
   │        ├─ CreatRolePanel.cs
   │        └─ RoleInfoPanel.cs
   └─ Game
      ├─ Flow
      │  └─ CreateRoleFlowController.cs
      └─ Entity
         ├─ RoleClass
         │  ├─ Data
         │  │  ├─ RoleClassConfig.cs
         │  │  └─ RoleClassConfigList.cs
         │  └─ Manager
         │     └─ RoleDataManager.cs
         ├─ Data
         │  ├─ PlayerData.cs
         │  ├─ PlayerBaseData.cs
         │  ├─ PlayerProgressData.cs
         │  ├─ PlayerAttributeData.cs
         │  ├─ PlayerRuntimeData.cs
         │  └─ CreateRoleRequest.cs
         └─ Factory
            └─ PlayerFactory.cs
```

## 快速开始

1) 打开工程并加载场景  
- 推荐从 BeginScene 开始（含主菜单与相机过渡）  
- 场景需包含：  
  - 主摄像机并挂载 [CameraEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/GameEvents/BeginPanelCameraEvent.cs)，Animator 内含 “Turn” 触发器，并在动画尾部触发 PlayerOver 事件  
  - [AudioManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/AudioManager.cs) 挂载到一个常驻物体

2) 运行游戏  
- 启动入口 [StartGame.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Boot/StartGame.cs) 会初始化 UIManager 与 CreateRoleFlowController，并显示 BeginPanel  
- 点击“开始游戏”进入创建角色流程  
- “设置”可调节音乐/音效开关与音量，立即生效并持久化  
- 在创建角色页面可左右切换职业、查看详情、输入昵称并创建

## UI 框架说明

- 面板基类：[BasePanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Base/BasePanel.cs)  
  - 重写 OnCreate 注册事件与缓存组件  
  - 重写 OnShow/OnHide 实现显示/隐藏逻辑  
  - 提供 ShowMe/HideMe/HideImmediately 控制可见性与过渡  
- UI 管理器：[UIManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/UIManager.cs)  
  - ShowPanel(name) / ShowMainPage(name)：加载 Resources/UI/Windows 下的同名预制并显示  
  - HidePanel(name)、DestroyPanel(name)：隐藏或销毁面板（支持淡出回调）  
  - Popup 栈与遮罩：弹窗面板（Layer=Popup）进入栈，UIMask 跟随栈顶显示，支持遮罩点击关闭  
- 遮罩组件：[UIMask.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Base/UIMask.cs)

示例：显示一个新面板

```csharp
// 通过事件打开
EventBus.Publish(new OpenPanelEvent("SettingPanel"));
// 或直接调用
UIManager.Instance.ShowPanel("SettingPanel");
```

## 事件与数据

- 事件总线：[EventBus.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventBus.cs)  
  - Subscribe<T>(Action<T>) / Unsubscribe<T>(Action<T>) / Publish<T>(T data)  
- 常用 UI 事件定义  
  - [OpenPanelEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/OpenPanelEvent.cs)  
  - [ClosePanelEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/ClosePanelEvent.cs)  
  - [OpenMainPageEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/OpenMainPageEvent.cs)  
  - [OpenRoleInfoPanelEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/OpenRoleInfoPanelEvent.cs)  
  - [CreateRoleRequestEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/CreateRoleRequestEvent.cs)
- 数据持久化  
  - [JsonMgr.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Json/JsonMgr.cs) 负责对象的序列化与反序列化  
  - [SettingData.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Data/SettingData.cs) 保存音量与开关；[DataManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/DataManager.cs) 提供读写接口

## 创建角色流程

1) BeginPanel（主菜单）  
   - 点击“开始游戏”后，调用 [CameraEvent.TurnAround](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/GameEvents/BeginPanelCameraEvent.cs) 播放转场动画，动画回调打开 CreateRolePanel  
2) CreateRolePanel（创建角色）  
   - 左右切换职业、刷新名称与头像（来自 Resources/Portrait）  
   - “职业详情”发布 OpenRoleInfoPanelEvent 打开弹窗  
   - “创建”发布 CreateRoleRequestEvent  
3) RoleInfoPanel（职业详情）  
   - 弹窗层，可点击遮罩关闭  
4) CreateRoleFlowController（流程控制）  
   - 订阅 CreateRoleRequestEvent，调用 [PlayerFactory](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Entity/Factory/PlayerFactory.cs) 生成 [PlayerData](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Entity/Data/PlayerData.cs) 并写入 DataManager  
5) RoleDataManager（配置加载）  
   - 从 [Resources/Config/RoleClassConfig.json](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Resources/Config/RoleClassConfig.json) 加载职业配置

## 资源组织与加载

- 资源路径约定  
  - UI 预制：Resources/UI/Windows/<PanelName>.prefab  
  - 根画布与遮罩：Resources/UI/Root/PanelCanvas.prefab、UIMask.prefab  
  - 配置：Resources/Config  
  - 头像：Resources/Portrait  
- 统一加载入口：[ResourceManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/ResourceManager.cs)

## 常见问题（FAQ）

- 运行后没有任何 UI  
  - 确认 Resources/UI/Root/PanelCanvas.prefab 存在且命名正确；UIManager 会自动实例化  
- 点击“开始游戏”没有转场  
  - 确认主摄像机已挂载 CameraEvent，Animator 有 “Turn” 触发器，且动画尾部触发 PlayerOver  
- 设置面板无效  
  - 确认场景有 AudioManager，SettingPanel 的控件引用已在预制上绑定

## 开发约定

- UI 面板脚本命名与预制名一致（如 SettingPanel.cs ↔ SettingPanel.prefab）  
- 新增弹窗需继承 BasePanel，将 Layer 设为 UILayer.Popup，并按需开启 UseMask/CloseByMask  
- 新功能优先通过事件驱动，避免强耦合引用

## 路线图

- 接入 Continue 存档系统与 About 页面  
- 为 CreateRolePanel/RoleInfoPanel 增加 UI 过渡动画与输入合法性提示  
- 引入 Play Mode 测试覆盖主流程  
- 评估 Addressables/AssetBundle 接入方案并扩展 ResourceManager 加载策略

