using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data;

namespace newgame
{
    //능력치를 가지고 있는 모든 캐릭터의 부모 클래스
    internal class Character
    {
        Status? Status;

        public Status MyStatus
        {
            get => Status ?? new Status();
            protected set => Status = value;
        }

        //현제 활성화 되어있는 포션 효과
        private List<ActiveItemEffect> activeEffects = new();
        //현제 지속되고 있는 스킬의 지속 효과
        protected Dictionary<string, int> activeSkills = new Dictionary<string, int>();
        protected Dictionary<string, Character> activeSkillCasters = new();

        //플레이어가 죽었는지
        public bool IsDead = false;
        //플레이어가 전투에서 도망쳤는지
        public bool isbattleRun = false;

        protected static readonly string[] battleLog = new string[2];

        #region 로그 관련
        protected static string[] SnapshotBattleLog()
        {
            return new[]
            {
                battleLog[0] ?? string.Empty,
                battleLog[1] ?? string.Empty
            };
        }

        protected static void ResetBattleLog()
        {
            battleLog[0] = string.Empty;
            battleLog[1] = string.Empty;
        }

        protected static bool IsPlayer(Character actor)
        {
            return ReferenceEquals(actor, GameManager.Instance.player);
        }

        private static string FormatLogLine(string message, bool isFirstLine)
        {
            string content = (message ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            string prefix = isFirstLine ? "->" : " ->";
            return prefix + content;
        }

        private void UpdateBattleMessage(Character attacker, string message, bool clearOpponentMessage)
        {
            bool isPlayer = IsPlayer(attacker);
            int index = isPlayer ? 0 : 1;

            string formatted = FormatLogLine(message, true);
            battleLog[index] = formatted;

            if (clearOpponentMessage)
            {
                battleLog[isPlayer ? 1 : 0] = string.Empty;
            }
        }

        private void AppendBattleMessage(Character actor, string message)
        {
            bool isPlayer = IsPlayer(actor);
            int index = isPlayer ? 0 : 1;

            string existing = battleLog[index] ?? string.Empty;
            bool isFirstLine = string.IsNullOrEmpty(existing);
            string formatted = FormatLogLine(message, isFirstLine);

            if (string.IsNullOrEmpty(formatted))
            {
                return;
            }

            battleLog[index] = isFirstLine ? formatted : existing + "\n" + formatted;
        }

        private static void ClearBattleMessageForActor(Character actor)
        {
            if (actor == null)
            {
                return;
            }

            if (ReferenceEquals(actor, GameManager.Instance.player))
            {
                battleLog[0] = string.Empty;
            }
            else if (ReferenceEquals(actor, GameManager.Instance.monster))
            {
                battleLog[1] = string.Empty;
            }
        }

        private static void ClearBattleMessageForOpponent(Character actor)
        {
            if (actor == null)
            {
                return;
            }

            if (ReferenceEquals(actor, GameManager.Instance.player))
            {
                battleLog[1] = string.Empty;
            }
            else if (ReferenceEquals(actor, GameManager.Instance.monster))
            {
                battleLog[0] = string.Empty;
            }
        }

        private void ApplyTickLogs(IEnumerable<SkillTickLog> tickLogs)
        {
            foreach (var log in tickLogs)
            {
                AppendBattleMessage(log.Actor, log.Message);

                if (log.ClearOpponent)
                {
                    ClearBattleMessageForOpponent(log.Actor);
                }
            }
        }

        private static string BuildActionMessage(Character attacker, Character defender, int damage, string? actionName, bool targetDefeated)
        {
            string label = string.IsNullOrWhiteSpace(actionName) ? "공격" : actionName!;
            string prefix = $"{attacker.MyStatus.Name}의 {label}!";
            string suffix = $"{defender.MyStatus.Name}은 {damage}의 피해를 입었다. 남은 체력: {defender.MyStatus.Hp}/{defender.MyStatus.maxHp}";

            if (targetDefeated)
            {
                suffix += " (쓰러짐)";
            }

            return $"{prefix} {suffix}";
        }
        #endregion

        #region 전투 진입
        Character? target;

        public void EnteringBattle(Character target)
        {
            this.target = target;

            if (IsPlayer(this))
            {
                ResetBattleLog();
            }
        }
        #endregion

        #region 기본공격
        /// <summary>
        /// 플레이어 기본공격 메서드
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual string[] Attack(Character target)
        {
            if (target == null)
                return SnapshotBattleLog();

            ClearBattleMessageForActor(this);

            List<SkillTickLog> tickLogs = TickSkillTurns();

            // 지속 피해로 사망자가 발생하면 즉시 처리 후 return
            bool anyDeath = tickLogs.Any(log => log.TargetDefeated);
            if (anyDeath)
            {
                ApplyTickLogs(tickLogs);
                ShowBattleInfo(target, battleLog);
                ResolveTickDeaths(tickLogs);
                return SnapshotBattleLog();
            }

            // 사망자가 없을 때만 공격 데미지 계산
            int damage = Damage(target, MyStatus.ATK);
            MyStatus.mp = Math.Min(MyStatus.maxMp, MyStatus.mp + 10);
            bool defeated = target.Status.Hp <= 0;
            string message = BuildActionMessage(this, target, damage, null, defeated);

            bool clearOpponent = IsPlayer(this);
            UpdateBattleMessage(this, message, clearOpponent);

            ApplyTickLogs(tickLogs);

            ShowBattleInfo(target, battleLog);

            if (defeated)
            {
                Thread.Sleep(1000);
                target.Dead(this);
            }

            ResolveTickDeaths(tickLogs);

            beforHP[0] = MyStatus.Hp;
            beforHP[1] = target.MyStatus.Hp;

            return SnapshotBattleLog();
        }
        #endregion

        #region 스킬 공격
        /// <summary>
        /// 스킬 사용 기본 메서드
        /// </summary>
        /// <param name="target"></param>
        /// <param name="skillNum"></param>
        /// <returns></returns>
        public virtual string[] UseAttackSkill(SkillType skill)
        {
            if (target == null || string.IsNullOrWhiteSpace(skill.name))
            {
                return SnapshotBattleLog();
            }

            ClearBattleMessageForActor(this);

            List<SkillTickLog> tickLogs = TickSkillTurns();

            bool attackerDiedFromTick = tickLogs.Any(log => ReferenceEquals(log.Target, this) && log.TargetDefeated);
            bool preserveOpponentLog = tickLogs.Any(log => IsPlayer(log.Actor) != IsPlayer(this));

            if (attackerDiedFromTick)
            {
                ApplyTickLogs(tickLogs);
                ShowBattleInfo(target, battleLog);
                ResolveTickDeaths(tickLogs);
                return SnapshotBattleLog();
            }

            if(this is Player)
            {
                MyStatus.mp -= skill.skillMana;
            }

            int damage = Damage(target, skill.skillDamage);

            bool defeated = target.Status.Hp <= 0;
            string message = BuildActionMessage(this, target, damage, skill.name, defeated);

            bool clearOpponent = IsPlayer(this) && !preserveOpponentLog;
            UpdateBattleMessage(this, message, clearOpponent);

            ApplyTickLogs(tickLogs);

            ShowBattleInfo(target, battleLog);

            if (defeated)
            {
                Thread.Sleep(1000);
                target.Dead(this);
            }

            ResolveTickDeaths(tickLogs);

            return SnapshotBattleLog();
        }
        #endregion

        #region 사망
        /// <summary>
        /// 사망 메서드
        /// </summary>
        /// <param name="target"></param>
        public virtual void Dead(Character target)
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            activeSkills.Clear();
            activeSkillCasters.Clear();

            if (target != null)
            {
                target.activeSkills.Clear();
                target.activeSkillCasters.Clear();
            }

            ResetBattleLog();

            bool killedByPlayer = ReferenceEquals(target, GameManager.Instance.player);
            bool playerDied = ReferenceEquals(this, GameManager.Instance.player);

            if (killedByPlayer && !playerDied)
            {
                UiHelper.TxtOut([
                    $"{Status.Name}은 쓰러졌다!",
                    $"{Status.Name}에게서 승리했다!",
                    $"+{Status.exp}Exp , +{Status.gold}골드 를 획득했다!",
                    $"다음 레벨까지 : {target.Status.exp}/{target.Status.nextEXP}",
                    ""
                ]);
                target.Status.LevelUp();
                Console.WriteLine();
                UiHelper.WaitForInput("[Enter]를 눌러 계속");
                return;
            }

            if (playerDied)
            {
                Console.WriteLine("전투에서 패배했다..");
                Console.WriteLine("눈앞이 깜깜해졌다.");
                Thread.Sleep(2000);

                MyStatus.Hp = Math.Max(1, MyStatus.maxHp / 2);
                MyStatus.mp = MyStatus.maxMp;
                IsDead = false;

                Tavern tavern = new Tavern();
                tavern.Start();
            }
        }
        #endregion

