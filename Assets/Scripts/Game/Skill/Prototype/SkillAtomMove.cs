using UnityEngine;

namespace PrototypeSkillSystem
{
    public class SkillAtomMove : SkillAtom
    {
        private readonly float moveDistance;
        private float movedDistance;

        public SkillAtomMove(float distance = 2f, int durationMs = 250)
            : base(E_AtomType.Move, durationMs)
        {
            moveDistance = Mathf.Max(0f, distance);
        }

        public override void Init(SkillShareData data)
        {
            base.Init(data);
            movedDistance = 0f;
        }

        public override bool Update(float deltatime)
        {
            if (!entered)
            {
                OnEnter();
                entered = true;
            }

            if (shareData != null && shareData.SourceAnimal != null)
            {
                Vector3 targetPosition = shareData.MainTarget != null
                    ? shareData.MainTarget.Position
                    : shareData.SourceAnimal.Position + shareData.SourceAnimal.Forward * moveDistance;

                Vector3 before = shareData.SourceAnimal.Position;
                shareData.SourceAnimal.MoveTowards(targetPosition, deltatime);
                movedDistance += Vector3.Distance(before, shareData.SourceAnimal.Position);
            }

            elapsedTimeMs += Mathf.RoundToInt(Mathf.Max(0f, deltatime) * 1000f);
            if (elapsedTimeMs >= durationTimeMs || movedDistance >= moveDistance)
            {
                if (!exited)
                {
                    OnExit();
                    exited = true;
                }
            }

            return exited;
        }
    }
}
