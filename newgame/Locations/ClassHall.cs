using newgame.Characters;
using newgame.Items;
using newgame.Systems;
using newgame.UI;

namespace newgame.Locations
{
    internal class ClassHall : IRoom
    {
        public void Start()
        {
            Console.Clear();
            Menu();
        }

        public void Menu()
        {
            int sel = UiHelper.SelectMenu([
                "전직하기",
                "퀘스트",
                "나가기"
            ]);
            switch (sel)
            {
                case 0:
                {
                    break;
                }
                case 1:
                {
                    ShowQuestBoard();
                    return;
                }
                case 2:
                {
                    GameManager.Instance.ReturnToLobby();
                    return;
                }
            }
            
            Player? player = GameManager.Instance.Player;
            if (player == null)
            {
                UiHelper.TxtOut(["플레이어 정보가 없습니다."], false);
                return;
            }

            IReadOnlyList<CharacterClassType> classes = GameManager.Instance.GetAvailablePlayerClasses();
            if (classes.Count == 0)
            {
                UiHelper.TxtOut(["전직 가능한 직업이 없습니다.", "조건을 충족해 직업을 해금하세요."], false);
                return;
            }

            string currentJob = string.IsNullOrWhiteSpace(player.MyStatus.ClassName) ? "없음" : player.MyStatus.ClassName;

            string[] message =
            {
                "\t[전직소]",
                string.Empty,
                "전직할 직업을 선택하세요.",
                $"현재 직업 : {currentJob}"
            };

            string[] options = new string[classes.Count];
            for (int i = 0; i < classes.Count; i++)
            {
                options[i] = classes[i].name;
            }

            int selectedIndex = UiHelper.MessageAndSelect(message, options, true);
            CharacterClassType selectedClass = classes[selectedIndex];

            if (string.Equals(selectedClass.name, currentJob, StringComparison.OrdinalIgnoreCase))
            {
                UiHelper.TxtOut(["\t전직 실패!", "이미 해당 직업입니다."], false);
                return;
            }

            bool changed = player.TryChangeClass(selectedClass.name);
            if (changed)
            {
                UiHelper.TxtOut(["\t전직 성공!", $"당신은 이제 {selectedClass.name}입니다!"]);
            }
            else
            {
                UiHelper.TxtOut(["\t전직 실패!", "직업 정보를 찾을 수 없거나 조건을 만족하지 못했습니다."], false);
            }
            UiHelper.WaitForInput();
        }
        
        #region 퀘스트

        void ShowQuestBoard()
        {
            QuestManager questManager = GameManager.Instance.QuestManager;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("┏━━━━━━━━━━━━━━━━━━━━━━┓");
                Console.WriteLine("┃  전직 퀘스트 게시판  ┃");
                Console.WriteLine("┗━━━━━━━━━━━━━━━━━━━━━━┛");
                Console.WriteLine();

                List<Quest> activeQuests = questManager.GetActiveQuests().ToList();
                List<Quest> readyQuests = questManager.GetReadyToClaimQuests().ToList();
                List<Quest> availableQuests = questManager.GetAvailableQuests().ToList();

                if (activeQuests.Count == 0)
                {
                    Console.WriteLine("진행 중인 퀘스트가 없습니다.");
                }
                else
                {
                    Console.WriteLine("[진행 중인 퀘스트]");
                    foreach (Quest quest in activeQuests)
                    {
                        Console.WriteLine($"- {quest.Name}: {quest.CurrentCount}/{quest.RequiredCount} (목표: {quest.TargetMobName})");
                    }
                }

                Console.WriteLine();

                if (readyQuests.Count > 0)
                {
                    Console.WriteLine("[완료 퀘스트]");
                    foreach (Quest quest in readyQuests)
                    {
                        Console.WriteLine($"- {quest.Name}: 보상 수령 대기 중");
                    }
                    Console.WriteLine();
                }

                if (availableQuests.Count > 0)
                {
                    Console.WriteLine("[신규 퀘스트]");
                    foreach (Quest quest in availableQuests)
                    {
                        Console.WriteLine($"- {quest.Name}: {quest.Description}");
                    }
                    Console.WriteLine();
                }

                List<string> menu = new List<string>();
                if (availableQuests.Count > 0)
                {
                    menu.Add("퀘스트 수락");
                }
                if (readyQuests.Count > 0)
                {
                    menu.Add("보상 수령");
                }
                menu.Add("돌아가기");

                int selection = UiHelper.SelectMenu(menu.ToArray());
                string selectedOption = menu[selection];

                if (selectedOption == "퀘스트 수락")
                {
                    AcceptQuest(questManager, availableQuests);
                }
                else if (selectedOption == "보상 수령")
                {
                    ClaimQuestReward(questManager, readyQuests);
                }
                else
                {
                    break;
                }
            }

            Start();
        }

        static void AcceptQuest(QuestManager questManager, List<Quest> availableQuests)
        {
            Console.Clear();
            Console.WriteLine("수락할 퀘스트를 선택하세요.");
            Console.WriteLine();

            string[] questNames = availableQuests
                .Select(q => $"{q.Name} - 목표: {q.TargetMobName} {q.RequiredCount}마리")
                .ToArray();

            int selectedIndex = UiHelper.SelectMenu(questNames);
            Quest selectedQuest = availableQuests[selectedIndex];

            Console.Clear();
            UiHelper.TxtOut([
                $"퀘스트 이름 : {selectedQuest.Name}",
                $"설명 : {selectedQuest.Description}",
                $"목표 : {selectedQuest.TargetMobName} {selectedQuest.RequiredCount}마리",
                $"보상 : 골드 {selectedQuest.RewardGold}"
            ], SlowTxtOut: false);

            foreach ((ItemType type, int count) in selectedQuest.ItemRewards)
            {
                string itemName = Inventory.Instance.GetItemName(type);
                Console.WriteLine($"추가 보상 : {itemName} x {count}");
            }

            Console.WriteLine();
            int accept = UiHelper.SelectMenu(["수락", "취소"]);
            if (accept == 0)
            {
                if (questManager.TryAcceptQuest(selectedQuest))
                {
                    UiHelper.WaitForInput();
                }
            }
        }

        static void ClaimQuestReward(QuestManager questManager, List<Quest> readyQuests)
        {
            Console.Clear();
            Console.WriteLine("보상을 받을 퀘스트를 선택하세요.");
            Console.WriteLine();

            string[] questNames = readyQuests
                .Select(q => q.Name)
                .ToArray();

            int selectedIndex = UiHelper.SelectMenu(questNames);
            Quest selectedQuest = readyQuests[selectedIndex];
            questManager.TryClaimReward(selectedQuest);
        }

        #endregion
    }
}