        #region 아이템 관련 추가
        [JsonProperty]
        List<ItemSlot> items = new List<ItemSlot>();

        [JsonProperty]
        Dictionary<ItemType, string> itemNames = new Dictionary<ItemType, string>()
            {
                {ItemType.F_POTION_HP, "회복 물약" },
                {ItemType.T_POTION_EXPUP, "경험치 획득량 증가" },
                {ItemType.T_POTION_ATKUP, "공격력 증가" },
                {ItemType.F_ETC_RESETNAME, "닉네임 변경" }
            };

        public string GetItemName(ItemType _type)
        {
            foreach (var names in itemNames)
            {
                if (names.Key == _type)
                {
                    return names.Value;
                }
            }

            return string.Empty;
        }
        #endregion

        #region 아이템 관련 추가
        public void UseItem()
        {
            if (Inventory.Instance.ShowItems() == false)
            {
                return;
            }

            Console.Write("입력 : ");
            string input = Console.ReadLine();
            int idx = Inventory.Instance.SelectedItem(input);
            if (idx == -1)
            {
                Console.WriteLine("잘못입력하였습니다.");
                //UseItem();
                return;
            }

            Inventory.Instance.UseItem(idx);
        }

        /// <summary>
        /// 지속형 아이템 사용
        /// </summary>
        /// <param name="item"></param>
        public void AddEffect(Item item)
        {
            bool isFound = false;

            foreach (ActiveItemEffect effect in activeEffects)
            {
                if (effect.ItemType == item.ItemType)
                {
                    effect.RemainingTurn += item.ItemUsedCount;
                    effect.TotalBonus += item.ItemStatus;

                    Console.WriteLine($"{item.ItemType} 효과가 누적되었습니다! → " +
                        $"+{item.ItemStatus} / 남은 턴: {effect.RemainingTurn} " +
                        $"(소모 시점: {effect.ConsumeType})");
                    isFound = true;
                    break;
                }
            }

            if (!isFound)
            {
                ActiveItemEffect newEffect = new ActiveItemEffect(item);
                activeEffects.Add(newEffect);

                Console.WriteLine($"{item.ItemType} 효과가 새롭게 적용되었습니다! → " +
                    $"+{item.ItemStatus} / {item.ItemUsedCount}턴 간 지속 " +
                    $"(소모 시점: {newEffect.ConsumeType})");
            }
        }

