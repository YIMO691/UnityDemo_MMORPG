using UnityEngine;

namespace PrototypeSkillSystem
{
    public class SkillAtomSound : SkillAtom
    {
        private readonly string soundName;

        public SkillAtomSound(string name = "DefaultSkillSfx", int durationMs = 100)
            : base(E_AtomType.Sound, durationMs)
        {
            soundName = string.IsNullOrEmpty(name) ? "DefaultSkillSfx" : name;
        }

        public override void OnEnter()
        {
            string ownerName = shareData != null && shareData.SourceAnimal != null
                ? shareData.SourceAnimal.Name
                : "Unknown";

            Debug.Log("[SkillAtomSound] Play -> owner=" + ownerName + ", sound=" + soundName + ", skillId=" + (shareData != null ? shareData.SkillID : 0));
        }
    }
}
