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
        var player = GameManager.Instance.RequirePlayer();
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
                if (Enum.TryParse<newgame.Items.ItemType>(itemname, out var potionType))
                {
                    if (Inventory.Instance.AddItem(potionType))
                    {
                        string potionName = Inventory.Instance.GetItemName(potionType);
                        TxtOut([$"보물상자에서 {potionName}을(를) 획득했다!"]);
                    }
                }
                else
                {
                    TxtOut([$"알 수 없는 포션입니다: {itemname}"]);
                }
                break;
            }
            case ItemType.Equipment:
            {
                string[] parts = itemname.Split('_', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2 || !int.TryParse(parts[1], out int equipId))
                {
                    TxtOut(["잘못된 장비 데이터입니다."]);
                    break;
                }

                EquipType equipType = char.ToUpperInvariant(parts[0][0]) switch
                {
                    'S' => EquipType.SHOES,
                    'G' => EquipType.GLOVE,
                    'H' => EquipType.HELMET,
                    'P' => EquipType.PANTS,
                    'C' => EquipType.SHIRT,
                    _ => EquipType.NONE
                };

                if (equipType == EquipType.NONE)
                {
                    TxtOut([$"알 수 없는 장비 타입입니다: {itemname}"]);
                    break;
                }

                Equipment? equip = GameManager.Instance.FindEquipment(equipType, equipId);
                if (equip != null)
                {
                    Inventory.Instance.AddEquip(equip);
                    TxtOut([$"보물상자에서 {equip.GetEquipName}을(를) 획득했다!"]);
                }
                else
                {
                    TxtOut([$"장비 정보를 찾을 수 없습니다: {itemname}"]);
                }
                break;
            }
            case ItemType.Weapon:
            {
                if (int.TryParse(itemname, out int weaponId))
                {
                    Equipment? weapon = GameManager.Instance.FindEquipment(EquipType.WEAPON, weaponId);
                    if (weapon != null)
                    {
                        Inventory.Instance.AddEquip(weapon);
                        TxtOut([$"보물상자에서 {weapon.GetEquipName}을(를) 획득했다!"]);
                    }
                    else
                    {
                        TxtOut([$"무기 정보를 찾을 수 없습니다: {itemname}"]);
                    }
                }
                else
                {
                    TxtOut([$"알 수 없는 무기 데이터입니다: {itemname}"]);
                }
                break;
            }
            case ItemType.SkillBook:
            {
                var skill = GameManager.Instance.FindSkillByName(itemname);
                if (skill != null)
                {
                    player.skillSystem.AddCanUseSkill(itemname);
                    TxtOut([$"보물상자에서 스킬북을 읽고 '{itemname}' 스킬을 익혔다!"]);
                }
                else
                {
                    TxtOut([$"알 수 없는 스킬북입니다: {itemname}"]);
                }
                break;
            }
            case ItemType.Stat:
            {
                switch (itemname.ToLowerInvariant())
                {
                    case "str":
                        player.MyStatus.ATK += 1;
                        TxtOut(["힘이 1 상승했다!"]);
                        break;
                    case "def":
                        player.MyStatus.DEF += 1;
                        TxtOut(["방어력이 1 상승했다!"]);
                        break;
                    case "hp":
                        player.MyStatus.MaxHp += 5;
                        player.MyStatus.Hp = Math.Min(player.MyStatus.Hp + 5, player.MyStatus.MaxHp);
                        TxtOut(["최대 체력이 5 증가했다!"]);
                        break;
                    case "mp":
                        player.MyStatus.MaxMp += 5;
                        player.MyStatus.Mp = Math.Min(player.MyStatus.Mp + 5, player.MyStatus.MaxMp);
                        TxtOut(["최대 마나가 5 증가했다!"]);
                        break;
                    default:
                        TxtOut([$"알 수 없는 스탯 보상입니다: {itemname}"]);
                        break;
                }
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