        /// <summary>
        /// 전투 시작 시 사용되는 아이템
        /// </summary>
        public void OnBattleStart()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeEffects[i];
                if (effect.ConsumeType == ConsumeType.BattleStart)
                {
                    effect.RemainingTurn--;
                    Console.WriteLine($"{effect.ItemType} 효과가 1턴 차감되었습니다. → 남은 턴: {effect.RemainingTurn}");

                    if (effect.RemainingTurn <= 0)
                    {
                        Console.WriteLine($"{effect.ItemType} 효과가 종료되었습니다. (전투 시작 시 차감)");
                        activeEffects.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 매 턴 사용되는 아이템
        /// </summary>
        public void OnTurnPassed()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeEffects[i];
                if (effect.ConsumeType == ConsumeType.PerTurn)
                {
                    effect.RemainingTurn--;
                    Console.WriteLine($"{effect.ItemType} 효과가 1턴 차감되었습니다. → 남은 턴: {effect.RemainingTurn}");

                    if (effect.RemainingTurn <= 0)
                    {
                        Console.WriteLine($"{effect.ItemType} 효과가 종료되었습니다. (턴 경과로 차감)");
                        activeEffects.RemoveAt(i);
                    }
                }
            }
        }

        public int GetTotalBonus(ItemType effectType)
        {
            int total = 0;

            // 효과 리스트를 모두 확인
            foreach (ActiveItemEffect effect in activeEffects)
            {
                // 같은 종류의 효과라면
                if (effect.ItemType == effectType)
                {
                    total += effect.TotalBonus;
                }
            }

            return total;
        }

        #endregion

        #region 스킬 지속 효과

        protected struct SkillTickLog
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

        //이름과 지속 턴을 받아서 딕셔너리에 추가
        protected virtual void AddTickSkill(string skillName, int duration)
        {
            activeSkills[skillName] = duration;
            activeSkillCasters[skillName] = this;
        }

        protected virtual void EnemyAddTickSkill(string skillName, int duration)
        {
            if (target == null)
            {
                return;
            }

            target.activeSkills[skillName] = duration;
            target.activeSkillCasters[skillName] = this;
        }

        protected virtual List<SkillTickLog> TickSkillTurns()
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
                SkillTickLog? effectLog = SkillTickEffact(skillName, caster, Math.Max(remain, 0));
                if (effectLog.HasValue)
                {
                    logs.Add(effectLog.Value);
                }

                if (activeSkills.TryGetValue(skillName, out int currentRemain) && currentRemain <= 0)
                {
                    activeSkills.Remove(skillName);
                    activeSkillCasters.Remove(skillName);
                    logs.Add(new SkillTickLog(caster, this, $"{skillName} 효과가 종료되었습니다.", false));
                }
            }

