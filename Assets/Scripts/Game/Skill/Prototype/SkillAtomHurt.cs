using UnityEngine;

namespace PrototypeSkillSystem
{
    public class SkillAtomHurt : SkillAtom
    {
        private readonly int damageValue;

        public SkillAtomHurt(int damage = 10, int durationMs = 120)
            : base(E_AtomType.Hurt, durationMs)
        {
            damageValue = Mathf.Max(0, damage);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (shareData == null || shareData.MainTarget == null || shareData.MainTarget.IsDead)
            {
                Debug.LogWarning("[SkillAtomHurt] no valid target.");
                return;
            }

            int finalDamage = damageValue;
            if (shareData.SourceAnimal != null)
            {
                finalDamage += shareData.SourceAnimal.Attack;
            }

            shareData.MainTarget.ReceiveDamage(finalDamage);

            Debug.Log("[SkillAtomHurt] Apply -> target=" + shareData.MainTarget.Name + ", damage=" + finalDamage + ", skillId=" + (shareData != null ? shareData.SkillID : 0));
        }
    }
}
