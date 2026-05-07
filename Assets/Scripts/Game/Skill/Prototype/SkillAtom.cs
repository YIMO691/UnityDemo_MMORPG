using UnityEngine;

namespace PrototypeSkillSystem
{
    public class SkillAtom
    {
        public E_AtomType atomType = E_AtomType.None;
        public int durationTimeMs = 0;

        protected SkillShareData shareData;
        protected int elapsedTimeMs = 0;
        protected bool entered = false;
        protected bool exited = false;

        public bool IsFinished => exited;

        public SkillAtom()
        {
        }

        public SkillAtom(E_AtomType type, int durationMs)
        {
            atomType = type;
            durationTimeMs = Mathf.Max(0, durationMs);
        }

        public virtual void Init(SkillShareData data)
        {
            shareData = data;
            elapsedTimeMs = 0;
            entered = false;
            exited = false;
        }

        public virtual void UnInit()
        {
            shareData = null;
            elapsedTimeMs = 0;
            entered = false;
            exited = false;
        }

        public virtual bool Update(float deltatime)
        {
            if (exited)
                return true;

            if (!entered)
            {
                OnEnter();
                entered = true;
            }

            elapsedTimeMs += Mathf.RoundToInt(Mathf.Max(0f, deltatime) * 1000f);
            if (elapsedTimeMs >= durationTimeMs)
            {
                OnExit();
                exited = true;
            }

            return exited;
        }

        public virtual void OnEnter()
        {
            Debug.Log("[SkillAtom] Enter -> type=" + atomType + ", skillId=" + (shareData != null ? shareData.SkillID : 0));
        }

        public virtual void OnExit()
        {
            Debug.Log("[SkillAtom] Exit -> type=" + atomType + ", skillId=" + (shareData != null ? shareData.SkillID : 0));
        }
    }
}
