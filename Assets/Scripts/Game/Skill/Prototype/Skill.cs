using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSkillSystem
{
    public class Skill
    {
        public int SkillId = 0;
        public List<SkillAtom> SkillAtomList = null;

        private int durationMs = 0;
        private SkillShareData skillShareData = null;
        private bool isRunning = false;
        private bool isPaused = false;
        private int elapsedTimeMs = 0;

        public bool IsRunning => isRunning;
        public bool IsFinished => !isRunning && elapsedTimeMs >= durationMs;

        public void Init(int skillID)
        {
            SkillId = skillID;
            SkillAtomList = new List<SkillAtom>();
            skillShareData = new SkillShareData
            {
                SkillID = skillID
            };
            durationMs = 0;
            elapsedTimeMs = 0;
            isRunning = false;
            isPaused = false;
        }

        public void BindShareData(Player sourceAnimal, Player[] enemies)
        {
            if (skillShareData == null)
            {
                skillShareData = new SkillShareData();
            }

            skillShareData.SkillID = SkillId;
            skillShareData.SourceAnimal = sourceAnimal;
            skillShareData.FindEnemyList = enemies;
        }

        public void AddAtom(SkillAtom atom)
        {
            if (atom == null)
                return;

            if (SkillAtomList == null)
            {
                SkillAtomList = new List<SkillAtom>();
            }

            SkillAtomList.Add(atom);
            durationMs = Mathf.Max(durationMs, atom.durationTimeMs);
        }

        public void Update(float deltatime)
        {
            if (!isRunning || isPaused || SkillAtomList == null)
                return;

            elapsedTimeMs += Mathf.RoundToInt(Mathf.Max(0f, deltatime) * 1000f);

            bool allFinished = true;
            for (int i = 0; i < SkillAtomList.Count; i++)
            {
                SkillAtom atom = SkillAtomList[i];
                if (atom == null)
                    continue;

                bool finished = atom.Update(deltatime);
                if (!finished)
                {
                    allFinished = false;
                }
            }

            if (allFinished || elapsedTimeMs >= durationMs)
            {
                Stop();
            }
        }

        public void Start()
        {
            if (SkillAtomList == null || SkillAtomList.Count == 0)
            {
                Debug.LogWarning("[Skill] Start failed, no atoms. skillId=" + SkillId);
                return;
            }

            elapsedTimeMs = 0;
            isPaused = false;
            isRunning = true;

            for (int i = 0; i < SkillAtomList.Count; i++)
            {
                SkillAtom atom = SkillAtomList[i];
                if (atom == null)
                    continue;
                atom.Init(skillShareData);
            }
        }

        public void Pause()
        {
            if (!isRunning)
                return;

            isPaused = true;
        }

        public void Stop()
        {
            if (!isRunning && SkillAtomList != null)
            {
                for (int i = 0; i < SkillAtomList.Count; i++)
                {
                    SkillAtom atom = SkillAtomList[i];
                    if (atom == null)
                        continue;
                    atom.UnInit();
                }

                return;
            }

            isRunning = false;
            isPaused = false;
            elapsedTimeMs = durationMs;

            if (SkillAtomList == null)
                return;

            for (int i = 0; i < SkillAtomList.Count; i++)
            {
                SkillAtom atom = SkillAtomList[i];
                if (atom == null)
                    continue;

                if (!atom.IsFinished)
                {
                    atom.OnExit();
                }

                atom.UnInit();
            }
        }
    }
}
