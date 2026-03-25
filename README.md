# UnityDemo_MMORPG

基于 Unity 2022.3.62f3 的 MMORPG 客户端原型。

**目标**：最小可运行主链路 + 明确职责边界 + 数据/资源常量化 + 可持续扩展

---

## 目录

- [快速开始](#快速开始)
- [总体流程](#总体流程)
- [目录结构](#目录结构)
- [核心系统](#核心系统)
  - [启动与场景装配](#启动与场景装配)
  - [UI 系统](#ui-系统)
  - [导航与地图](#导航与地图)
  - [怪物系统](#怪物系统)
  - [存档与数据](#存档与数据)
- [常量与资源规范](#常量与资源规范)
- [代码约定](#代码约定)
- [故障排查](#故障排查)
- [工程化改进计划](#工程化改进计划)
- [参考文档](#参考文档)

---

## 快速开始

### 环境要求
- Unity 2022.3.62f3
- 必要包：Input System、Cinemachine、TextMeshPro

### 运行步骤
1. 打开 Unity 项目
2. Build Settings：添加 `BeginScene.unity` 与 `GameScene.unity`
3. 输入资产：PlayerInput 的 Actions 指向 `StarterAssets.inputactions`
4. 运行 BeginScene：开始游戏（创角→自动存档→进入游戏）或继续游戏（选择存档）

---

## 总体流程

```
BeginScene → 创角/读档 → GameScene → 角色/相机/UI 装配 → 导航/地图/怪物模块运行
```

### 架构分层

| 层级 | 职责 | 组成 |
|------|------|------|
| **Framework** | 基础设施，不含业务规则 | UI框架、事件总线、资源加载、数据管理、常量 |
| **Game** | 业务逻辑与流程控制 | CharacterControl、GameScene、Player、Monster、Navigation、Map、Flow、UI |
| **Resources** | 运行时资源 | UI、配置、怪物Prefab、动画/AnimatorController |

---

## 目录结构

```
Assets/
├─ Scenes/
│  ├─ BeginScene.unity          # 开始场景（主菜单/创角/读档）
│  └─ GameScene.unity           # 游戏场景
├─ Resources/
│  ├─ UI/
│  │  ├─ Root/                  # DebugCanvas 等
│  │  └─ Windows/               # 页面/弹窗预制体
│  ├─ Map/                      # 地图图片资源
│  ├─ Monster/                  # 怪物 Prefab（Zombie/Z1~Z4）
│  ├─ AnimatorController/       # 动画控制器
│  └─ Config/                   # JSON 配置文件
│     ├─ RoleClassConfig.json
│     ├─ MapConfig.json
│     └─ MonsterConfig.json
└─ Scripts/
   ├─ Framework/                # 基础设施层
   │  ├─ UI/                    # UI框架
   │  ├─ Event/                 # 事件总线
   │  ├─ Managers/              # 管理器
   │  ├─ Json/                  # JSON处理
   │  └─ Consts/                # 常量定义
   └─ Game/                     # 业务层
      ├─ Boot/                  # 启动入口
      ├─ CharacterControl/      # 角色控制与输入
      ├─ GameScene/             # 场景装配
      ├─ Navigation/            # 导航系统
      ├─ Player/                # 玩家系统
      ├─ Monster/               # 怪物系统
      ├─ Map/                   # 地图系统
      ├─ Combat/                # 战斗系统
      ├─ Battle/                # 战斗运行时
      ├─ Flow/                  # 流程控制
      └─ UI/                    # UI面板
```

---

## 核心系统

### 启动与场景装配

**应用级管理器**：[GameManager.cs](Assets/Scripts/Game/Boot/GameManager.cs)
- 单例 + DontDestroyOnLoad
- Initialize：核心管理器 → 怪物配置 → 进入 Begin 流程
- ReturnToBeginFlow：清理小地图等 → 跳转 BeginScene

**Game 场景入口**：[GameSceneEntry.cs](Assets/Scripts/Game/GameScene/Entry/GameSceneEntry.cs)
- 两阶段：`TryAssembleScene(...)` → `CommitScene(...)`
- Assemble：PlayerRuntimeService.CreateRuntimePlayer（组装→Init→ApplySnapshot→SetAgentId→Register）
- Commit：打开主页面、写入场景上下文、小地图绑定、MonsterModule.InitForScene

**常量定义**：
- 资源路径：[AssetPaths.cs](Assets/Scripts/Framework/Consts/AssetPaths.cs)
- 对象名：[ObjectNames.cs](Assets/Scripts/Framework/Consts/ObjectNames.cs)
- 导航常量：[NavigationConsts.cs](Assets/Scripts/Framework/Consts/NavigationConsts.cs)
- UI名称：[UINames.cs](Assets/Scripts/Framework/UI/UINames.cs)
- UI文案：[UIStrings.cs](Assets/Scripts/Framework/Consts/UIStrings.cs)

---

### UI 系统

| 组件 | 文件 | 职责 |
|------|------|------|
| 基类 | [BasePanel.cs](Assets/Scripts/Framework/UI/Base/BasePanel.cs) | 生命周期管理（OnCreate/OnShow/OnHide/OnDestroyPanel） |
| 管理器 | [UIManager.cs](Assets/Scripts/Framework/Managers/UIManager.cs) | 画布/分层/弹窗栈/主页面管理 |
| 路由 | [UIRouteNames.cs](Assets/Scripts/Framework/UI/UIRouteNames.cs) | 路由常量 |
| 主页面 | [UIMainPages.cs](Assets/Scripts/Framework/UI/UIMainPages.cs) | 主页面注册 |
| 对话服务 | [UIDialogService.cs](Assets/Scripts/Framework/UI/Services/UIDialogService.cs) | 弹窗服务 |

**层级约定**：Bottom / Normal / Popup / Top

---

### 导航与地图

**导航系统**：
- [NavigationPathSolver](Assets/Scripts/Game/Navigation/Runtime/NavigationPathSolver.cs)：基于 NavMesh 求路
- [NavigationRegistry](Assets/Scripts/Game/Navigation/Runtime/NavigationRegistry.cs)：代理注册表
- [PlayerNavigator](Assets/Scripts/Game/Player/Navigation/PlayerNavigator.cs)：玩家导航代理
- [MonsterNavigator](Assets/Scripts/Game/Monster/Navigation/MonsterNavigator.cs)：怪物导航代理

**地图系统**：
- [MapDataManager](Assets/Scripts/Game/Map/Manager/MapDataManager.cs)：地图配置加载
- [MapPanel](Assets/Scripts/Game/Map/UI/MapPanel.cs)：地图UI显示

---

### 怪物系统

**职责分层**：

| 组件 | 职责 |
|------|------|
| [MonsterEntity](Assets/Scripts/Game/Monster/Runtime/MonsterEntity.cs) | 数据/身份/血量/死亡/存档 |
| [MonsterBrain](Assets/Scripts/Game/Monster/Runtime/MonsterBrain.cs) | 状态判断与命令产生 |
| [MonsterLocomotionExecutor](Assets/Scripts/Game/Monster/Runtime/MonsterLocomotionExecutor.cs) | 命令执行（导航/动画） |
| [MonsterNavigator](Assets/Scripts/Game/Monster/Navigation/MonsterNavigator.cs) | 路径执行与速度采样 |
| [MonsterRuntimeRegistry](Assets/Scripts/Game/Monster/Runtime/MonsterRuntimeRegistry.cs) | 注册表 |
| [MonsterRuntimeService](Assets/Scripts/Game/Monster/Runtime/MonsterRuntimeService.cs) | 创建/恢复服务 |

---

### 存档与数据

| 服务 | 文件 | 职责 |
|------|------|------|
| 玩家数据服务 | [GamePlayerDataService.cs](Assets/Scripts/Game/Player/Save/GamePlayerDataService.cs) | 当前玩家与槽位管理 |
| 存档执行 | [PlayerSaveService.cs](Assets/Scripts/Game/Player/Save/PlayerSaveService.cs) | 存档读写 |
| 槽位策略 | [DataManager.cs](Assets/Scripts/Framework/Managers/DataManager.cs) | 尾部追加策略 |
| 摘要映射 | [PlayerSaveMetaMapper.cs](Assets/Scripts/Game/Player/Save/Mapper/PlayerSaveMetaMapper.cs) | PlayerData → 摘要 |

---

## 常量与资源规范

### 规范要求
- **禁止魔法字符串**：新增资源/对象名先补常量再引用
- **统一加载入口**：`ResourceManager.Instance.Load<T>(AssetPaths.Xxx)`

### 常量类

```csharp
// 资源路径
AssetPaths.Window(UINames.Xxx)

// 对象名
ObjectNames.MiniMapCamera

// 导航常量
NavigationConsts.PlayerAgentId
```

---

## 代码约定

### 单一职责
- **单一移动源**：避免脚本位移与 Root Motion 同时驱动
- **动画写入单一来源**：由执行器统一写入

### 导航门禁
- Navigator 不读取业务状态（除 IsDead）
- stopDistance 建议 0.2

### UI规范
- 主页面独占呈现
- 弹窗经 UIDialogService
- 路由统一 UIRouteNames/UIMainPages

---

## 故障排查

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 收到路径但不动 | stopDistance 过大 | 建议设为 0.2 |
| 攻击后一直不动 | Animator 未触发 AttackOver | 检查 Has Exit Time/条件 |
| 转向"趴下/闪烁" | 使用了俯仰旋转 | 使用平面转向（y=0） |

---

## 工程化改进计划

> 目标：将"可运行原型"升级为"可持续演进工程"

### P0（必须优先完成）
- [ ] 运行时入口去占位化
- [ ] 自动化测试恢复
- [ ] 生命周期统一契约
- [ ] 存档读取语义修复

### P1（建议下一阶段完成）
- [ ] 槽位索引文件与可配置分配策略
- [ ] 导航可观测性升级
- [ ] 场景提交里程碑事件化

### P2（工程卫生与长期治理）
- [ ] namespace 与 asmdef 规范统一
- [ ] 静态检查规则
- [ ] ADR 文档化关键决策

---

## 参考文档

| 文档 | 说明 |
|------|------|
| [architecture.md](docs/architecture.md) | 架构指南 |
| [minimal-runtime-checklist.md](docs/minimal-runtime-checklist.md) | 最小链路自检清单 |
| [module-integration-template.md](docs/module-integration-template.md) | 新模块接入模板 |
| [engineering-recommendations-2026-03-25.md](docs/engineering-recommendations-2026-03-25.md) | 工程改进建议 |

---

## 构建与运行自检

- [ ] 场景包含 BeginScene/GameScene；EventSystem 存在
- [ ] PlayerInput 指向唯一输入资产
- [ ] 资源路径与对象名引用均来自常量
- [ ] DebugCanvas 可加载；Pool 监控可显示

---

## 提交与规范

- **分批提交**：逻辑/资源/场景/AnimatorController/Prefab 分离
- **信息格式**：模块名 + 简述 + 关键影响
- **发布前检查**：无未提交更改；README/architecture 同步
