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

        public void StartBoss(int floor)
        {
            bossKey = floor;

            Status bossStatus = GameManager.Instance.GetBossStat(floor);
            string displayName = string.IsNullOrWhiteSpace(bossStatus.Name)
                ? $"정체불명의 존재({floor}층)"
                : bossStatus.Name;
            bossStatus.Name = displayName;

            MyStatus = bossStatus;
            if (MyStatus.maxHp <= 0)
            {
                MyStatus.maxHp = Math.Max(MyStatus.Hp, 1);
            }
            if (MyStatus.Hp <= 0)
            {
                MyStatus.Hp = MyStatus.maxHp;
            }
            if (MyStatus.level <= 0)
            {
                MyStatus.level = 1;
            }
            if (MyStatus.ATK <= 0)
            {
                MyStatus.ATK = 1;
            }
            if (MyStatus.DEF < 0)
            {
                MyStatus.DEF = 0;
            }

            Console.Clear();
            UiHelper.TxtOut(new string[]
            {
                $"{displayName}이(가) 나타났다!",
                ""
            });
            UiHelper.WaitForInput();

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
