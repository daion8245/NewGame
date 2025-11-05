using newgame.Characters;
using newgame.Services;
using newgame.UI;

namespace newgame.Enemies
{
    internal class Boss : Monster
    {
        private readonly List<SkillType> availableSkills = new List<SkillType>();
        private int bossKey;
        private const int SkillUseChancePercent = 45;
        private static readonly Random Randomizer = new Random();

        public Boss() : this(GameManager.Instance.BattleLogService)
        {
        }

        public Boss(BattleLogService battleLogService) : base(battleLogService)
        {
        }

        public void StartBoss(int floor)
        {
            bossKey = floor;

            Status bossStatus = GameManager.Instance.GetBossStat(floor);
            string displayName = string.IsNullOrWhiteSpace(bossStatus.Name)
                ? $"정체불명의 존재({floor}층)"
                : bossStatus.Name;
            bossStatus.Name = displayName;

            MyStatus = bossStatus;
            if (MyStatus.MaxHp == 0)
            {
                MyStatus.MaxHp = Math.Max(MyStatus.Hp, 1UL);
            }
            if (MyStatus.Hp == 0)
            {
                MyStatus.Hp = MyStatus.MaxHp;
            }
            if (MyStatus.level <= 0)
            {
                MyStatus.level = 1;
            }
            if (MyStatus.ATK == 0)
            {
                MyStatus.ATK = 1UL;
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
                    return BattleSkillLogic(target, skill);
                }
            }

            return base.Attack(target);
        }

        //스킬 클래스에서 스킬을 가져와 사용하는 함수

        string[] BattleSkillLogic(Character target, SkillType skillType)
        {
            SkillType useSkill = skillType;
            if (string.IsNullOrWhiteSpace(useSkill.name))
            {
                return battleLogService.SnapshotBattleLog();
            }

            string[] log = UseAttackSkill(useSkill);

            bool appliedEffect = TryApplyTickSkill(target, useSkill);

            if (appliedEffect)
            {
                battleLogService.ShowBattleInfo(this, target);
                log = battleLogService.SnapshotBattleLog();
            }

            return log;
        }

        bool TryApplyTickSkill(Character target, SkillType skill)
        {
            if (skill.skillTurn <= 0)
            {
                return false;
            }

            if (target == null)
            {
                return false;
            }

            if (target.IsDead && !ReferenceEquals(target, this))
            {
                return false;
            }

            switch (skill.name)
            {
                case "파이어볼":
                case "소드 어택":
                {
                    StatusEffects.EnemyAddTickSkill(skill.name, skill.skillTurn);
                    return true;
                }
                case "영혼 흡수":
                {
                    StatusEffects.EnemyAddTickSkill(skill.name, skill.skillTurn);
                    return true;
                }
                case "물기":
                    {
                        StatusEffects.EnemyAddTickSkill(skill.name, skill.skillTurn);
                        return true;
                    }
                case "아쿠아 볼":
                    {
                        if (!IsDead)
                        {
                            StatusEffects.AddTickSkill(skill.name, skill.skillTurn);
                            return true;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return false;
        }
    }
}
