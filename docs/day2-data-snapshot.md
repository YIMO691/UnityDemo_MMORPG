# Day2 数据准备

## PlayerData
- baseData: 角色静态档（roleId, roleName, classId, genderId, appearanceId）
- progressData: 成长进度（level, currentExp, expToNextLevel, skillIds）
- attributeData: 基础属性（maxHp, maxStamina, attack, defense, speed, critRate, critDamage, hitRate, dodgeRate）
- runtimeData: 运行时（currentHp, currentStamina, isDead, Scene, posX, posY, posZ, rotY, hasValidPosition）
- monsterData: 当前场景怪物存活数据（List<MonsterSaveData>）
- inventoryData: 背包（slotCount, slots…）
- 其他字段: saveTime

## 当前已有成长字段
- level: 在 PlayerProgressData.level
- exp: 在 PlayerProgressData.currentExp
- unlockedSkillIds: 以 skillIds 表示（List<int>）

## 当前属性字段
- maxHp
- maxStamina
- attack
- defense
- speed
- critRate
- critDamage
- hitRate
- dodgeRate

## 当前问题
1. 成长数据的经验阈值字段为 expToNextLevel，后续升级公式需与该字段对齐或重命名统一
2. 技能解锁以 skillIds 表示，命名上与“unlockedSkillIds”不一致，后续可考虑语义统一
3. runtimeData 与 attribute/progress 的边界需保持：运行时只保存位置/血量/状态，不写入成长逻辑

