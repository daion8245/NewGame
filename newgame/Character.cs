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

        //전투 중 발생하는 안내·로그 문구를 모든 객체가 함께 쓰는 정적 문자열 버퍼로 모아두는 변수
        private static string BattleInfoStr = "";

        protected static string[] battleLog = new string[2];

        #region 전투 진입
        Character? target;

        public void EnteringBattle(Character target)
        {
            this.target = target;
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
            battleLog[0] = "";
            battleLog[1] = "";
            int damage = Damage(target, MyStatus.ATK);
            TickSkillTurns();

            if (target.Status.Hp <= 0)
            {
                Console.WriteLine();
                Console.WriteLine($"{MyStatus.Name}의 공격! {target.Status.Name} 은 {damage} 만큼의 데미지를 받았다 {target.Status.Name} 의 남은 체력: 0");
                Thread.Sleep(1000);
                Console.WriteLine();
                target.Dead(this);
                return new string[] { " ", " " };
            }

            // 0번: 플레이어 메시지, 1번: 몬스터 메시지
            string[] messages = new string[2];
            if (this == GameManager.Instance.player)
            {
                messages[0] = $"{MyStatus.Name}의 공격! {target.Status.Name} 은 {damage} 만큼의 데미지를 받았다 {target.Status.Name} 의 남은 체력: {target.Status.Hp}";
                BattleInfoStr = messages[0];
                messages[1] = "";
            }
            else
            {
                if (BattleInfoStr != "")
                {
                    messages[0] = BattleInfoStr;
                }
                else
                {
                    messages[0] = "";
                }
                messages[1] = $"{MyStatus.Name}의 공격! {target.Status.Name}은 {damage} 만큼의 데미지를 받았다 {target.Status.Name}의 남은 체력: {target.Status.Hp}";
            }

            beforHP[0] = MyStatus.Hp;
            beforHP[1] = target.MyStatus.Hp;
            battleLog = messages;

            ShowBattleInfo(target, messages);
            return messages;
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
            int damage = Damage(target, skill.skillDamage);
            ShowBattleInfo(target, battleLog);
            TickSkillTurns();

            // ② 틱으로 죽었을 수도 있으니 즉시 체크
            if (target.Status.Hp <= 0)
            {
                target.Dead(this);
                return new string[] { " ", " " };
            }

            if (target.Status.Hp <= 0)
            {
                // 죽는 경우 메시지
                string killMsg = $"{MyStatus.Name}의 공격! {target.Status.Name}은 {damage} 만큼의 데미지를 받았다 {target.Status.Name}의 남은 체력: 0";
                Console.WriteLine(); Console.WriteLine(killMsg); Thread.Sleep(1000); Console.WriteLine();
                target.Dead(this);
                return new[] { " ", " " };
            }

            string[] messages = new string[2];
            if (this == GameManager.Instance.player)
            {
                messages[0] = $"{MyStatus.Name}의 공격! {target.Status.Name}은 {damage} 만큼의 데미지를 받았다 {target.Status.Name}의 남은 체력: {target.Status.Hp}";
                BattleInfoStr = messages[0];
                messages[1] = "";
            }
            else
            {
                messages[0] = string.IsNullOrEmpty(BattleInfoStr) ? "" : BattleInfoStr;
                messages[1] = $"{MyStatus.Name}의 공격! {target.Status.Name}은 {damage} 만큼의 데미지를 받았다 {target.Status.Name}의 남은 체력: {target.Status.Hp}";
            }
            return messages;
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
            battleLog[0] = "";
            battleLog[1] = "";

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
            // ============================================================
            // 개편 목표 (Pseudocode):
            // 1. 좌/우(공통) 레이아웃 통일
            // 2. 중복 if 블록 제거
            // 3. 고정 폭 컬럼 정렬 (Name / Lv / Hp)
            // 4. log[0], log[1] 를 우측 메시지 영역에 자연스럽게 표시
            // 5. 널/범위 안전 처리
            // ============================================================

            Console.Clear();

            // 안전 처리
            log ??= Array.Empty<string>();
            string msg0 = log.Length > 0 ? log[0] ?? "" : "";
            string msg1 = log.Length > 1 ? log[1] ?? "" : "";

            // 좌측/우측 캐릭터 결정
            // 기존 로직 유지: target 이 Monster 이면 (플레이어 vs 몬스터) 형태 출력
            bool leftIsThis = target is Monster;
            Character left = leftIsThis ? this : target;
            Character right = leftIsThis ? target : this;

            // 컬럼 폭 정의
            const int colWidth = 22;
            string Col(string text) => text.Length >= colWidth ? text[..(colWidth - 1)] + " " : text.PadRight(colWidth);

            // 1줄: 이름 + 메시지(log[0])
            // 2줄: 레벨 + 메시지(log[1])
            // 3줄: HP
            var line1 = $"{Col($"Name.{left.MyStatus.Name}")}{Col($"Name.{right.MyStatus.Name}")}{msg0}";
            var line2 = $"{Col($"Lv.{left.MyStatus.level}")}{Col($"Lv.{right.MyStatus.level}")}{msg1}";
            var line3 = $"{Col($"Hp.{left.MyStatus.Hp}")}{Col($"Hp.{right.MyStatus.Hp}")}";

            Console.WriteLine(line1);
            Console.WriteLine(line2);
            Console.WriteLine(line3);
        }
        #endregion
    }
}
