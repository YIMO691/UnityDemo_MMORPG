# 最小运行链路清单（Minimal Runtime Checklist, Detailed）

本清单用于保障“BeginScene → 创角/读档 → GameScene → PlayerArmature / Camera / UI”的黄金链路在最瘦身情况下可正常运行；适用于删除资源或调整目录前的安全核对。

## 0. 黄金链路（Gold Path）

BeginScene（启动）  
→ 显示 BeginPanel  
→ 创建角色（或读档）  
→ 保存并注入当前玩家与槽位  
→ 切换 GameScene  
→ 组装 PlayerArmature 与 Camera  
→ 打开 MainPanel（头像可查看职业详情）

---

## 1. 场景（Scenes）

- 必备场景（必须加入 Build Settings）  
  - Assets/Scenes/BeginScene.unity  
  - Assets/Scenes/GameScene.unity

自检：
- [ ] File → Build Settings 列表中可见 BeginScene 与 GameScene

## 2. 程序集与包（Assemblies & Packages）

- 程序集（asmdef，运行期必需）  
  - Assets/Scripts/Framework/MMORPG.Framework.asmdef  
  - Assets/Scripts/Game/MMORPG.Game.asmdef
- 程序集（编辑器，非运行期必需）  
  - Assets/Scripts/Editor/MMORPG.Editor.asmdef
- 包依赖（Package Manager）  
  - Cinemachine（虚拟相机跟随；缺失时走降级）  
  - Input System（PlayerInput 与 inputactions）  
  - TextMeshPro（文本渲染）

自检：
- [ ] Game.asmdef 引用 Cinemachine / Unity.InputSystem / Unity.TextMeshPro  
- [ ] Framework 与 Game 能无错误编译

---

## 3. 资源（Resources）

### 3.1 UI 根节点（Resources/UI/Root）
- PanelCanvas.prefab（Canvas/CanvasScaler/GraphicRaycaster）  
- UIMask.prefab（Popup 栈遮罩，支持点击关闭）

### 3.2 UI 窗口（Resources/UI/Windows）
- Pages：BeginPanel、CreateRolePanel、MainPanel、ContinuePanel  
- Popups：ConfirmPanel、MessageTipPanel、RoleInfoPanel  
- Items：Items/ContinueSlotItem

### 3.3 角色与相机（Resources/Role/PlayerAmature）
- PlayerArmature.prefab：CharacterController、StarterAssets.ThirdPersonController、StarterAssets.StarterAssetsInputs、PlayerInput；包含 PlayerCameraRoot 与 CinemachineCameraTarget  
- MainCamera.prefab：Camera、AudioListener、CinemachineBrain（若安装）  
+- PlayerFollowCamera.prefab：CinemachineVirtualCamera（运行时绑定 Follow/LookAt）

### 3.4 配置（Resources/Config）
- RoleClassConfig.json（职业配置）

---

## 4. 输入（Input System）

- 唯一 inputactions 资产  
  - Assets/Scripts/Game/CharacterControl/Input/StarterAssets.inputactions  
  - GUID：4419d82f33d36e848b3ed5af4c8da37e（仅保留这一份）  
  - 必含 Action Map：Player；Actions：Move、Look、Jump、Sprint
- PlayerInput 绑定  
  - PlayerArmature 的 PlayerInput.m_Actions 指向上述资产（同 GUID）

---

## 5. 脚本（关键运行脚本）

### 5.1 启动/初始化（Game/Boot）
- AppRuntimeInitializer：BeforeSceneLoad 初始化 DataManager、UIManager、RoleDataManager、CreateRoleFlowController、RoleUIController  
- GameBootstrapper：兜底初始化  
- StartGame：显示 BeginPanel

### 5.2 流程/事件
- CreateRoleFlowController：处理创角 → SavePlayerDataToSlot → SetCurrentPlayerData/SetCurrentSlotId → 进入 GameScene  
- RoleUIController：订阅 OpenRoleInfoPanelEvent → 打开 RoleInfoPanel 并填充

