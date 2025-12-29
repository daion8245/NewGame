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
