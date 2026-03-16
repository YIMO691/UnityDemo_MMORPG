# 框架架构与操作指南

## 模块关系总览

- UIManager
  - 职责：面板实例化与显示/隐藏、层级挂载、主页面切换、Popup 栈与遮罩、事件驱动打开/关闭。
  - 入口方法：ShowPanel(name)、ShowMainPage(name, hideOld, useFade)、HidePanel(name)、DestroyPanel(name)、GetPanel<T>()、IsPanelVisible(name)、ShowConfirm(message, onConfirm, onCancel)。
  - 事件订阅：OpenPanelEvent、ClosePanelEvent、OpenMainPageEvent、OpenRoleInfoPanelEvent。
  - 参考：[UIManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/UIManager.cs)

- EventBus（事件总线）
  - 职责：跨模块通信、解耦 UI 与业务逻辑。
  - 方法：Subscribe<T>(Action<T>)、Unsubscribe<T>(Action<T>)、Publish<T>(T data)、Clear()。
  - 参考：[EventBus.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventBus.cs)

- BasePanel（UI 基类）
  - 职责：统一面板生命周期与淡入淡出时序；提供 OnCreate/OnShow/OnHide/OnDestroyPanel 等扩展点。
  - 属性：Layer（Bottom/Normal/Popup/Top）、UseMask、CloseByMask。
  - 参考：[BasePanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Base/BasePanel.cs)

- BeginPanel（主菜单）
  - 职责：主菜单入口，触发创建角色流程、设置、继续游戏、关于、退出等行为。
  - 关键点：
    - “开始游戏” → 播放 CameraEvent 转场 → 打开 CreateRolePanel
    - “继续游戏” → 有存档则打开 ContinuePanel；无存档弹 MessageTipPanel
    - “设置/关于/退出” → 通过事件打开对应面板或调用确认弹窗
  - 参考：[BeginPanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Panels/BeginPanel.cs)

- DataManager（数据管理）
  - 职责：设置数据持久化、当前玩家数据缓存、多存档槽位管理。
  - 多存档核心 API：GetFirstEmptySlotId、GetMaxUsedSlotId、GetNextAvailableSlotId、SavePlayerDataToSlot、LoadPlayerDataFromSlot、DeletePlayerDataInSlot、HasPlayerSaveInSlot、HasAnyPlayerSave、GetPlayerSaveMetaData、GetAllPlayerSaveMetaData。
  - 参考：[DataManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/DataManager.cs)

- CreateRoleFlowController（创角流程控制）
  - 职责：处理 CreateRoleRequestEvent；调用 PlayerFactory 生成玩家数据；保存到槽位；切换到 MainPanel。
  - 参考：[CreateRoleFlowController.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Flow/CreateRoleFlowController.cs)

- PlayerFactory（玩家数据工厂）
  - 职责：根据职业配置构造 PlayerData（拆分为 base/progress/attribute）。
  - 参考：[PlayerFactory.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Entity/Factory/PlayerFactory.cs)

- RoleDataManager（职业配置）
  - 职责：载入角色职业配置（Resources/Config/RoleClassConfig.json）；提供查询接口。
  - 参考：[RoleDataManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Entity/RoleClass/Manager/RoleDataManager.cs)

- 事件定义（UI）
  - 打开/关闭面板：[OpenPanelEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/OpenPanelEvent.cs)、[ClosePanelEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/ClosePanelEvent.cs)
  - 主页面切换：[OpenMainPageEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/OpenMainPageEvent.cs)
  - 角色详情弹窗：[OpenRoleInfoPanelEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/OpenRoleInfoPanelEvent.cs)
  - 创角请求：[CreateRoleRequestEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/CreateRoleRequestEvent.cs)

## 数据与交互流转

- 启动流
  - StartGame.Start → UIManager.Init → 订阅事件、实例化 PanelCanvas/UIMask → ShowMainPage("BeginPanel")
  - 参考：[StartGame.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Boot/StartGame.cs)

- 创建角色流
  - BeginPanel.OnClickStart → CameraEvent.TurnAround → Publish(OpenMainPageEvent("CreateRolePanel"))
  - CreateRolePanel 选择职业、输入昵称 → Publish(CreateRoleRequestEvent)
  - CreateRoleFlowController 订阅事件 → PlayerFactory.Create → DataManager.SetCurrentPlayerData → SaveCurrentPlayerDataToSlot(GetNextAvailableSlotId) → Publish(OpenMainPageEvent("MainPanel"))
  - MainPanel 读取 DataManager.GetCurrentPlayerData 刷新 UI

