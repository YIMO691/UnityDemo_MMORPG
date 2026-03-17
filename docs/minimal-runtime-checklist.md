# 最小运行链路清单（Minimal Runtime Checklist）

## 最小启动场景
- 场景：Assets/Scenes/BeginScene.unity
- 主链路：BeginScene → 创角/读档 → GameScene → PlayerArmature / Camera / UI

## 必备场景（需在 Build Settings 中可加载）
- Assets/Scenes/BeginScene.unity
- Assets/Scenes/GameScene.unity

## 必备 Prefab（Resources 路径）
- UI 根节点
  - Resources/UI/Root/PanelCanvas
  - Resources/UI/Root/UIMask
- 角色与相机
  - Resources/Role/PlayerAmature/PlayerArmature
  - Resources/Role/PlayerAmature/MainCamera
  - Resources/Role/PlayerAmature/PlayerFollowCamera

## 必备 UI 窗口 Prefab（Resources/UI/Windows）
- 主页面与流程页（至少）
  - BeginPanel
  - CreateRolePanel
  - MainPanel
- 弹窗与通用
  - ConfirmPanel
  - MessageTipPanel
  - RoleInfoPanel（头像详情用）
  - ContinuePanel（读档用）
  - Items/ContinueSlotItem（ContinuePanel 列表项）

## 必备配置/JSON（Resources 路径）
- Resources/Config/RoleClassConfig.json（职业配置，用于创角/职业详情）

## 必备输入配置
- StarterAssets.inputactions（GUID：4419d82f33d36e848b3ed5af4c8da37e）
  - 路径：Assets/Scripts/Game/CharacterControl/Input/StarterAssets.inputactions
  - 要求：PlayerArmature 上的 PlayerInput 组件 m_Actions 指向上述资产（同 GUID）

## 必备脚本程序集（asmdef）
- 运行期必需
  - Assets/Scripts/Framework/MMORPG.Framework.asmdef
  - Assets/Scripts/Game/MMORPG.Game.asmdef
- 编译期/编辑器（非运行期必需）
  - Assets/Scripts/Editor/MMORPG.Editor.asmdef

## 关键运行脚本（最小链路）
- 启动/初始化（Game/Boot）
  - AppRuntimeInitializer（BeforeSceneLoad 初始化 DataManager、UIManager、RoleDataManager、CreateRoleFlowController、RoleUIController）
  - GameBootstrapper（兜底初始化）
  - StartGame（进入 BeginPanel）
- 流程/事件
  - CreateRoleFlowController（创角、存档、注入当前玩家与槽位、切换 GameScene）
  - RoleUIController（监听 OpenRoleInfoPanelEvent 并填充 RoleInfo 面板）
- 角色/相机装配（Game/GameScene）
  - GameSceneEntry（场景入口：创建/装配角色与相机、打开 MainPanel）
  - PlayerCharacterAssembler（实例化 PlayerArmature、挂载职业外观、初始化相机目标）
  - CameraRigAssembler（创建主相机/随从相机，或降级回退到主相机跟随）
- 数据/配置
  - RoleDataManager（读取 RoleClassConfig.json）
  - DataManager（设置与多存档读写/当前槽位/当前玩家数据）
- UI 管理（Framework）
  - UIManager（画布/层级/弹窗遮罩/路由与主页面打开）
  - UIDialogService（以路由+反射方式调用 ConfirmPanel.SetData）

## 第三方/包依赖（运行所需）
- Cinemachine（虚拟相机跟随；无则自动降级为主相机跟随）
- Unity Input System（PlayerInput 与 inputactions）
- TextMeshPro（文本渲染）

## 最小运行自检清单（删除资源前务必核对）
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

## 运行路径（从最小链路验证）
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

