using newgame.Characters;
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
        }
        
    }
}
