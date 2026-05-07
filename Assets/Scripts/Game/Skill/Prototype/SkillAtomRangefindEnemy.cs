using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSkillSystem
{
    public class SkillAtomRangefindEnemy : SkillAtom
    {
        private readonly float range;

        public SkillAtomRangefindEnemy(float findRange = 8f, int durationMs = 100)
            : base(E_AtomType.RangefindEnemy, durationMs)
        {
            range = Mathf.Max(0f, findRange);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (shareData == null || shareData.SourceAnimal == null || shareData.FindEnemyList == null)
                return;

            List<Player> result = new List<Player>();
            float bestSqr = float.MaxValue;
            Player mainTarget = null;

            for (int i = 0; i < shareData.FindEnemyList.Length; i++)
            {
                Player enemy = shareData.FindEnemyList[i];
                if (enemy == null || enemy.IsDead)
                    continue;
                if (enemy.CampId == shareData.SourceAnimal.CampId)
                    continue;

                float sqr = (enemy.Position - shareData.SourceAnimal.Position).sqrMagnitude;
                if (sqr > range * range)
                    continue;

                result.Add(enemy);
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    mainTarget = enemy;
                }
            }

            shareData.FindEnemyList = result.Count > 0 ? result.ToArray() : null;
            shareData.MainTarget = mainTarget;

            Debug.Log("[SkillAtomRangefindEnemy] Find -> count=" + result.Count + ", mainTarget=" + (mainTarget != null ? mainTarget.Name : "null"));
        }
    }
}
