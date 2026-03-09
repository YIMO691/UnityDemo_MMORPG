# MyUnityProject — Data-driven Unity RPG Demo

**Demo 目标**：Unity 客户端作品集 Demo，覆盖 **战斗 / 系统 / UGUI / 性能优化**，并采用 **JSON 数据驱动（配置化）**。

> 关键词：Data-driven / Config / Combat / UI / Inventory / Drop / Pool / Profiler

---

## 3 分钟演示流程（面试官快速体验）
1. 登录（或进入主菜单）
2. 进入场景（主城/关卡）
3. 打怪（技能/普攻/受击/数值结算）
4. 掉落（掉落展示/拾取）
5. 打开背包（物品列表、筛选/排序如有）
6. 使用 / 装备（消耗品、生效提示）
7. 属性变化（UI 数值刷新：攻击/防御/HP 等）

> 建议你录一个 2~3 分钟的 gif/mp4，放到这里（可选但很加分）

---

## 架构图（UI / 系统 / 数据 / 框架）
```mermaid
flowchart TB
  UI[UGUI / UI Views] -->|Events| SYS[Game Systems]
  SYS --> COMBAT[Combat: Damage/Skill/Buff]
  SYS --> INV[Inventory: Bag/Equip/Use]
  SYS --> DROP[Drop: Roll/Spawn/Pickup]
  SYS --> DATA[Data Layer: JSON Configs]
  DATA -->|Load/Parse| CFG[Config Models]
  SYS --> CORE[Core/Framework: EventBus / Timer / Pool / FSM]
  CORE --> PERF[Performance: Pooling / UI Refresh Strategy / Profiler]
