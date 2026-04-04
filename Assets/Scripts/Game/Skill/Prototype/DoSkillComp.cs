using System.Collections.Generic;

namespace PrototypeSkillSystem
{
    public class DoSkillComp
    {
        public List<Skill> SkillList = null;

        private Player owner = null;

        public void Init(Player player)
        {
            owner = player;
            SkillList = SkillList ?? new List<Skill>();
        }

        public void Update(float deltatime)
        {
            if (SkillList == null || SkillList.Count == 0)
                return;

            for (int i = SkillList.Count - 1; i >= 0; i--)
            {
                Skill skill = SkillList[i];
                if (skill == null)
                {
                    SkillList.RemoveAt(i);
                    continue;
                }

                skill.Update(deltatime);
                if (skill.IsFinished)
                {
                    SkillMgr.instance.RecoverSkill(skill);
                    SkillList.RemoveAt(i);
                }
            }
        }

        public void AddSkill(Skill skill)
        {
            if (skill == null)
                return;

            SkillList = SkillList ?? new List<Skill>();
            SkillList.Add(skill);
        }

        public void DoSkill()
        {
            if (SkillList == null)
                return;

            for (int i = 0; i < SkillList.Count; i++)
            {
                Skill skill = SkillList[i];
                if (skill == null || skill.IsRunning)
                    continue;

                skill.Start();
            }
        }

        public void StopSkill()
        {
            if (SkillList == null)
                return;

            for (int i = SkillList.Count - 1; i >= 0; i--)
            {
                Skill skill = SkillList[i];
                if (skill == null)
                    continue;

                skill.Stop();
                SkillMgr.instance.RecoverSkill(skill);
            }

            SkillList.Clear();
        }

        public void Clear()
        {
            StopSkill();
            owner = null;
        }
    }
}