            return logs;
        }

        private static void ResolveTickDeaths(IEnumerable<SkillTickLog> logs)
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
            if (activeSkillCasters.TryGetValue(skillName, out Character caster))
            {
                return caster;
            }

            return target ?? this;
        }

        #region 스킬 효과
        protected virtual SkillTickLog? SkillTickEffact(string skill, Character caster, int remainingTurns)
        {
            switch (skill)
            {
                case "파이어볼":
                    {
                        int dotDamage = 1;
                        MyStatus.Hp = Math.Max(0, MyStatus.Hp - dotDamage);
                        bool defeated = MyStatus.Hp <= 0;
                        int remain = Math.Max(remainingTurns, 0);
                        string label = $"{skill}(지속)";
                        string message = BuildActionMessage(caster, this, dotDamage, label, defeated) + $" (남은 턴: {remain})";
                        return new SkillTickLog(caster, this, message, defeated);
                    }
                default:
                    {
                        return null;
                    }
            }
        }
        #endregion

        #endregion

        #region 데미지 계산
        protected int Damage(Character target, int damage)
        {
            // A: 공격자 공격력, D: 대상 방어력, K: 고정값(데미지가 절반이 되는 지점, 예: 100 : 받는 데미지 50%)
            int A = damage;
            int D = target.Status.DEF;
            int K = 50;
            //
            int KC = this.Status.CriticalChance;
            float KD = (float)this.Status.CriticalDamage;

            if (UiHelper.GetRandomInt1To100() <= KC)
            {
                A = (int)(A * KD / 100f);
            }

            // 데미지 공식: max(1, round(A * K / (D + K)))
            int totaldamage = Math.Max(1, (int)Math.Round(A * K / (double)(D + K)));

            target.Status.Hp -= totaldamage;
            return totaldamage;
        }
        #endregion

        #region 플레이어&적 정보
        protected int[] beforHP = new int[2];

        // Character 클래스 내 ShowBattleInfo 메서드 교체
        public void ShowBattleInfo(Character target, string[] log)
        {
            Console.Clear();

            log ??= Array.Empty<string>();
            string msg0 = log.Length > 0 ? log[0] ?? string.Empty : string.Empty;
            string msg1 = log.Length > 1 ? log[1] ?? string.Empty : string.Empty;

            Character? playerChar = GameManager.Instance.player;
            Character? monsterChar = GameManager.Instance.monster;

            if (playerChar == null)
            {
                if (this is Player)
                {
                    playerChar = this;
                }
                else if (target is Player)
                {
                    playerChar = target;
                }
            }

            if (monsterChar == null)
            {
                if (this is Monster)
                {
                    monsterChar = this;
                }
                else if (target is Monster)
                {
                    monsterChar = target;
                }
            }

            Status playerStatus = playerChar?.MyStatus ?? this.MyStatus;
            Status monsterStatus = monsterChar?.MyStatus ?? (target?.MyStatus ?? this.MyStatus);

            const int width = 68;
            string border = new string('=', width);
            string divider = new string('-', width);

            static string Fit(string text, int width)
            {
                text ??= string.Empty;
                return text.Length > width ? text[..(width - 3)] + "..." : text.PadRight(width);
            }

            static string FormatStatus(string label, Status status)
            {
                string name = string.IsNullOrWhiteSpace(status.Name) ? "??" : status.Name;
                return $"{label} : {name}  Lv.{status.level}  HP {status.Hp}/{status.maxHp}";
            }

            string playerLine = FormatStatus("플레이어", playerStatus);
            string monsterLine = FormatStatus("몬스터  ", monsterStatus);
            static void PrintLog(string label, string message, int width, Func<string, int, string> formatter)
            {
                string prefix = $"{label} ▶ ";
                string indent = new string(' ', prefix.Length);

                string[] lines = string.IsNullOrEmpty(message)
                    ? Array.Empty<string>()
                    : message.Split('\n');

                if (lines.Length == 0)
                {
                    Console.WriteLine(formatter(prefix, width));
                    return;
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    string linePrefix = i == 0 ? prefix : indent;
                    string content = lines[i];
                    Console.WriteLine(formatter(linePrefix + content, width));
                }
            }

            Console.WriteLine(border);
            Console.WriteLine(Fit(playerLine, width));
            Console.WriteLine(Fit(monsterLine, width));
            Console.WriteLine(divider);
            PrintLog("플레이어", msg0, width, Fit);
            PrintLog("몬스터  ", msg1, width, Fit);
            Console.WriteLine(border);
            Console.WriteLine();
        }
        #endregion
    }
}
