using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace newgame
{
    //능력치를 가지고 있는 모든 캐릭터의 부모 클래스
    internal class Character
    {
        Status? Status;

        public bool HasStatus => Status != null;

        public Status MyStatus
        {
            get => Status ?? throw new InvalidOperationException("Status has not been initialized.");
            protected set => Status = value ?? throw new ArgumentNullException(nameof(value));
        }

        //현제 활성화 되어있는 포션 효과
        private List<ActiveItemEffect> activeEffects = new();
        private readonly StatusEffectTracker statusEffects;

        public StatusEffectTracker StatusEffects => statusEffects;

        //플레이어가 죽었는지
        public bool IsDead = false;
        //플레이어가 전투에서 도망쳤는지
        public bool isbattleRun = false;
        //이전 HP 값을 저장하는 배열 (플레이어[0], 적[1])
        protected int[] beforHP = new int[2];

        protected static readonly string[] battleLog = new string[2];

        protected Character()
        {
            statusEffects = new StatusEffectTracker(
                this,
                () => target,
                (attacker, defender, damage, actionName, targetDefeated, isCritical) =>
                    BuildActionMessage(attacker, defender, damage, actionName, targetDefeated, isCritical));
        }

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
            Player? activePlayer = GameManager.Instance.player;
            return activePlayer != null && ReferenceEquals(actor, activePlayer);
        }

        // 로그 한 줄을 표준 형식으로 만들어 반환한다.
        // - isFirstLine이 true면 "->" 접두사, 아니면 " ->" 접두사를 사용한다.
        // - 공백/널 입력은 빈 문자열로 정규화한다(빈 로그는 출력 안 함).
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

        // 공격자(플레이어/몬스터)에 따라 로그 슬롯을 선택해(플레이어: 0, 몬스터: 1)
        // 해당 슬롯의 메시지를 덮어쓴다.
        // clearOpponentMessage가 true면 상대 슬롯 로그는 비운다(턴 전환 느낌을 주는 효과).
        private void UpdateBattleMessage(Character attacker, string message, bool clearOpponentMessage)
        {
            bool isPlayer = IsPlayer(attacker);
            int index = isPlayer ? 0 : 1;

            string formatted = FormatLogLine(message, true); // 항상 첫 줄로 기록
            battleLog[index] = formatted;

            if (clearOpponentMessage)
            {
                battleLog[isPlayer ? 1 : 0] = string.Empty;
            }
        }

        // 기존 슬롯의 마지막 줄 뒤에 추가 메시지를 이어붙인다.
        // - 해당 액터의 첫 줄이면 "->", 아니면 " ->"로 들여쓰기 느낌을 준다.
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

        // 인자로 전달된 액터가 어느 쪽인지 판별하여 해당 측 로그만 비운다.
        // - 플레이어면 0, 몬스터면 1
        private static void ClearBattleMessageForActor(Character actor)
        {
            if (actor == null)
            {
                return;
            }

            Player? activePlayer = GameManager.Instance.player;
            Monster? activeMonster = GameManager.Instance.monster;

            if (activePlayer != null && ReferenceEquals(actor, activePlayer))
            {
                battleLog[0] = string.Empty;
            }
            else if (activeMonster != null && ReferenceEquals(actor, activeMonster))
            {
                battleLog[1] = string.Empty;
            }
        }

        // 인자로 전달된 액터의 '상대' 로그를 비운다.
        // - 플레이어면 몬스터 로그(1), 몬스터면 플레이어 로그(0)
        private static void ClearBattleMessageForOpponent(Character actor)
        {
            if (actor == null)
            {
                return;
            }

            Player? activePlayer = GameManager.Instance.player;
            Monster? activeMonster = GameManager.Instance.monster;

            if (activePlayer != null && ReferenceEquals(actor, activePlayer))
            {
                battleLog[1] = string.Empty;
            }
            else if (activeMonster != null && ReferenceEquals(actor, activeMonster))
            {
                battleLog[0] = string.Empty;
            }
        }

        // 틱 로그 집합을 실제 battleLog에 반영한다.
        // - 각 로그는 액터 기준으로 자신의 슬롯에 누적된다.
        // - ClearOpponent 플래그가 있으면 상대 슬롯은 비운다(상태 이상에 의한 간섭 표현).
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

        // 한 액션(공격/스킬) 메시지를 표준 문장으로 구성한다.
        // - actionName이 null/공백이면 "공격"으로 치환
        // - isCritical이면 접두에 [치명타!] 부착
        // - 대상 체력 및 쓰러짐 여부 표기
        private static string BuildActionMessage(Character attacker, Character defender, int damage, string? actionName, bool targetDefeated, bool isCritical = false)
        {
            string label = string.IsNullOrWhiteSpace(actionName) ? "공격" : actionName!;
            string prefix = $"{attacker.MyStatus.Name}의 {label}!";

            if (isCritical)
            {
                prefix = $"[치명타!] {prefix}";
            }
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

            Status targetStatus = target.MyStatus;

            ClearBattleMessageForActor(this);

            List<SkillTickLog> tickLogs = statusEffects.TickSkillTurns();

            // 지속 피해로 사망자가 발생하면 즉시 처리 후 return
            bool anyDeath = tickLogs.Any(log => log.TargetDefeated);
            if (anyDeath)
            {
                ApplyTickLogs(tickLogs);
                ShowBattleInfo(target, battleLog);
                StatusEffectTracker.ResolveTickDeaths(tickLogs);
                return SnapshotBattleLog();
            }

            // 사망자가 없을 때만 공격 데미지 계산
            (int damage, bool isCritical) = Damage(target, MyStatus.ATK);
            MyStatus.mp = Math.Min(MyStatus.maxMp, MyStatus.mp + 10);
            bool defeated = targetStatus.Hp <= 0;
            string message = BuildActionMessage(this, target, damage, null, defeated, isCritical);

            bool clearOpponent = IsPlayer(this);
            UpdateBattleMessage(this, message, clearOpponent);

            ApplyTickLogs(tickLogs);

            ShowBattleInfo(target, battleLog);

            if (defeated)
            {
                Thread.Sleep(1000);
                target.Dead(this);
            }

            StatusEffectTracker.ResolveTickDeaths(tickLogs);

            beforHP[0] = MyStatus.Hp;
            beforHP[1] = targetStatus.Hp;

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
            Character? targetCharacter = target;
            if (targetCharacter == null || string.IsNullOrWhiteSpace(skill.name))
            {
                return SnapshotBattleLog();
            }

            Status targetStatus = targetCharacter.MyStatus;

            ClearBattleMessageForActor(this);

            List<SkillTickLog> tickLogs = statusEffects.TickSkillTurns();

            bool attackerDiedFromTick = tickLogs.Any(log => ReferenceEquals(log.Target, this) && log.TargetDefeated);
            bool preserveOpponentLog = tickLogs.Any(log => IsPlayer(log.Actor) != IsPlayer(this));

            if (attackerDiedFromTick)
            {
                ApplyTickLogs(tickLogs);
                ShowBattleInfo(targetCharacter, battleLog);
                StatusEffectTracker.ResolveTickDeaths(tickLogs);
                return SnapshotBattleLog();
            }

            if(this is Player)
            {
                MyStatus.mp -= skill.skillMana;
            }

            (int damage, bool isCritical) = Damage(targetCharacter, skill.skillDamage);

            bool defeated = targetStatus.Hp <= 0;
            string message = BuildActionMessage(this, targetCharacter, damage, skill.name, defeated, isCritical);

            bool clearOpponent = IsPlayer(this) && !preserveOpponentLog;
            UpdateBattleMessage(this, message, clearOpponent);

            ApplyTickLogs(tickLogs);

            ShowBattleInfo(targetCharacter, battleLog);

            if (defeated)
            {
                Thread.Sleep(1000);
                targetCharacter.Dead(this);
            }

            StatusEffectTracker.ResolveTickDeaths(tickLogs);

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
            statusEffects.Clear();

            target?.StatusEffects.Clear();

            ResetBattleLog();

            Player? activePlayer = GameManager.Instance.player;
            bool killedByPlayer = activePlayer != null && ReferenceEquals(target, activePlayer);
            bool playerDied = activePlayer != null && ReferenceEquals(this, activePlayer);

            Status selfStatus = MyStatus;
            Status? targetStatus = target?.HasStatus == true ? target.MyStatus : null;

            if (killedByPlayer && !playerDied)
            {
                if (targetStatus == null)
                {
                    throw new InvalidOperationException("Target status is not initialized.");
                }

                UiHelper.TxtOut([
                    $"{selfStatus.Name}은 쓰러졌다!",
                    $"{selfStatus.Name}에게서 승리했다!",
                    $"+{selfStatus.exp}Exp , +{selfStatus.gold}골드 를 획득했다!",
                    $"다음 레벨까지 : {targetStatus.exp}/{targetStatus.nextEXP}",
                    ""
                ]);
                targetStatus.LevelUp();
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
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("입력이 비어 있습니다.");
                return;
            }

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

        #region 데미지 계산
        // 데미지 공식:
        // - 기본 공격력 A, 대상 방어력 D, 고정 상수 K(완화 기준점)
        // - 치명타 판정(확률 KC[%]) → 피해를 KD[%] 배율로 증폭
        // - 최종 피해 = max(1, round(A * K / (D + K)))
        //   방어력이 높을수록 분모가 커져 피해가 점진적으로 감소(완만한 방어 효율 곡선)
        protected (int Damage, bool IsCritical) Damage(Character target, int damage)
        {
            Status myStatus = MyStatus;
            Status targetStatus = target.MyStatus;

            int A = damage;
            int D = targetStatus.DEF;
            int K = 50; // D ≈ K일 때 절반 수준으로 완화되는 기준점

            int KC = myStatus.CriticalChance;  // 치명타 확률(%)
            float KD = (float)myStatus.CriticalDamage; // 치명타 피해(%)
            bool isCritical = false;

            // 치명타 판정 선행 → 증폭된 공격력으로 최종 방어 공식 적용
            if (UiHelper.GetRandomInt1To100() <= KC)
            {
                A = (int)(A * KD / 100f);
                isCritical = true;
            }

            // 방어 기반 피해 완화 공식
            int totaldamage = Math.Max(1, (int)Math.Round(A * K / (double)(D + K)));

            targetStatus.Hp -= totaldamage;
            return (totaldamage, isCritical);
        }
        #endregion

        #region 플레이어&적 정보
        // 전투 UI 전체를 그린다.
        // - 플레이어/몬스터 객체를 안전하게 추론(현재 this/target 기반 대체 포함)
        // - 상태 라벨에 버프/디버프 표시 태그 포함([화상] 등)
        // - 로그는 "플레이어 ▶", "몬스터  ▶" 접두로 첫 줄, 이후 줄은 동일 들여쓰기 유지
        public void ShowBattleInfo(Character target, string[] log)
        {
            Console.Clear();

            log ??= Array.Empty<string>();
            string msg0 = log.Length > 0 ? log[0] ?? string.Empty : string.Empty;
            string msg1 = log.Length > 1 ? log[1] ?? string.Empty : string.Empty;

            Character? playerChar = GameManager.Instance.player;
            Character? monsterChar = GameManager.Instance.monster;

            // 안전한 널 대체: 싱글플레이/테스트 상황에서 GM에 미등록일 수 있음
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

            const int width = 68; // 한 줄 폭(고정 레이아웃)
            string border = new string('=', width);
            string divider = new string('-', width);

            // 폭에 맞춰 문자열을 채운다.
            // - 길이가 길어도 자르지 않고 우측 패딩만 넣는다(레이아웃 깨짐 방지).
            static string Fit(string text, int width)
            {
                text ??= string.Empty;
                // 너무 길 때 자르는 기능은 주석 처리되어 있음(디자인 선택)
                return text.Length > width ? text.PadRight(width)/*text[..(width - 3)] + "..."*/ : text.PadRight(width);
            }

            /// <summary>
            /// 한 줄 상태 라벨 구성
            /// - 이름 미설정 시 "??" 대체
            /// - StatusEffects.GetActiveSkillEffectDisplay()로 [효과] 태그를 이름 옆에 붙인다.
            /// </summary>
            static string FormatStatus(string label, Character? character, Status status)
            {
                string name = string.IsNullOrWhiteSpace(status.Name) ? "??" : status.Name;
                string effectLabel = character?.StatusEffects.GetActiveSkillEffectDisplay() ?? string.Empty;
                return $"{label} : {name}{effectLabel}  Lv.{status.level}  HP {status.Hp}/{status.maxHp}";
            }

            string playerLine = FormatStatus("플레이어", playerChar, playerStatus);
            string monsterLine = FormatStatus("몬스터  ", monsterChar, monsterStatus);

            // 로그 출력:
            // - 첫 줄은 "라벨 ▶ " 접두사를 붙이고, 이후 줄은 같은 길이만큼 공백으로 들여쓰기
            // - 비어있으면 접두사만 출력하여 영역 유지
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