### 5.3 场景装配（Game/GameScene）
- GameSceneEntry：TryCreate Camera → TryAssemble Player → TryBind VirtualCamera（失败则 FallbackBind 主相机）→ Show MainPanel  
- PlayerCharacterAssembler：确保 PlayerCameraRoot/CinemachineCameraTarget 存在，重挂职业外观  
- CameraRigAssembler：从 Resources 创建相机或动态创建主相机；配置虚拟相机或降级

### 5.4 数据/配置
- RoleDataManager：加载 RoleClassConfig.json  
- DataManager：设置数据与多存档

---

## 6. UI 路由（Framework/UI）

- UIRouteNames：至少包含 BeginPanel、CreateRolePanel、MainPanel、SettingPanel、ContinuePanel、AboutPanel、RoleInfoPanel、ConfirmPanel、MessageTipPanel  
- UIMainPages：注册 BeginPanel、MainPanel 等主页面
- 输入无效：检查 PlayerInput 的 Actions 绑定

---

## 10. 删除前必看（Do-Not-Remove）
- BeginScene、GameScene  
- PanelCanvas、UIMask、PlayerArmature、MainCamera、PlayerFollowCamera  
- Resources/Config/RoleClassConfig.json  
- Game/CharacterControl/Input/StarterAssets.inputactions（唯一 GUID）  
- MMORPG.Framework、MMORPG.Game  
- AppRuntimeInitializer、GameBootstrapper、StartGame、CreateRoleFlowController、RoleUIController、GameSceneEntry、PlayerCharacterAssembler、CameraRigAssembler、UIManager、RoleDataManager、DataManager

## 7. 最小运行自检清单（删除资源前务必核对）
- 场景
  - [ ] Build Settings 中包含 BeginScene 与 GameScene
- Prefab 与组件
  - [ ] Resources/UI/Root/PanelCanvas 与 UIMask 可加载
  - [ ] PlayerArmature 存在且挂有 CharacterController、ThirdPersonController、StarterAssetsInputs、PlayerInput（Actions 指向 Game 下 inputactions）
  - [ ] MainCamera 与 PlayerFollowCamera 可加载；若缺失，运行时可由 CameraRigAssembler 自动创建主相机
- 配置
  - [ ] Resources/Config/RoleClassConfig.json 存在且可解析（至少一条职业）
- 输入
  - [ ] 仅保留 Game/CharacterControl/Input 下的 StarterAssets.inputactions（消除重复 GUID）
- 程序集
  - [ ] Framework 与 Game 两个 asmdef 存在并编译通过
- UI 路由
  - [ ] UIRouteNames 与 UIMainPages 定义齐全（BeginPanel、CreateRolePanel、MainPanel 等）

## 8. 运行路径（从最小链路验证）
1. 启动加载 BeginScene → UIManager 初始化 → 打开 BeginPanel
2. BeginPanel：
   - “开始游戏” → CreateRolePanel → 填写姓名/选择职业 → 确认
   - “继续游戏” → ContinuePanel（有存档）/MessageTip（无存档）
3. 创角成功：
   - CreateRoleFlowController 保存到槽位 → 注入 DataManager 当前玩家与槽位 → 设置 GameRuntime → 加载 GameScene
4. GameSceneEntry：
   - TryCreate Camera → TryAssemble Player → TryBind VirtualCamera（失败则降级）→ 打开 MainPanel
5. MainPanel：
   - 点击头像 → 通过 RoleDataManager + RoleUIController 打开并填充 RoleInfoPanel

> 说明：本清单用于防止后续清理资源/目录时误删主流程所需的最小依赖，确保“BeginScene → 创角/读档 → GameScene → PlayerArmature / Camera / UI”始终可跑通。*** End Patch*** End Patch
*** End Patch
*** End Patch


## PowerShell：
PS C:\Users\Administrator\Desktop\UnityDemo_MMORPG> Get-ChildItem -Recurse . | Sort-Object Length -Descending | Select-Object FullName, Length -First 100


