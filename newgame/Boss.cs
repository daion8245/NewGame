using System;
using System.Collections.Generic;

namespace newgame
{
    internal class Boss : Monster
    {
        private readonly List<SkillType> availableSkills = new List<SkillType>();
        private int bossKey;
        private const int SkillUseChancePercent = 35;
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
                $"보스 {displayName}이(가) 나타났다!",
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
                    BattleSkillLogic(target,skill);
                    return UseAttackSkill(skill);
                }
            }

            return base.Attack(target);
        }

        //스킬 클래스에서 스킬을 가져와 사용하는 함수

        void BattleSkillLogic(Character target, SkillType skillType)
        {
            SkillType useSkill = skillType;
            if (string.IsNullOrWhiteSpace(useSkill.name))
            {
                return;
            }

            // 즉시 효과 처리 (기존 동작 유지)
            UseAttackSkill(useSkill);

            // 지속효과 적용은 대상이 누구인지 명확히 지정해서 적용
            switch (useSkill.name)
            {
                case "파이어볼":
                    {
                        // 파이어볼은 적(target)에게 지속 효과를 남겨야 함
                        InvokeProtectedAddTickSkill(target, useSkill.name, useSkill.skillTurn);
                        break;
                    }
                case "아쿠아 볼":
                    {
                        // 아쿠아 볼은 보스(self)에게 적용
                        InvokeProtectedAddTickSkill(this, useSkill.name, useSkill.skillTurn);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        void InvokeProtectedAddTickSkill(Character receiver, string skillName, int duration)
        {
            if (receiver == null) return;

            try
            {
                // protected 메서드인 AddTickSkill를 리플렉션으로 호출 시도
                var binding = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy;
                var method = receiver.GetType().GetMethod("AddTickSkill", binding);
                if (method != null)
                {
                    method.Invoke(receiver, new object[] { skillName, duration });
                    return;
                }

                // AddTickSkill이 없을 경우 EnemyAddTickSkill도 시도 (안전망)
                method = receiver.GetType().GetMethod("EnemyAddTickSkill", binding);
                if (method != null)
                {
                    method.Invoke(receiver, new object[] { skillName, duration });
                    return;
                }

                // 실패 시 아무 것도 하지 않음 (디버그용 로그를 원하면 여기에 추가)
            }
            catch
            {
                // 리플렉션 호출 중 문제 발생 시 무시 (필요하면 로그 추가)
            }
        }
    }
}
