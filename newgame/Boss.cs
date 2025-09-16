using System;
using System.Collections.Generic;

namespace newgame
{
    internal class Boss : Monster
    {
        private readonly List<SkillType> availableSkills = new List<SkillType>();
        private int bossKey;
        private const int SkillUseChancePercent = 40;
        private static readonly Random Randomizer = new Random();

        public void StartBoss(int bossType)
        {
            bossKey = bossType;
            base.Start(bossType);
            LoadBossSkills();
        }

        void LoadBossSkills()
        {
            availableSkills.Clear();
            foreach (SkillType skill in GameManager.Instance.GetBossSkills(bossKey))
            {
                availableSkills.Add(skill);
            }
        }

        public override string[] Attack(Character target)
        {
            if (availableSkills.Count > 0)
            {
                if (Randomizer.Next(100) < SkillUseChancePercent)
                {
                    SkillType skill = availableSkills[Randomizer.Next(availableSkills.Count)];
                    return UseAttackSkill(skill);
                }
            }

            return base.Attack(target);
        }
    }
}