FullName
--------
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\libburst-llvm-1...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\ArtifactDB
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\libburst-llvm-1...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\e5\e5e168453443a6e76ddc10b2280adab6
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\libburst-llvm-1...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\libburst-llvm-1...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\libburst-llvm-1...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\hostmac\dsymutil
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\c7\c76d22a733ecb990cd9de5a291c9bb5d
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Scenes\Scene_Demo\Scene_Demo_Terrain.asset
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-19.dll
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-18.dll
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-19-a...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Trees_Branches.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-18-a...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Tombs1.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Tombs1_nmp.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Trees.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Graveyard_atlas1.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Graveyard_atlas1_n...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Trees_nmp.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Mausoleum_Atlas_nm...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Mausoleum_Atlas.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\GabrielAguiarProductions\Unique_Projectiles_Volume_1\H...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\GabrielAguiarProductions\Unique_Projectiles_Volume_1\L...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Settings\URP\URP_7.3.1.unitypackage
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Demo\Demo1.unity
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Scenes\Scene_Demo\Scene_Demo.unity
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\hostlin\dsymutil
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-B4B9...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-B4B9...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-43FF...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\SourceAssetDB
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-B4B9...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-43FF...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\hostwin\dsymuti...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\burst-llvm-43FF...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff02_m.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff02_n.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff04_m.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff03_m.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\hostwin-arm64\d...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff05_m.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff03_n.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff01_m.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff05_n.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff04_n.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff01_n.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\hostmac\llvm-lipo
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\6c\6c3d723e38553317af07af5bb0b27d36
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Statues_nmp.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\29\299db4233c430a30cd87474f9b56b985
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\a5\a59de56a2851901ca8fd077138b9012f
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\0f\0fe090a5493840e64316ffc3f93a8f69
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\41\415da74b96643b9903ed1d3d23582cc2
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\db\dbfe5962138c80652b767b02cb04527e
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\dd\dd2faf88df8563201a76e8ca7eb0a6a6
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\41\4151e8118afa465b81a222d6e786e6e9
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\fd\fd2fb28285912e6b677eb019a6b9b8bb
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\ec\eced9895079e1f397e8a21f1734466ad
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\49\492845f0823064c72c4e681e4dc9306f
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\32\3280baefa7889788b8c6f4f214f2d70c
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Settings\Built-in\Built-in.unitypackage
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Tombs2_nmp.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Settings\URP\URP_10.3.2.unitypackage
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\StarterAssets\ThirdPersonController\Character\Textures\Armatu...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\StarterAssets\ThirdPersonController\Character\Textures\Armatu...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Bee\1900b0aEDbg.dag
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Rocks_AO.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\StarterAssets\ThirdPersonController\Character\Textures\Armatu...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Artifacts\6b\6b5c4bfc7f19ca9048d8567c580628ed
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Rocks_m.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Tombs1_m.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Prefabs_props\Textures\Rocks.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Prefabs_props\Textures\Stone_elements.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Prefabs_props\Textures\Tree_leaves_winte...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Graveyard_atlas1_m...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Statues_m.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Prefabs_props\Textures\Tree_leaves.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Trees_m.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Tombs2_m.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Ornaments1_nmp.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Ornaments1.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Prefabs_props\Textures\Ground_decals_win...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Grounds\Ground_05_winter.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Prefabs_props\Textures\Rocks_winter.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Prefabs_props\Textures\Stone_elements_Wi...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Grounds\Ground_01_winter.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Prefabs_props\Textures\Ground_decals.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Tower Defense\Grounds\Ground_05.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\_Origin\Textures\Mausoleum_Atlas_m.tif
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff02_a.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff03_a.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff04_a.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff05_a.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\Bee\1900b0aE.dag
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\PureNature\Models\Rocks\Cliffs\Textures\Cliff01_a.png
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\hostlin\llvm-lipo
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Library\PackageCache\com.unity.burst@1.8.21\.Runtime\hostwin-arm64\l...
C:\Users\Administrator\Desktop\UnityDemo_MMORPG\Assets\ArtRes\Top-Down Graveyard\Demo1\Demo1.unity

