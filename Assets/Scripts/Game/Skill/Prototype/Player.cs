using UnityEngine;

namespace PrototypeSkillSystem
{
    public class Player
    {
        public DoSkillComp doSkillComp = null;

        public string Name { get; set; } = "PrototypePlayer";
        public int DefaultSkillId { get; set; } = 1001;
        public Vector3 Position { get; set; } = Vector3.zero;
        public Player[] FindEnemyList { get; set; } = null;
        public Vector3 Forward { get; private set; } = Vector3.forward;
        public int CampId { get; set; } = 0;
        public int MaxHp { get; set; } = 100;
        public int CurrentHp { get; private set; } = 100;
        public int Attack { get; set; } = 10;
        public float MoveSpeed { get; set; } = 3f;
        public bool IsDead => CurrentHp <= 0;

        public Player()
        {
        }

        public Player(string name, int campId, Vector3 position)
        {
            Name = name;
            CampId = campId;
            Position = position;
            CurrentHp = MaxHp;
        }

        public void Init()
        {
            CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);

            if (doSkillComp == null)
            {
                doSkillComp = SkillMgr.instance.CreateDoSkillComp(this);
            }
        }

        public void DoSkill()
        {
            DoSkill(DefaultSkillId);
        }

        public void DoSkill(int skillId)
        {
            Init();

            Skill skill = SkillMgr.instance.CreateSkill(skillId);
            skill.BindShareData(this, FindEnemyList);

            doSkillComp.AddSkill(skill);
            doSkillComp.DoSkill();
        }

        public void StopSkill()
        {
            if (doSkillComp == null)
                return;

            doSkillComp.StopSkill();
        }

        public void SetEnemies(Player[] enemies)
        {
            FindEnemyList = enemies;
        }

        public void ReceiveDamage(int damage)
        {
            if (IsDead)
                return;

            CurrentHp = Mathf.Max(0, CurrentHp - Mathf.Max(0, damage));
            Debug.Log("[PrototypePlayer] ReceiveDamage -> name=" + Name + ", damage=" + damage + ", hp=" + CurrentHp + "/" + MaxHp);
        }

        public void RecoverHp(int value)
        {
            if (IsDead)
                return;

            CurrentHp = Mathf.Min(MaxHp, CurrentHp + Mathf.Max(0, value));
        }

        public void MoveTowards(Vector3 targetPosition, float deltatime)
        {
            Vector3 delta = targetPosition - Position;
            if (delta.sqrMagnitude <= 0.0001f)
                return;

            Vector3 direction = delta.normalized;
            Forward = direction;
            Position += direction * MoveSpeed * Mathf.Max(0f, deltatime);
        }

        public void TurnTowards(Vector3 targetPosition)
        {
            Vector3 delta = targetPosition - Position;
            if (delta.sqrMagnitude <= 0.0001f)
                return;

            Forward = delta.normalized;
        }

        public void Update(float deltatime)
        {
            if (doSkillComp == null)
                return;

            doSkillComp.Update(deltatime);
        }
    }
}
