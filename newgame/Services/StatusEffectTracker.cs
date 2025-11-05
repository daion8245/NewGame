using newgame.Characters;

namespace newgame.Services
{
    internal readonly struct SkillTickLog
    {
        public SkillTickLog(Character actor, Character target, string message, bool targetDefeated, bool clearOpponent = false)
        {
            Actor = actor;
            Target = target;
            Message = message;
            TargetDefeated = targetDefeated;
            ClearOpponent = clearOpponent;
        }

        public Character Actor { get; }
        public Character Target { get; }
        public string Message { get; }
        public bool TargetDefeated { get; }
        public bool ClearOpponent { get; }
    }

    internal interface IStatusEffectTracker
    {
        void AddTickSkill(string skillName, int duration, Character? caster = null);
        void EnemyAddTickSkill(string skillName, int duration);
        string GetActiveSkillEffectDisplay();
        List<SkillTickLog> TickSkillTurns();
        void Clear();
    }

    internal sealed class StatusEffectTracker : IStatusEffectTracker
    {
        private readonly Character owner;
        private readonly Func<Character?> targetResolver;
        private readonly Func<Character, Character, int, string?, bool, bool, string> messageBuilder;
        private readonly Dictionary<string, int> activeSkills = new();
        private readonly Dictionary<string, Character?> activeSkillCasters = new();

        public StatusEffectTracker(
            Character owner,
            Func<Character?> targetResolver,
            Func<Character, Character, int, string?, bool, bool, string> messageBuilder)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.targetResolver = targetResolver ?? throw new ArgumentNullException(nameof(targetResolver));
            this.messageBuilder = messageBuilder ?? throw new ArgumentNullException(nameof(messageBuilder));
        }

        public void AddTickSkill(string skillName, int duration, Character? caster = null)
        {
            if (string.IsNullOrWhiteSpace(skillName) || duration <= 0)
            {
                return;
            }

            activeSkills[skillName] = duration;
            activeSkillCasters[skillName] = caster ?? owner;
        }

        public void EnemyAddTickSkill(string skillName, int duration)
        {
            Character? target = targetResolver();
            if (target == null)
            {
                return;
            }

            target.StatusEffects.AddTickSkill(skillName, duration, owner);
        }

