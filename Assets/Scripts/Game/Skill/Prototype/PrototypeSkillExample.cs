using UnityEngine;

namespace PrototypeSkillSystem
{
    public static class PrototypeSkillExample
    {
        public static Player CreateSamplePlayerWithEnemies()
        {
            Player caster = new Player("Caster", 1, Vector3.zero)
            {
                Attack = 12,
                MaxHp = 100,
                MoveSpeed = 4f,
                DefaultSkillId = 1001
            };

            Player enemyA = new Player("EnemyA", 2, new Vector3(2f, 0f, 3f));
            Player enemyB = new Player("EnemyB", 2, new Vector3(6f, 0f, 1f));
            caster.SetEnemies(new[] { enemyA, enemyB });
            caster.Init();
            return caster;
        }
    }
}
