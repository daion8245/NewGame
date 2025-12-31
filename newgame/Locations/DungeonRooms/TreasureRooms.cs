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
        ItemGenarator(itemname, itemtype);
        
        Console.Clear();
        for (int i=0; i<30; i++)
        {
            Console.Write("[]");
        }
    }

    private void ItemGenarator(string itemname, ItemType itemtype)
    {
        var player = GameManager.Instance.Player;
        switch (itemtype)
        {
            case ItemType.Gold:
            {
                player.MyStatus.gold += int.Parse(itemname);
                TxtOut(["보물상자에서 골드 "+itemname+"을(를) 획득했다!"]);
                break;
            }
            case ItemType.Potion:
            {
                break;
            }
            case ItemType.Equipment:
            {
                break;
            }
            case ItemType.Weapon:
            {
                break;
            }
            case ItemType.SkillBook:
            {
                break;
            }
            case ItemType.Stat:
            {
                break;
            }
        }
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
            {
                Random random = new Random();
                //S, G, H, P, C,
                string[] equipments = {"S_11", "G_9", "H_8", "P_13", "C_7"};
                int index = random.Next(equipments.Length);
                return equipments[index];
            }
            case ItemType.Weapon:
                Random rand = new Random();
                int[] weapons = {1,3,6,8,9,15 };
                int weaponIndex = rand.Next(weapons.Length);
                return weapons[weaponIndex].ToString();
            case ItemType.SkillBook:
                Random rnd = new Random();
                string[] skillbooks = {"파이어볼","아쿠아 볼","마구치기"};
                int skillIndex = rnd.Next(skillbooks.Length);
                return skillbooks[skillIndex];
            case ItemType.Stat:
                Random randomStat = new Random();
                string[] stat = { "str", "def", "hp", "mp" };
                return stat[randomStat.Next(stat.Length)];
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