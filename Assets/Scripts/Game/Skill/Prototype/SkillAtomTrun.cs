using UnityEngine;

namespace PrototypeSkillSystem
{
    public class SkillAtomTrun : SkillAtom
    {
        public SkillAtomTrun(int durationMs = 80)
            : base(E_AtomType.Trun, durationMs)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (shareData == null || shareData.SourceAnimal == null || shareData.MainTarget == null)
                return;

            shareData.SourceAnimal.TurnTowards(shareData.MainTarget.Position);

            Debug.Log("[SkillAtomTrun] Turn -> owner=" + shareData.SourceAnimal.Name + ", target=" + shareData.MainTarget.Name);
        }
    }
}
