using UnityEngine;

namespace PrototypeSkillSystem
{
    public class SkillAtomAnimation : SkillAtom
    {
        public SkillAtomAnimation()
            : base(E_AtomType.Animation, 300)
        {
        }

        public SkillAtomAnimation(int durationMs)
            : base(E_AtomType.Animation, durationMs)
        {
        }

        public override void OnEnter()
        {
            string ownerName = shareData != null && shareData.SourceAnimal != null
                ? shareData.SourceAnimal.Name
                : "Unknown";

            Debug.Log("[SkillAtomAnimation] OnEnter -> owner=" + ownerName + ", skillId=" + (shareData != null ? shareData.SkillID : 0));
        }
    }
}
