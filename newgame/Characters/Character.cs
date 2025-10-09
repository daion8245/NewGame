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

        protected readonly BattleLogService battleLogService;

        protected Character(BattleLogService battleLogService)
        {
            this.battleLogService = battleLogService ?? throw new ArgumentNullException(nameof(battleLogService));
            statusEffects = new StatusEffectTracker(
                this,
                () => target,
                (attacker, defender, damage, actionName, targetDefeated, isCritical) =>
                    this.battleLogService.BuildActionMessage(attacker, defender, damage, actionName, targetDefeated, isCritical));
        }

        #region 전투 진입
        Character? target;

        public void EnteringBattle(Character target)
        {
            this.target = target;

            if (battleLogService.IsPlayer(this))
            {
                battleLogService.ResetBattleLog();
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
                return battleLogService.SnapshotBattleLog();

            Status targetStatus = target.MyStatus;

            battleLogService.ClearBattleMessageForActor(this);

            List<SkillTickLog> tickLogs = statusEffects.TickSkillTurns();

            // 지속 피해로 사망자가 발생하면 즉시 처리 후 return
            bool anyDeath = tickLogs.Any(log => log.TargetDefeated);
            if (anyDeath)
            {
                battleLogService.ApplyTickLogs(tickLogs);
                battleLogService.ShowBattleInfo(this, target);
                StatusEffectTracker.ResolveTickDeaths(tickLogs);
                return battleLogService.SnapshotBattleLog();
            }

            // 사망자가 없을 때만 공격 데미지 계산
            (int damage, bool isCritical) = Damage(target, MyStatus.ATK);
            MyStatus.Mp = Math.Min(MyStatus.MaxMp, MyStatus.Mp + 10);
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

            StatusEffectTracker.ResolveTickDeaths(tickLogs);

            beforHP[0] = MyStatus.Hp;
            beforHP[1] = targetStatus.Hp;

            return battleLogService.SnapshotBattleLog();
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
                return battleLogService.SnapshotBattleLog();
            }

            Status targetStatus = targetCharacter.MyStatus;

            battleLogService.ClearBattleMessageForActor(this);

            List<SkillTickLog> tickLogs = statusEffects.TickSkillTurns();

            bool attackerDiedFromTick = tickLogs.Any(log => ReferenceEquals(log.Target, this) && log.TargetDefeated);
            bool preserveOpponentLog = tickLogs.Any(log => battleLogService.IsPlayer(log.Actor) != battleLogService.IsPlayer(this));

            if (attackerDiedFromTick)
            {
                battleLogService.ApplyTickLogs(tickLogs);
                battleLogService.ShowBattleInfo(this, targetCharacter);
                StatusEffectTracker.ResolveTickDeaths(tickLogs);
                return battleLogService.SnapshotBattleLog();
            }

            if(this is Player)
            {
                MyStatus.Mp -= skill.skillMana;
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

            StatusEffectTracker.ResolveTickDeaths(tickLogs);

            return battleLogService.SnapshotBattleLog();
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

            battleLogService.ResetBattleLog();

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

                GameManager.Instance.QuestManager.NotifyMonsterDeath(selfStatus.Name);

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
            }
        }
        #endregion

        #region 아이템 관련 추가
        [JsonProperty]
        List<ItemSlot> items = new List<ItemSlot>();

        [JsonProperty]
        Dictionary<ItemType, string> itemNames = new Dictionary<ItemType, string>()
            {
                {ItemType.F_POTION_LOW_HP, "하급 회복 물약" },
                {ItemType.F_POTION_MIDDLE_HP, "중급 회복 물약" },
                {ItemType.F_POTION_HIGH_HP, "상급 회복 물약" },
                {ItemType.T_POTION_EXPUP, "경험치 획득량 증가" },
                {ItemType.T_POTION_ATKUP, "공격력 증가" },
                {ItemType.F_ETC_RESETNAME, "닉네임 변경" },

                #region 제작 재료
                {ItemType.M_WOOD, "나무" }
                #endregion
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

    }
}