        public string GetActiveSkillEffectDisplay()
        {
            if (activeSkills.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new();
            foreach (var skillName in activeSkills.Keys)
            {
                string effectName = GetSkillEffectLabel(skillName);
                if (string.IsNullOrWhiteSpace(effectName))
                {
                    continue;
                }

                labels.Add($"[{effectName}]");
            }

            return labels.Count == 0 ? string.Empty : string.Join(string.Empty, labels);
        }

        public List<SkillTickLog> TickSkillTurns()
        {
            List<SkillTickLog> logs = new();

            if (activeSkills.Count == 0)
            {
                return logs;
            }

            var keys = new List<string>(activeSkills.Keys);

            foreach (var skillName in keys)
            {
                if (!activeSkills.TryGetValue(skillName, out int remain))
                {
                    continue;
                }

                remain -= 1;
                activeSkills[skillName] = remain;

                Character caster = GetSkillCaster(skillName);

                SkillTickLog? effectLog = SkillTickEffect(skillName, caster, Math.Max(remain, 0));
                if (effectLog.HasValue)
                {
                    logs.Add(effectLog.Value);
                }

                if (activeSkills.TryGetValue(skillName, out int currentRemain) && currentRemain <= 0)
                {
                    activeSkills.Remove(skillName);
                    activeSkillCasters.Remove(skillName);
                    logs.Add(new SkillTickLog(caster, owner, $"{skillName} 효과가 종료되었습니다.", false));
                }
            }

            return logs;
        }

        public void Clear()
        {
            activeSkills.Clear();
            activeSkillCasters.Clear();
        }

        public static void ResolveTickDeaths(IEnumerable<SkillTickLog> logs)
        {
            foreach (var log in logs)
            {
                if (log.TargetDefeated && !log.Target.IsDead)
                {
                    log.Target.Dead(log.Actor);
                }
            }
        }

        private Character GetSkillCaster(string skillName)
        {
            if (activeSkillCasters.TryGetValue(skillName, out Character? caster) && caster != null)
            {
                return caster;
            }

            return targetResolver() ?? owner;
        }

        private string GetSkillEffectLabel(string skillName)
        {
            return skillName switch
            {
                "파이어볼" => "화상",
                "소드 어택" => "출혈",
                "영혼 흡수" => "저주",
                "물기" => "출혈",
                _ => skillName
            };
        }

        private SkillTickLog? SkillTickEffect(string skill, Character caster, int remainingTurns)
        {
            switch (skill)
            {
                case "파이어볼":
                    {
                        Character? target = targetResolver();
                        ulong referenceHp = target?.HasStatus == true ? target.MyStatus.Hp : owner.MyStatus.Hp;
                        ulong rawDamage = 1UL + (referenceHp / 20UL);
                        int dotDamage = rawDamage >= (ulong)int.MaxValue ? int.MaxValue : (int)rawDamage;

                        ulong currentHp = owner.MyStatus.Hp;
                        ulong applied = (ulong)Math.Max(dotDamage, 0);
                        owner.MyStatus.Hp = applied >= currentHp ? 0 : currentHp - applied;

                        bool defeated = owner.MyStatus.Hp == 0;
                        int remain = Math.Max(remainingTurns, 0);
                        string label = $"{skill}(지속)";
                        string message = messageBuilder(caster, owner, dotDamage, label, defeated, false) + $" (남은 턴: {remain})";
                        return new SkillTickLog(caster, owner, message, defeated);
                    }
                case "소드 어택":
                    {
                        Character? target = targetResolver();
                        int dotDamage = 5;
                        ulong currentHp = owner.MyStatus.Hp;
                        ulong applied = (ulong)Math.Max(dotDamage, 0);
                        owner.MyStatus.Hp = applied >= currentHp ? 0 : currentHp - applied;
                        bool defeated = owner.MyStatus.Hp == 0;
                        int remain = Math.Max(remainingTurns, 0);
                        string label = $"{skill}(지속)";
                        string message = messageBuilder(caster, owner, dotDamage, label, defeated, false) + $" (남은 턴: {remain})";
                        return new SkillTickLog(caster, owner, message, defeated);
                    }
                case "영혼 흡수":
                    {
                        Character? target = targetResolver();
                        int dotDamage = 7;
                        ulong applied = (ulong)Math.Max(dotDamage, 0);
                        ulong currentHp = owner.MyStatus.Hp;
                        owner.MyStatus.Hp = applied >= currentHp ? 0 : currentHp - applied;

                        if (target != null)
                        {
                            ulong healAmount = 7UL;
                            ulong targetHp = target.MyStatus.Hp;
                            ulong maxHp = target.MyStatus.MaxHp;
                            ulong healed = targetHp > maxHp - healAmount ? maxHp : targetHp + healAmount;
                            target.MyStatus.Hp = healed;
                        }

                        bool defeated = owner.MyStatus.Hp == 0;
                        int remain = Math.Max(remainingTurns, 0);
                        string label = $"{skill}(지속)";
                        string message = messageBuilder(caster, owner, dotDamage, label, defeated, false) + $" (남은 턴: {remain})";
                        return new SkillTickLog(caster, owner, message, defeated);
                    }
                case "물기":
                    {
                        Character? target = targetResolver();
                        int dotDamage = 3;
                        ulong applied = (ulong)Math.Max(dotDamage, 0);
                        ulong currentHp = owner.MyStatus.Hp;
                        owner.MyStatus.Hp = applied >= currentHp ? 0 : currentHp - applied;
                        bool defeated = owner.MyStatus.Hp == 0;
                        int remain = Math.Max(remainingTurns, 0);
                        string label = $"{skill}(지속)";
                        string message = messageBuilder(caster, owner, dotDamage, label, defeated, false) + $" (남은 턴: {remain})";
                        return new SkillTickLog(caster, owner, message, defeated);
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
