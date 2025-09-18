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
        //현제 지속되고 있는 스킬의 지속 효과
        protected Dictionary<string, int> activeSkills = new Dictionary<string, int>();
        protected Dictionary<string, Character?> activeSkillCasters = new();

        //플레이어가 죽었는지
        public bool IsDead = false;
        //플레이어가 전투에서 도망쳤는지
        public bool isbattleRun = false;
        //이전 HP 값을 저장하는 배열 (플레이어[0], 적[1])
        protected int[] beforHP = new int[2];

        private readonly BattleLogService battleLogService;

        protected Character()
            : this(GameManager.Instance.BattleLogService)
        {
        }

        protected Character(BattleLogService battleLogService)
        {
            this.battleLogService = battleLogService ?? throw new ArgumentNullException(nameof(battleLogService));
        }

        protected BattleLogService BattleLogService => battleLogService;

        #region 로그 관련
        protected string[] SnapshotBattleLog()
        {
            return battleLogService.SnapshotBattleLog();
        }

        protected void ResetBattleLog()
        {
            battleLogService.ResetBattleLog();
        }

        #endregion

        #region 전투 진입
        Character? target;

        public void EnteringBattle(Character target)
        {
            this.target = target;

            if (battleLogService.IsPlayer(this))
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

            battleLogService.ClearBattleMessageForActor(this);

            List<SkillTickLog> tickLogs = TickSkillTurns();

            // 지속 피해로 사망자가 발생하면 즉시 처리 후 return
            bool anyDeath = tickLogs.Any(log => log.TargetDefeated);
            if (anyDeath)
            {
                battleLogService.ApplyTickLogs(tickLogs);
                battleLogService.ShowBattleInfo(this, target);
                ResolveTickDeaths(tickLogs);
                return SnapshotBattleLog();
            }

            // 사망자가 없을 때만 공격 데미지 계산
            (int damage, bool isCritical) = Damage(target, MyStatus.ATK);
            MyStatus.mp = Math.Min(MyStatus.maxMp, MyStatus.mp + 10);
            bool defeated = targetStatus.Hp <= 0;
            string message = battleLogService.BuildActionMessage(this, target, damage, null, defeated, isCritical);

            bool clearOpponent = battleLogService.IsPlayer(this);
            battleLogService.UpdateBattleMessage(this, message, clearOpponent);

            battleLogService.ApplyTickLogs(tickLogs);

            battleLogService.ShowBattleInfo(this, target);

            if (defeated)
            {
                Thread.Sleep(1000);
                target.Dead(this);
            }

            ResolveTickDeaths(tickLogs);

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

            battleLogService.ClearBattleMessageForActor(this);

            List<SkillTickLog> tickLogs = TickSkillTurns();

            bool attackerDiedFromTick = tickLogs.Any(log => ReferenceEquals(log.Target, this) && log.TargetDefeated);
            bool preserveOpponentLog = tickLogs.Any(log => battleLogService.IsPlayer(log.Actor) != battleLogService.IsPlayer(this));

            if (attackerDiedFromTick)
            {
                battleLogService.ApplyTickLogs(tickLogs);
                battleLogService.ShowBattleInfo(this, targetCharacter);
                ResolveTickDeaths(tickLogs);
                return SnapshotBattleLog();
            }

            if(this is Player)
            {
                MyStatus.mp -= skill.skillMana;
            }

            (int damage, bool isCritical) = Damage(targetCharacter, skill.skillDamage);

            bool defeated = targetStatus.Hp <= 0;
            string message = battleLogService.BuildActionMessage(this, targetCharacter, damage, skill.name, defeated, isCritical);

            bool clearOpponent = battleLogService.IsPlayer(this) && !preserveOpponentLog;
            battleLogService.UpdateBattleMessage(this, message, clearOpponent);

            battleLogService.ApplyTickLogs(tickLogs);

            battleLogService.ShowBattleInfo(this, targetCharacter);

            if (defeated)
            {
                Thread.Sleep(1000);
                targetCharacter.Dead(this);
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

        #region 스킬 지속 효과

        protected internal struct SkillTickLog
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

        /// <summary>
        /// 현재 캐릭터에게 적용 중인 지속 스킬(버프/디버프)의 표시용 라벨을 생성해 반환합니다.
        /// 각 효과는 [효과명] 형태로 이어붙여 표시되며, 활성 효과가 없으면 빈 문자열을 반환합니다.
        /// </summary>
        /// <returns>활성화된 지속 스킬 효과의 표시 문자열. 없으면 빈 문자열.</returns>
        public string GetActiveSkillEffectDisplay()
        {
            if (activeSkills.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();

            foreach (var skillName in activeSkills.Keys)
            {
                string effectName = GetSkillEffectLabel(skillName);
                if (string.IsNullOrWhiteSpace(effectName))
                {
                    continue;
                }

                labels.Add($"[{effectName}]");
            }

            if (labels.Count == 0)
            {
                return string.Empty;
            }

            // 공백 없이 붙여 출력: [화상][중독] 형태
            return $"{string.Join(string.Empty, labels)}";
        }

        protected virtual string GetSkillEffectLabel(string skillName)
        {
            return skillName switch
            {
                "파이어볼" => "화상",
                _ => skillName
            };
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

                // 현재 턴 소모
                remain -= 1;
                activeSkills[skillName] = remain;

                // 시전자 추적(버프/디버프 출처 표시용)
                Character caster = GetSkillCaster(skillName);

                // 틱 효과 적용(로그 생성). remainingTurns는 사용자 표시용이므로 0 미만 방지
                SkillTickLog? effectLog = SkillTickEffact(skillName, caster, Math.Max(remain, 0));
                if (effectLog.HasValue)
                {
                    logs.Add(effectLog.Value);
                }

                // 만료 처리(0 이하): 효과 제거 및 종료 로그
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
            if (activeSkillCasters.TryGetValue(skillName, out Character? caster) && caster != null)
            {
                return caster;
            }

            return target ?? this;
        }

        #region 스킬 효과
        // 스킬별 틱 효과를 적용한다.
        // - 예시: "파이어볼" → 1 고정 화상 피해, 남은 턴 수를 메시지에 표기
        // - 반환 로그에는 (액터, 타겟, 메시지, 대상 사망 여부, 상대 로그 초기화 여부) 포함
        protected virtual SkillTickLog? SkillTickEffact(string skill, Character caster, int remainingTurns)
        {
            switch (skill)
            {
                case "파이어볼":
                    {
                        Character? targetCharacter = target;
                        int referenceHp = targetCharacter?.HasStatus == true ? targetCharacter.MyStatus.Hp : MyStatus.Hp;
                        int dotDamage = 1 + (referenceHp / 20); // 화상 고정 피해(게임 밸런스에 따라 조정 가능)
                        MyStatus.Hp = Math.Max(0, MyStatus.Hp - dotDamage);
                        bool defeated = MyStatus.Hp <= 0;
                        int remain = Math.Max(remainingTurns, 0); // 표시용 잔여 턴(음수 방지)
                        string label = $"{skill}(지속)";
                        string message = battleLogService.BuildActionMessage(caster, this, dotDamage, label, defeated) + $" (남은 턴: {remain})";
                        return new SkillTickLog(caster, this, message, defeated);
                    }
                default:
                    {
                        // 미구현/표시 불필요한 스킬은 로그 없이 null
                        return null;
                    }
            }
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

        #endregion
    }
}
