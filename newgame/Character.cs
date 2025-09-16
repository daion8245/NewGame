using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data;

namespace newgame
{
    //능력치를 가지고 있는 모든 캐릭터의 부모 클래스
    internal class Character
    {
        Status Status;

        public Status MyStatus
        {
            get => Status;
            protected set => Status = value;
        }

        //현제 활성화 되어있는 포션 효과
        private List<ActiveItemEffect> activeEffects = new();
        //현제 지속되고 있는 스킬의 지속 효과
        protected Dictionary<string, int> activeSkills = new Dictionary<string, int>();

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

        private void UpdateBattleMessage(Character attacker, string message, bool clearOpponentMessage)
        {
            message ??= string.Empty;

            if (IsPlayer(attacker))
            {
                battleLog[0] = message;
                if (clearOpponentMessage)
                {
                    battleLog[1] = string.Empty;
                }
            }
            else
            {
                battleLog[1] = message;
                if (clearOpponentMessage)
                {
                    battleLog[0] = string.Empty;
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
            {
                return SnapshotBattleLog();
            }

            int damage = Damage(target, MyStatus.ATK);
            TickSkillTurns();

            bool defeated = target.Status.Hp <= 0;
            string message = BuildActionMessage(this, target, damage, null, defeated);

            bool clearOpponent = IsPlayer(this);
            UpdateBattleMessage(this, message, clearOpponent);

            ShowBattleInfo(target, battleLog);

            if (defeated)
            {
                Thread.Sleep(1000);
                target.Dead(this); 
                // 여기서 Dead를 호출한 후에도 함수는 계속 진행됨
            }
            
            beforHP[0] = MyStatus.Hp;
            beforHP[1] = target.MyStatus.Hp;
            
            return SnapshotBattleLog(); // 함수가 정상 종료됨
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

            int damage = Damage(target, skill.skillDamage);
            TickSkillTurns();

            bool defeated = target.Status.Hp <= 0;
            string message = BuildActionMessage(this, target, damage, skill.name, defeated);

            bool clearOpponent = IsPlayer(this);
            UpdateBattleMessage(this, message, clearOpponent);

            ShowBattleInfo(target, battleLog);

            if (defeated)
            {
                Thread.Sleep(1000);
                target.Dead(this);
            }

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
            if (IsDead == true) { return; }

            IsDead = true;
            activeSkills.Clear();
            target.activeSkills.Clear();
            ResetBattleLog();

            if (target == GameManager.Instance.player)
            {
                UiHelper.TxtOut([$"{Status.Name}은 쓰러졌다!", $"{Status.Name}에게서 승리했다!", $"+{Status.exp}Exp , +{Status.gold}골드 를 획득했다!", $"다음 레벨까지 : {target.Status.exp}/{target.Status.nextEXP}", ""]);
                target.Status.LevelUp();
                Console.WriteLine();
                UiHelper.WaitForInput("[Enter]를 눌러 계속");
            }
            else
            {
                Console.WriteLine("전투에서 패배했다..");
                Console.WriteLine("눈앞이 깜깜해졌다.");
                Thread.Sleep(2000);
                Tavern tavern = new Tavern();
                tavern.Start();
            }

            return;
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

        //이름과 지속 턴을 받아서 딕셔너리에 추가
        protected virtual void AddTickSkill(string skillName, int duration)
        {
            this.activeSkills[skillName] = duration;
        }

        protected virtual void EnemyAddTickSkill(string skillName, int duration)
        {
            target.activeSkills[skillName] = duration;
        }

        protected virtual void TickSkillTurns()
        {
            var keys = new List<string>(activeSkills.Keys);

            foreach (var skillName in keys)
            {
                if (!activeSkills.ContainsKey(skillName))
                    continue;

                Console.WriteLine();
                activeSkills[skillName] = activeSkills[skillName] - 1;
                Console.WriteLine($"{skillName} 의 지속 턴이 1 감소했습니다. → 남은 턴: {activeSkills[skillName]}");

                SkillTickEffact(skillName);

                if (activeSkills.ContainsKey(skillName) && activeSkills[skillName] <= 0)
                {
                    activeSkills.Remove(skillName);
                    Console.WriteLine($"{skillName} 효과가 종료되었습니다.");
                }
            }
        }

        #region 스킬 효과
        protected virtual void SkillTickEffact(string skill)
        {
            Console.WriteLine();
            switch (skill)
            {
                case "파이어볼":
                    Console.WriteLine($"{MyStatus.Name} 은/는 화상 데미지를 입었다! (체력 -1)");
                    MyStatus.Hp--;
                    break;
                default:
                    Console.WriteLine($"{skill} 에 대해 처리할 매턴 효과가 없습니다.");
                    break;
            }

            if (MyStatus.Hp <= 0)
            {
                Dead(target);
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
            string playerMsg = $"플레이어 ▶ {msg0}";
            string monsterMsg = $"몬스터   ▶ {msg1}";

            Console.WriteLine(border);
            Console.WriteLine(Fit(playerLine, width));
            Console.WriteLine(Fit(monsterLine, width));
            Console.WriteLine(divider);
            Console.WriteLine(Fit(playerMsg, width));
            Console.WriteLine(Fit(monsterMsg, width));
            Console.WriteLine(border);
            Console.WriteLine();
        }
        #endregion
    }
}