- 继续游戏流
  - BeginPanel.OnClickContinue → HasAnyPlayerSave ? 否 → Show(MessageTipPanel)；是 → Publish(OpenPanelEvent("ContinuePanel"))
  - ContinuePanel 列出摘要 → LoadPlayerDataFromSlot(slotId) → Publish(OpenMainPageEvent("MainPanel"))

- 设置与音频
  - SettingPanel 控件变化 → DataManager.SetXXX → AudioManager.SetXXX → SaveSettingData；关闭时 Publish(ClosePanelEvent("SettingPanel"))

- 确认弹窗
  - 使用 UIManager.ShowConfirm(message, onConfirm, onCancel)；BeginPanel/SettingPanel 在“退出/返回标题”等敏感操作前调用

## 面板生命周期与层级规范

- BasePanel 生命周期
  - OnCreate：仅一次，用于注册事件/缓存组件
  - OnShow/OnHide：每次显示/隐藏时调用，用于刷新/停止监听等
  - OnDestroyPanel：销毁前清理监听，避免泄漏

- 层级与遮罩
  - Layer=Normal：主页面（BeginPanel、CreateRolePanel、MainPanel）
  - Layer=Popup：弹窗（Setting、RoleInfo、Continue、MessageTip、Confirm、About）
  - UseMask/CloseByMask：遮罩显示与点击关闭配置；UIManager 通过 Popup 栈管理遮罩与顺序

## 新增功能操作指南

- 新增主页面（Normal）
  - 预制：Resources/UI/Windows/NewMainPanel.prefab；脚本 NewMainPanel.cs（继承 BasePanel，Layer=Normal）
  - 打开：Publish(OpenMainPageEvent("NewMainPanel", hideOld: true, useFade: false)) 或 UIManager.ShowMainPage("NewMainPanel")

- 新增弹窗（Popup）
  - 预制：Resources/UI/Windows/NewPopup.prefab；脚本 NewPopupPanel.cs（Layer=Popup；按需 UseMask/CloseByMask）
  - 打开/关闭：Publish(OpenPanelEvent("NewPopup")) / Publish(ClosePanelEvent("NewPopup")

- 新增跨模块动作（事件驱动）
  - 定义事件结构于 EventDefine；发布 EventBus.Publish；在合适模块 Subscribe 并在清理时 Unsubscribe

- 多存档接入
  - 写入：DataManager.SaveCurrentPlayerDataToSlot(slotId)
  - 读取：DataManager.LoadPlayerDataFromSlot(slotId)
  - 摘要：DataManager.GetAllPlayerSaveMetaData(maxCount)
  - 槽位选择：DataManager.GetNextAvailableSlotId()

- 统一确认流程
  - UIManager.ShowConfirm("内容", onConfirm, onCancel)

## 问题定位建议

- 面板不显示：确认 Resources/UI/Windows 下同名预制；UIManager.Init 已加载 PanelCanvas；检查 Layer 与脚本继承。
- 事件无响应：检查 Subscribe；销毁时是否误 Unsubscribe；事件结构路径一致性。
- 遮罩异常：确认 Layer=Popup 且 UseMask；UIMask 预制存在并正确加载。
- 存档读取失败：确认 JsonMgr.HasData(fileName)；PlayerData.baseData/progressData 非空；文件名规则 player_<slotId>。

## 快速索引

- [UIManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/UIManager.cs)
- [EventBus.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventBus.cs)
- [BasePanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Base/BasePanel.cs)
- [BeginPanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Panels/BeginPanel.cs)
- [CreateRoleFlowController.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Flow/CreateRoleFlowController.cs)
- [PlayerFactory.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Entity/Factory/PlayerFactory.cs)
- [RoleDataManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Game/Entity/RoleClass/Manager/RoleDataManager.cs)
- [DataManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Managers/DataManager.cs)
- 事件定义（UI）：[OpenPanelEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/OpenPanelEvent.cs)、[ClosePanelEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/ClosePanelEvent.cs)、[OpenMainPageEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/OpenMainPageEvent.cs)、[OpenRoleInfoPanelEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/OpenRoleInfoPanelEvent.cs)、[CreateRoleRequestEvent](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Event/EventDefine/UIEvents/CreateRoleRequestEvent.cs)

