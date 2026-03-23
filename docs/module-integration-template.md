# 新增模块接入模板（制度化开发清单）

本模板用于在新增任何系统前，先完成工程化设计与接入决策，确保结构稳定、依赖清晰、上线可控。建议在每个新模块创建一份副本，按章节逐项填写与勾选。

---

## 0. 模块总览（What & Why）
- 名称：
- 简介：
- 价值/目标：
- 非目标（明确不包含的范围）：

---

## 1. 分层定位（Framework or Game）
- 所属层：Framework / Game
- 选择理由：
  - 若为 Framework：是否通用、是否与业务脱钩、是否可跨项目复用
  - 若为 Game：是否含业务规则/资源耦合/玩法相关

决策要求：Framework 不得引用 Game 类型；跨层能力通过事件/接口/路由解耦。

---

## 2. 运行态设计（Runtime）
- 形态：Service / MonoBehaviour / ScriptableObject / 其他
- 生命周期：初始化时机（BeforeSceneLoad / Start / 懒加载），销毁/清理策略
- 单例与线程：是否单例、是否涉及异步/线程
- 关键依赖：UIManager / DataManager / RoleDataManager / EventBus / Cinemachine / Input System / TextMeshPro / NavMesh（如需）

---

## 3. 持久化策略（Persistence）
- 归属：Framework（设置/通用）或 Game（业务数据）
- 数据结构/版本：JSON Schema、版本号、向后兼容与迁移方案
- 存储位置与命名：persistentDataPath 文件名规则（如 player_<slotId>）
- 入口：JsonMgr / Addressables/Resources（只读配置）
- 读写接口：新增或复用 GamePlayerDataService/DataManager

---

## 4. UI 接入（Pages / Popups / Items）
- 类型：主页面（Pages）/ 弹窗（Popups）/ 元素（Items）
- 路由：是否新增到 UIRouteNames / UIMainPages
- 层级：UILayer（Bottom/Normal/Popup/Top），是否需要遮罩与遮罩点击关闭
- 预制资源：Resources/UI/Windows/<PanelName>.prefab（与脚本同名）
- 动画/过渡：是否需要淡入淡出或自定义动画

---

## 5. 资源路径与常量（AssetPaths/UIPaths）
- 是否需要新增 AssetPaths / UIPaths 常量
- 资源归档到 Resources 规则是否满足（或后续 Addressables）
  - 地图类：UIPaths.MapImageRoot、PortraitRoleHeadRoot 等
 - 避免硬编码：
   - 资源路径统一使用 AssetPaths（如 DebugCanvas、PoolMonitorPanel、MapImageRoot）
   - 面板路由/面板名统一使用 UIPaths/UIRouteNames/UIMainPages
   - 对象名集中到 ObjectNames（如 MiniMapCamera、PlayerCameraRoot）
   - 导航 AgentId 集中到 NavigationConsts（如 PlayerAgentId）
   - 在实现前，若需要新路径或常量，先补常量再编码；不得直接写魔法字符串

---

## 6. 事件设计（EventBus）
- 事件列表：
  - 事件名：
  - 负载结构：
  - 发布位置：
  - 订阅者：
- 事件放置层：
  - 通用事件 → Framework/EventDefine
  - 业务事件 → Game/Events
- 解耦说明：不直接跨层引用类型，使用事件/路由/数据契约
  - 导航类推荐：NavigationMoveRequestEvent / NavigationStopRequestEvent（负载 NavigationMoveRequest）

---

## 7. 输入 & 相机（如适用）
- Input：是否复用 Player Action Map；是否需要新增 Action/Binding；GUID 统一性
- Camera：是否需要跟随/锁定；是否依赖 Cinemachine vcam；是否兼容降级（FallbackBind）
 - 光标：是否需要在焦点切换时解锁显示确保 UI 可点击

---

## 8. 数据模型与目录（Domain Model & Layout）
- 数据模型位置：
  - Game/Entity/<Domain>/Data/{Base/Progress/Runtime}
  - Save/Mapper（如需摘要或持久化映射）
