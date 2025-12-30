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
            Console.Clear();
            Menu();
        }

        public void Menu()
        {
            string currentJob = GameManager.Instance.player?.CurrentClass?.ToString() ?? "없음";
            
            ClassType selectClass = UiHelper.MessageAndSelect(["\t[전직소]", "", "전직할 직업을 선택하세요.",$"현제 직업 : {currentJob}"],
                availableClasses
                , true);
            
            ChangeOfJob(selectClass);
        }

        private void ChangeOfJob(ClassType newClass)
        {
            switch (newClass)
            {
                case ClassType.Warrior:
                    bool? success = GameManager.Instance.player?.TryChangeClass("검사");
                    if (success == true)
                    {
                        UiHelper.TxtOut(["\t전직 성공!", "당신은 이제 검사입니다!"]);
                    }
                    else
                    {
                        UiHelper.TxtOut(["\t전직 실패!", "전직 조건을 만족하지 못했습니다."]);
                    }
                    break;
            }
            //GameManager.Instance.player?.TryChangeClass();
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
