# UnityDemo_MMORPG — 客户端 Demo（UI/资源/音频/数据驱动）

**项目定位**：面向单人 RPG 的 Unity 客户端演示工程，已实现启动与主菜单 UI、资源加载抽象、设置数据持久化、基础音频系统与场景资源组织，采用 JSON 配置驱动的可扩展架构。

- 关键词：UI 框架 / 资源系统 / 数据持久化 / 音频管理 / 场景与动画
- 运行平台：Unity（Windows 编辑器优先）

---

## 快速上手
- 打开场景：Assets/Scenes/BeginScene.unity
- 挂载入口脚本：[StartGame.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Boot/StartGame.cs)（场景中已包含）
- 运行后：
  - 自动初始化 UIManager，实例化 PanelCanvas
  - 显示主菜单 [BeginPanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Panels/BeginPanel.cs)
  - 点击“设置”打开 [SettingPanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Panels/SettingPanel.cs)，进行音乐/音效开关与音量调节
- 音频：
  - 在场景中放置一个包含 [AudioManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Data/AudioManager.cs) 的对象（DontDestroyOnLoad）
  - 通过 Inspector 指定默认 BGM（如 Resources/Music/BKMusic.mp3），或运行时调用 PlayBGM/PlaySound

---

## 已实现功能
- 启动与主菜单
  - [StartGame.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Boot/StartGame.cs)：初始化 UIManager 并显示 BeginPanel
  - [BeginPanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Panels/BeginPanel.cs)：开始、继续、设置、关于、退出
- UI 框架
  - [UIManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/UIManager/UIManager.cs)：单例管理、Panel 缓存、显示/隐藏、淡入淡出
  - [BasePanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Base/BasePanel.cs)：CanvasGroup 控制，统一淡入/淡出动画
  - 运行时 UI 资源规范：
    - Canvas：[PanelCanvas.prefab](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Resources/UI/Root/PanelCanvas.prefab)
    - 面板：[BeginPanel.prefab](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Resources/UI/Windows/BeginPanel.prefab)、[SettingPanel.prefab](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Resources/UI/Windows/SettingPanel.prefab)
- 设置与持久化
  - [SettingPanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Panels/SettingPanel.cs)：音乐/音效开关与音量滑条（数据与 UI 双向联动）
  - [DataManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Data/DataManager.cs)：统一读写设置数据
  - [SettingData.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Data/SettingData.cs)：配置模型（musicOn/soundOn/volume/lastVolume）
  - [JsonMgr.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Json/JsonMgr.cs)：LitJSON / JsonUtility 序列化与本地文件持久化
- 资源系统
  - [ResourceManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/ResourceSystem/ResourceManager.cs)：统一加载接口与模式切换（编辑器默认 Resources）
  - [ResourcesLoader.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/ResourceSystem/ResourcesLoader.cs)：Resources 加载实现
  - [AssetBundleLoader.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/ResourceSystem/AssetBundleLoader.cs)：AB 预留（待实现）
- 音频系统
  - [AudioManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Data/AudioManager.cs)：BGM/SFX 双 AudioSource，音量/开关应用、PlayBGM/PlaySound
  - 运行时资源：Resources/Music 目录包含示例音频（BKMusic.mp3、Wound.wav 等）
- 场景与动画
  - BeginScene：启动展示与动效资源
  - AnimatorController：BeginSceneFlammerIdle、BeginSceneGunnerIdle、BeginSceneZombieRun 等

---

## 目录结构（核心）
```
Assets
├─ Resources
│  ├─ UI
│  │  ├─ Root/PanelCanvas.prefab
│  │  └─ Windows/BeginPanel.prefab, SettingPanel.prefab
│  ├─ Music/  # BKMusic.mp3, Wound.wav 等
│  ├─ AnimatorController/
│  └─ Animation/
├─ Scenes/BeginScene.unity
├─ Scripts
│  ├─ Boot/StartGame.cs
│  ├─ Framework
│  │  ├─ UI
│  │  │  ├─ Base/BasePanel.cs
│  │  │  ├─ Panels/BeginPanel.cs, SettingPanel.cs
│  │  │  └─ UIManager/UIManager.cs
│  │  ├─ ResourceSystem/ResourceManager.cs, ResourcesLoader.cs, AssetBundleLoader.cs
│  │  ├─ Json/JsonMgr.cs (+ LitJSON)
│  │  └─ Data/AudioManager.cs, DataManager.cs, SettingData.cs
│  └─ Game/  # 业务模块占位（AI/Combat/Entity/Inventory/Skill 等）
└─ ArtRes/   # 第三方美术与效果资源
```

---

## 开发规范与约定
- UI 面板路径：Resources/UI/Windows/{PanelName}，脚本类名与预制体名一致
- Canvas 根节点：Resources/UI/Root/PanelCanvas
- 资源访问：使用 ResourceManager.Instance.Load<T>(path)
- 设置持久化：JsonMgr 保存至 persistentDataPath；默认数据可放置于 StreamingAssets
- 提交规范：建议使用 Conventional Commits（feat/fix/refactor/chore/docs）
- 大体积资源：建议启用 Git LFS（fbx/psd/wav/mp4/tga 等）

---

## 路线图（Roadmap）
- 近期
  - 完成 AssetBundleLoader 基础能力（构建/版本/缓存）
  - 规范 UI 命名与异常处理（空引用保护/路径校验）
- 明日计划（数据建模）
  - 以“创建角色”为入口，结构化人物/怪物/NPC 等数据
    - 抽象 Entity 基础数据（ID、名称、阵营、等级、属性、移动/战斗参数）
    - 定义 Player/Monster/NPC 数据结构与差异字段（职业/外观/掉落/对话等）
    - 设计配置数据格式与加载流程（Resources/Config → 反序列化 → 运行时缓存）
    - 预留存档结构（角色创建结果持久化，后续接入继续游戏）

---

## 参考与入口
- UIManager 初始化与调用：参见 [UIManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/UIManager/UIManager.cs)
- 主菜单与设置面板：参见 [BeginPanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Panels/BeginPanel.cs)、[SettingPanel.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/UI/Panels/SettingPanel.cs)
- 资源加载：参见 [ResourceManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/ResourceSystem/ResourceManager.cs)
- 音频播放：参见 [AudioManager.cs](file:///c:/Users/Administrator/Desktop/UnityDemo_MMORPG/Assets/Scripts/Framework/Data/AudioManager.cs)
