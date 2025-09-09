using Newtonsoft.Json;
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

        private List<ActiveItemEffect> activeEffects = new();
        protected Dictionary<string, int> activeSkills = new Dictionary<string, int>();

        public bool IsDead = false;
        public bool isbattleRun = false;

        private static string BattleInfoStr = "";
        public virtual string[] Attack(Character target)
        {
            TickSkillTurns();
            target.Status.hp -= MyStatus.ATK;

            if (target.Status.hp <= 0)
            {
                Console.WriteLine();
                Console.WriteLine($"{MyStatus.Name}의 공격! {target.Status.Name} 은 {MyStatus.ATK} 만큼의 데미지를 받았다 {target.Status.Name} 의 남은 체력: 0");
                Thread.Sleep(1000);
                Console.WriteLine();
                target.Dead(this);
                return new string[] { " ", " " };
            }

            // 0번: 플레이어 메시지, 1번: 몬스터 메시지
            string[] messages = new string[2];
            if (this == GameManager.Instance.player)
            {
                messages[0] = $"{MyStatus.Name}의 공격! {target.Status.Name} 은 {MyStatus.ATK} 만큼의 데미지를 받았다 {target.Status.Name} 의 남은 체력: {target.Status.hp}";
                BattleInfoStr = messages[0];
                messages[1] = "";
            }
            else
            {
                if(BattleInfoStr != "")
                {
                    messages[0] = BattleInfoStr;
                }
                else
                {
                    messages[0] = "";
                }
                messages[1] = $"{MyStatus.Name}의 공격! {target.Status.Name} 은 {MyStatus.ATK} 만큼의 데미지를 받았다 {target.Status.Name} 의 남은 체력: {target.Status.hp}";
            }
            return messages;
        }

        public virtual string[] UseAttackSkill(Character target , int skillNum)
        {
            target.Status.hp -= MyStatus.ATK;
            TickSkillTurns();

            if (target.Status.hp <= 0)
            {
                Console.WriteLine();
                Console.WriteLine($"{MyStatus.Name}의 공격! {target.Status.Name} 은 {MyStatus.ATK} 만큼의 데미지를 받았다 {target.Status.Name} 의 남은 체력: 0");
                Thread.Sleep(1000);
                Console.WriteLine();
                target.Dead(this);
                return new string[] { " ", " " };
            }

            // 0번: 플레이어 메시지, 1번: 몬스터 메시지
            string[] messages = new string[2];
            if (this == GameManager.Instance.player)
            {
                messages[0] = $"{MyStatus.Name}의 공격! {target.Status.Name} 은 {MyStatus.ATK} 만큼의 데미지를 받았다 {target.Status.Name} 의 남은 체력: {target.Status.hp}";
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
                messages[1] = $"{MyStatus.Name}의 공격! {target.Status.Name} 은 {MyStatus.ATK} 만큼의 데미지를 받았다 {target.Status.Name} 의 남은 체력: {target.Status.hp}";
            }
            
            return messages;
        }
        public virtual void Dead(Character target)
        {
            IsDead = true;

            if (target == GameManager.Instance.player)
            {
                UiHelper.TxtOut([$"{Status.Name}은 쓰러졌다!", $"{Status.Name} 에게서 승리했다!", $"+{Status.exp}Exp , +{Status.gold}골드 를 획득했다!",$"다음 레벨까지 : {target.Status.exp}/{target.Status.nextEXP}", ""]);
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
        }

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
        protected void AddTickSkill(string skillName, int duration)
        {
            activeSkills[skillName] = duration;
        }

        // 계획(의사코드):
        // 1) TickSkillTurns:
        //    - activeSkills 딕셔너리의 키 목록을 복사해서 안전하게 순회한다.
        //    - 각 스킬에 대해 먼저 남은 턴을 1 감소시킨다.
        //    - 감소 후 현재 남은 턴을 출력한다.
        //    - 해당 스킬에 대해 매턴 발동되는 효과를 처리하기 위해 SkillTickEffact(skillName)를 호출한다.
        //    - 만약 감소 후 남은 턴이 0 이하이면 딕셔너리에서 해당 스킬을 제거하고 종료 메시지를 출력한다.
        //    - 루프 중 딕셔너리를 직접 수정하므로 복사한 키 목록으로만 순회한다.
        //
        // 2) SkillTickEffact(skillName):
        //    - 전달된 스킬 이름에 따라 매턴 발동되는 효과를 분기한다.
        //    - 현재 구현에서는 부작용을 최소화하기 위해 로그를 남기고,
        //      필요하면 여기에 실제 상태 변경(예: HP 감소, 버프 적용)을 추가할 수 있다.
        //    - 예: "파이어볼"의 경우 매 턴마다 화상 데미지/로그를 남김.
        //
        // 주의:
        // - 스킬 효과가 상태를 변경할 경우(예: ATK 증감) 중복 적용을 피하기 위한 별도 플래그나 저장 구조가 필요하다.
        // - 현재 activeSkills는 턴 수만 관리하므로 추가적인 수치/데이터는 별도 구조로 관리해야 안전하다.

        protected void TickSkillTurns()
        {
            var keys = new List<string>(activeSkills.Keys);

            foreach (var skillName in keys)
            {
                if (!activeSkills.ContainsKey(skillName))
                    continue;

                activeSkills[skillName] = activeSkills[skillName] - 1;
                Console.WriteLine($"{skillName} 의 지속 턴이 1 감소했습니다. → 남은 턴: {activeSkills[skillName]}");

                SkillTickEffact(skillName);

                if (activeSkills[skillName] <= 0)
                {
                    activeSkills.Remove(skillName);
                    Console.WriteLine($"{skillName} 효과가 종료되었습니다.");
                }
            }
        }

        #region 스킬 효과
        protected void SkillTickEffact(string skill)
        {
            switch (skill)
            {
                case "파이어볼":
                    {
                        Console.WriteLine($"{MyStatus.Name}의 {skill} 효과가 발동했다! (화염의 불씨가 타오른다.)");
                        break;
                    }
                default:
                    {
                        // 알려지지 않은 스킬은 로그만 남긴다.
                        Console.WriteLine($"{skill} 에 대해 처리할 매턴 효과가 없습니다.");
                        break;
                    }
            }
        }
        #endregion

        #endregion
    }
}
