using newgame.Characters;
using newgame.Items;
using newgame.UI;

namespace newgame.Locations.DungeonRooms;
using System;
using static newgame.UI.UiHelper;
using newgame.Characters;
using newgame.Enemies;
using newgame.Services;
using newgame.Systems;
using newgame.UI;

public class TreasureRooms 
{
    
    public void Start()
    {
        ShowMenu();
    }

    private void ShowMenu()
    {
        Console.Clear();
        TxtOut(["\t[보물방]", "눈앞에 반짝이는 보물상자가 있습니다.",""]);
        WaitForInput("[Enter]를 눌러 상자 열기.");
        OpenTreasureBox();
    }

    private void OpenTreasureBox()
    {
        var player = GameManager.Instance.Player;
        ItemType itemtype = RandomItemTypeGenarator();
        string itemname = RandomItemGenarator(itemtype);
        
        Console.Clear();
        for (int i=0; i<30; i++)
        {
            Console.Write("[]");
        }
    }

    private void ItemGenarator(string itemname, ItemType itemtype)
    {
        
    }

    private string RandomItemGenarator(ItemType itemtype)
    {
        switch (itemtype)
        {
            case ItemType.Gold:
            {
                Random random = new Random();
                int goldAmount = random.Next(1, 400);
                return goldAmount.ToString();
            }
            case ItemType.Potion:
            {
                Random random = new Random();
                string[] potions = {"F_POTION_LOW_HP", "F_POTION_MIDDLE_HP", "F_POTION_HIGH_HP"};
                int index = random.Next(potions.Length);
                return potions[index];
            }
            case ItemType.Equipment:
                return "Equipment";
            case ItemType.Weapon:
                return "Weapon";
            case ItemType.SkillBook:
                return "SkillBook";
            case ItemType.Stat:
                return "Stat";
            default:
                return "Unknown Item";
        }
    }

    private ItemType RandomItemTypeGenarator()
    {
        Random rand = new Random();
        Array values = Enum.GetValues(typeof(ItemType));
        ItemType randomItem = (ItemType)values.GetValue(rand.Next(values.Length))!;
        return randomItem;
    }

    enum ItemType
    {
        Gold,
        Potion,
        Equipment,
        Weapon,
        SkillBook,
        Stat
    }
}