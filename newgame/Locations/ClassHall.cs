using newgame.Characters;
using newgame.UI;

namespace newgame.Locations
{
    internal class ClassHall : IRoom
    {
        List<ClassType> availableClasses = new List<ClassType>
        {
            ClassType.Warrior,
            ClassType.Mage,
            ClassType.Archer,
            ClassType.Thief
        };
        
        public void Start()
        {
            Menu();
        }

        public void Menu()
        {
            string currentJob = GameManager.Instance.player?.CurrentClass?.ToString() ?? "없음";
            
            ClassType selectClass = UiHelper.MessageAndSelect(["\t[전직소]", "", "전직할 직업을 선택하세요.",$"현제 직업 : {currentJob}"],
                availableClasses
                , true);
            
            var classes = GameManager.Instance.GetPlayerClasses();
            
            CharacterClassType chosen = classes[0];
            GameManager.Instance.player?.AssignClass(chosen);
            Console.WriteLine($"기본 직업 [{chosen.name}] 이(가) 적용되었습니다.");
        }
    }
    
    enum ClassType
    {
        Warrior,
        Mage,
        Archer,
        Thief
    }
}
