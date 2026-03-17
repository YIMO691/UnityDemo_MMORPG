# 架构概览（Architecture Overview）

## 分层与程序集

- Framework（基础设施）
  - 目标：跨项目可复用的通用能力
  - 内容：UI 框架（BasePanel/UIManager/UIMainPages/UIRouteNames）、事件总线（EventBus）、资源加载（ResourceManager）、数据与设置（DataManager/JsonMgr）、常量
  - 程序集：MMORPG.Framework（引用尽可能精简）
- Game（业务域）
  - 目标：具体游戏逻辑与资产
  - 内容：角色控制（CharacterControl/Input+ThirdPerson）、场景装配（GameScene/Entry+Assemblers）、实体与配置（Entity/RoleClass）、流程控制（Flow）、UI（Controllers/Panels）
  - 程序集：MMORPG.Game（引用 Cinemachine/Input System/TextMeshPro）
- Editor（编辑器扩展）
  - 目标：工具与工作流提升
  - 内容：配置编辑器、场景菜单等
  - 程序集：MMORPG.Editor（仅在编辑器编译）

边界约束：Framework 不直接依赖 Game 类型。通用对话（ConfirmPanel）位于 Framework，调用通过路由名+反射以消除跨层耦合。

## 最小运行链路

BeginScene → 创角/读档 → GameScene → PlayerArmature / Camera / UI

- 启动与初始化
  - Game/Boot 下 AppRuntimeInitializer、GameBootstrapper、StartGame
  - 初始化 DataManager、UIManager、RoleDataManager、CreateRoleFlowController、RoleUIController
- 创角流程
  - CreateRolePanel 发布 CreateRoleRequestEvent
  - CreateRoleFlowController 生成 PlayerData → 保存到槽位 → 注入当前玩家与槽位 → 进入 GameScene
- 场景装配
  - GameSceneEntry 调用 Assemblers：PlayerCharacterAssembler（实例化骨架/外观/相机目标）、CameraRigAssembler（创建/绑定相机，失败降级）
  - 打开 MainPanel，点击头像通过 RoleUIController 打开 RoleInfoPanel

最小运行依赖清单见：[docs/minimal-runtime-checklist.md](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/docs/minimal-runtime-checklist.md)

## 模块说明

- UI 框架（Framework/UI）
  - BasePanel：生命周期与过渡
  - UIManager：画布/分层/弹窗栈/路由与主页面
  - UIDialogService：以路由名显示 ConfirmPanel 并通过反射调用 SetData
  - UIMainPages / UIRouteNames：主页面注册与路由常量
- 数据与配置
  - DataManager：设置数据与多存档（currentSlotId/currentPlayerData 兼容过渡）
  - RoleDataManager：从 Resources/Config/RoleClassConfig.json 读取职业配置
  - PlayerSaveMetaMapper（Game/Save/Mapper）：PlayerData → PlayerSaveMetaData
- 角色控制（Game/CharacterControl）
  - Input：StarterAssets.inputactions（唯一 GUID）、StarterAssetsInputs
  - ThirdPerson：ThirdPersonController、BasicRigidBodyPush
- 场景（Game/GameScene）
  - Entry：GameSceneEntry（组装与打开主界面）
  - Assemblers：PlayerCharacterAssembler、CameraRigAssembler（降级策略）
- 流程（Game/Flow）
  - CreateRoleFlowController：订阅创角事件，写入存档与当前玩家/槽位，切换 GameScene
  - RoleUIController：订阅 OpenRoleInfoPanelEvent 并填充面板

## 事件流

- OpenPanelEvent / ClosePanelEvent / OpenMainPageEvent（UI 路由）
- CreateRoleRequestEvent（创角）
- OpenRoleInfoPanelEvent（职业详情）

事件通过 Framework/EventBus 解耦模块间依赖，禁止业务层直接持有跨域引用。

## 包与外部依赖

- Cinemachine：虚拟相机跟随（缺失时自动降级为主相机跟随）
- Unity Input System：PlayerInput 与 inputactions
- TextMeshPro：文本渲染

## 开发约定

- 面板脚本与预制同名，位于 Resources/UI/Windows/<PanelName>.prefab
- 业务逻辑只能放在 Game 程序集；基础设施能力放在 Framework
- 新增通用能力优先抽至 Framework；与具体玩法耦合的能力留在 Game
- 资源引用优先使用 Resources 路径常量（AssetPaths/UIPaths）

## 后续演进

- 迁移 Player* 数据模型到 Game，稳定 DataManager 数据流接口
- 清理 StarterAssets 未使用资源并产出清单
- 覆盖主链路的 Play Mode/编辑器测试