- 命名规范：PlayerXxxData、XxxConfig、XxxRequest
- 序列化约束：字段类型、默认值、兼容策略
 - 地图：MapConfig（sceneName/displayName/mapImage/worldMinX/worldMaxX/worldMinZ/worldMaxZ）

---

## 9. 程序集与依赖（asmdef）
- 所属程序集：MMORPG.Game 或 MMORPG.Framework
- 依赖新增：Cinemachine / Unity.InputSystem / Unity.TextMeshPro（仅 Game 需要时加入）
- 循环依赖检查：Framework 不得引用 Game

---

## 10. 目录模板（建议）
```
Assets/Scripts/Game/<Feature>/
  ├─ Runtime/               # 运行态服务、控制器
  ├─ UI/
  │  ├─ Pages/
  │  ├─ Popups/
  │  └─ Items/
  ├─ Entity/
  │  └─ Data/               # Base/Progress/Runtime
  ├─ Save/
  │  └─ Mapper/             # DTO/摘要映射（可选）
  ├─ Events/                # 业务事件（可选）
  └─ Editor/                # 工具（可选）
```

---

## 11. 接入步骤（Checklist）
- [ ] 分层定位完成（Framework / Game，含理由）
- [ ] 运行态初始化点明确（AppRuntimeInitializer / 懒加载 / 其他）
- [ ] 持久化方案与版本管理确认（JsonMgr / 文件名规则 / 迁移）
- [ ] UI 路由与层级完成（UIRouteNames / UIMainPages / 预制路径）
- [ ] 事件契约与发布/订阅登记（EventBus）
- [ ] 资源路径常量更新（AssetPaths / UIPaths）
- [ ] 输入与相机需求评估（InputActions / Cinemachine）
- [ ] 数据模型落位到 Game/Entity/<Domain>/Data
- [ ] asmdef 依赖更新与循环依赖检查
- [ ] 最小运行链路回归通过（见 minimal-runtime-checklist）
 - [ ] 导航类：Bake NavMesh；NavigationService.Init；Registry 注册代理；UI 点击发布 NavigationMoveRequest

---

## 12. 验收标准（Acceptance）
- [ ] BeginScene → 创角/读档 → GameScene 全链路无报错
- [ ] 新页面/弹窗可按路由打开/关闭，遮罩与返回逻辑正确
- [ ] 数据保存/读取成功、摘要列表展示正确（若适用）
- [ ] 控制台无跨层引用警告/编译错误
- [ ] 文档补齐（本模板 + README/architecture 如需）

---

## 13. 示例（以“背包系统”为例）
- 分层：Game（玩法/物品/容量/排序均属业务）
- 运行态：InventoryService（单例 Service），在 AppRuntimeInitializer 懒加载
- 持久化：GamePlayerDataService 扩展槽位文件，或单独 inventory_<slotId>
- UI：
  - Pages：InventoryPanel（主页面）
  - Popups：ItemInfoPanel（弹窗）
  - 路由：UIRouteNames.InventoryPanel，UIMainPages 注册为可打开主页面
- 资源：Resources/UI/Windows/{InventoryPanel,ItemInfoPanel}；图标路径常量新增到 UIPaths
- 事件：OpenInventoryEvent、OpenItemInfoEvent（置于 Game/Events）
- 数据模型：
  - Game/Entity/Inventory/Data/{InventoryData, ItemData}
  - Save/Mapper 将 InventoryData → InventorySummary（列表摘要）
- asmdef：仅 MMORPG.Game；引用 TextMeshPro（如展示富文本）
- 验收：
  - 从 MainPanel 打开 InventoryPanel → 列表渲染 → 点击物品 → 打开 ItemInfoPanel
  - 存档后重启仍能正确还原

---

制定与使用本模板后，新增系统将先经过固定的工程化检查，确保分层清晰、依赖可控与运行链路稳定，再进入实现阶段。*** End Patch
