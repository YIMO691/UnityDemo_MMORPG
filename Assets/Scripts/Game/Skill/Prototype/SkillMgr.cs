using System.Collections.Generic;

namespace PrototypeSkillSystem
{
    public class SkillMgr
    {
        public static readonly SkillMgr instance = new SkillMgr();

        private readonly Stack<DoSkillComp> doSkillCompPool = new Stack<DoSkillComp>();
        private readonly Stack<Skill> skillPool = new Stack<Skill>();

        public DoSkillComp CreateDoSkillComp(Player player)
        {
            DoSkillComp doSkillComp = doSkillCompPool.Count > 0
                ? doSkillCompPool.Pop()
                : new DoSkillComp();

            doSkillComp.Init(player);
            return doSkillComp;
        }

        public void RecoverDoSkillComp(DoSkillComp doSkillComp)
        {
            if (doSkillComp == null)
                return;

            doSkillComp.Clear();
            doSkillCompPool.Push(doSkillComp);
        }

        public Skill CreateSkill(int skillId)
        {
            Skill skill = skillPool.Count > 0
                ? skillPool.Pop()
                : new Skill();

            skill.Init(skillId);
            BuildDefaultAtoms(skillId, skill);
            return skill;
        }

        public void RecoverSkill(Skill skill)
        {
            if (skill == null)
                return;

            skill.Stop();
            skillPool.Push(skill);
        }

        private static void BuildDefaultAtoms(int skillId, Skill skill)
        {
            switch (skillId)
            {
                case 1001:
                    skill.AddAtom(new SkillAtomAnimation(300));
                    skill.AddAtom(new SkillAtomSound("PowerShot", 100));
                    skill.AddAtom(new SkillAtomRangefindEnemy(8f, 100));
                    skill.AddAtom(new SkillAtomTrun(60));
                    skill.AddAtom(new SkillAtomHurt(15, 120));
                    break;

                case 1002:
                    skill.AddAtom(new SkillAtomAnimation(250));
                    skill.AddAtom(new SkillAtomRangefindEnemy(5f, 100));
                    skill.AddAtom(new SkillAtomMove(2f, 220));
                    skill.AddAtom(new SkillAtomHurt(25, 100));
                    break;

                case 1003:
                    skill.AddAtom(new SkillAtomAnimation(200));
                    skill.AddAtom(new SkillAtomSound("QuickTurn", 80));
                    skill.AddAtom(new SkillAtomRangefindEnemy(6f, 80));
                    skill.AddAtom(new SkillAtomTrun(80));
                    break;

                default:
                    skill.AddAtom(new SkillAtomAnimation(200));
                    skill.AddAtom(new SkillAtomSound("DefaultSkill", 80));
                    break;
            }
        }
    }
}
