using System.Collections.Generic;
using newgame.Characters;
using newgame.Services;
using newgame.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace newgame.Items
{
    [JsonObject(MemberSerialization.Fields)]
    internal class Inventory
    {
        private static Inventory? _instance;
        //외부에서 접근할 수 있도록 static 형식으로 자신을 호출할 때 사용한다.
        //Instance = 싱글톤 이라고도 한다.
        public static Inventory Instance
        {
            get
            {
                _instance ??= new Inventory();

                return _instance;
            }
        }

        private Dictionary<EquipType, Equipment?> _equips =
            new Dictionary<EquipType, Equipment?>()
            {
                {EquipType.WEAPON, null! },
                {EquipType.HELMET, null! },
                {EquipType.SHIRT, null!},
                {EquipType.PANTS, null! },
                {EquipType.GLOVE, null! },
                {EquipType.SHOES, null! }
            };

        //착용 가능한 장비
        private List<Equipment> _canEquips = [];

        #region 아이템 관련 추가
        [JsonProperty]
        private List<ItemSlot> _items = [];

        [JsonProperty] private Dictionary<ItemType, string> _itemNames = new Dictionary<ItemType, string>()
                {
                    {ItemType.F_POTION_LOW_HP, "하급 회복 물약" },
                    {ItemType.F_POTION_MIDDLE_HP, "중급 회복 물약" },
                    {ItemType.F_POTION_HIGH_HP, "상급 회복 물약" },
                    {ItemType.T_POTION_EXPUP, "경험치 획득량 증가" },
                    {ItemType.T_POTION_ATKUP, "공격력 증가" },
                    {ItemType.F_ETC_RESETNAME, "닉네임 변경" },

                    #region 제작 재료
                    {ItemType.M_WOOD, "나무" }
                    #endregion
        };

        public string GetItemName(ItemType type)
        {
            foreach (var names in _itemNames)
            {
                if (names.Key == type)
                {
                    return names.Value;
                }
            }

            return string.Empty;
        }
        #endregion

        public void SetEquip(int idx)
        {
            if (idx < 1 || idx > _canEquips.Count)
            {
                Console.WriteLine("착용가능 장비 없음");
                return;
            }
            Equipment equip = _canEquips[idx - 1];
            EquipType type = equip.GetEquipType;
            int id = equip.GetEquipID;

            SetEquip(type, id);

        }

        public void SetEquip(EquipType type, int id)
        {
            //현제 착용하고 있는 장비를 확인하는 함수
            Equipment? item = GameManager.Instance.FindEquipment(type, id);//equipment 타입item변수에
                                                                            //착용 가능한 장비의
                                                                            //타입과 id를 넣는다.
            if (item == null)
            {
                return;
            }

            Equipment equipItem = item;

            if (_equips.TryGetValue(type, out var currentlyEquipped) == false)
            {
                return;
            }

            if (currentlyEquipped != null)
            {

                foreach (var can in _canEquips)//그니깐 이건 리스트에 들어있는 모든 장비를 하나씩 꺼내서 can변수에 넣는 코드
                {
                    //canEquips 변수에 저장된 value 값과
                    //지금 착용하려는 아이템이 동일하다면,
                    //내가 현재 가지고 있는 장비 = 착용 가능한 장비
                    if (can == equipItem)
                    {
                        _canEquips.Remove(equipItem); //장비 꺼내기, 착용 가능한 장비에서 삭제
                        break;
                    }
                }
                //현제 착용하고 있는 장비를 다시 canEquips 에 저장
                // 위에서 착용하지 않고 가지고 있던 장비를 꺼냈으니깐
                _canEquips.Add(currentlyEquipped);
            }
            //해당 Key 값이 item에 추가되게 한다
            _equips[type] = equipItem;
        }

        public Equipment? GetEquip(EquipType type)
        {
            return _equips.GetValueOrDefault(type);
        }

        public IEnumerable<Equipment> GetEquippedItems()
        {
            foreach (Equipment? equip in _equips.Values)
            {
                if (equip != null)
                {
                    yield return equip;
                }
            }
        }

        #region 착용 장비 보이기
        public void ShowEquipList()
        {
            Console.WriteLine("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
            Console.WriteLine("┃          장착 장비           ┃");
            Console.WriteLine("┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫");

            foreach (EquipType type in Enum.GetValues(typeof(EquipType)))
            {
                if (type == EquipType.NONE || type == EquipType.MAX)
                {
                    continue;
                }

                Equipment? equipped = _equips.GetValueOrDefault(type);
                PrintEquipmentWithStats(type, equipped);
            }

            Console.WriteLine("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");
        }

        static void PrintEquipmentWithStats(EquipType type, Equipment? equipment)
        {
            string equipName = equipment?.GetEquipName ?? "없음";
            Console.WriteLine($"┃ {type,-6} : {equipName,-14}");

            if (equipment == null)
            {
                return;
            }

            List<string> statLines = new List<string>();
            foreach (string part in equipment.GetEquipStat.EnumerateSummaryParts())
            {
                statLines.Add(part);
            }

            if (statLines.Count == 0)
            {
                Console.WriteLine("┃          - 능력치 없음");
                return;
            }

            foreach (string stat in statLines)
            {
                Console.WriteLine($"┃          - {stat}");
            }
        }
        #endregion

        public void AddEquip(Equipment equip)
        {
            _canEquips.Add(equip);
        }

        public void RemoveCanEquip(int idx)
        {
            int temp = idx - 1;
            if (temp >= 0 && temp < _canEquips.Count)
            {
                Player player = GameManager.Instance.RequirePlayer();
                player.MyStatus.gold += _canEquips[idx - 1].GetPrice;
                _canEquips.RemoveAt(idx - 1);
                return;
            }

            Console.WriteLine("해당 번호의 장비가 존재하지 않습니다.");
        }

        public int ShowCanEquips()
        {
            if (_canEquips.Count == 0)
            {
                return -1;
            }

            List<string> equipItemList = [];

            foreach (Equipment t in _canEquips)
            {
                equipItemList.Add($"{t.GetEquipName} -> {t.GetEquipStat.ToSummary()}");
            }

            Console.WriteLine("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
            Console.WriteLine("┃          인벤토리            ┃");
            Console.WriteLine("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");
            int selectEquip = UiHelper.SelectMenu(equipItemList.ToArray());
            return selectEquip;
        }

        public void Load()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Inventory.json");

            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new StringEnumConverter() }
                };

                _instance = JsonConvert.DeserializeObject<Inventory>(data, settings);
            }
        }

        #region 아이템 관련 추가
        public void AddItem(Item item, int count = 1)
        {
            foreach (ItemSlot slot in _items)
            {
                if (slot.Item.ItemType == item.ItemType)
                {
                    slot.Add(count);
                    return;
                }
            }

            _items.Add(new ItemSlot(item, count));
        }

        public bool AddItem(ItemType type, int count = 1)
        {
            Item? item = GameManager.Instance.FindItem(type);
            if (item == null)
            {
                Console.WriteLine($"아이템 데이터가 초기화되지 않았거나 {type} 을(를) 찾을 수 없습니다.");
                return false;
            }

            AddItem(item, count);
            return true;
        }

        public void RemoveItem(ItemType type, int count = 1)
        {
            foreach (ItemSlot slot in _items)
            {
                if (slot.Item.ItemType == type)
                {
                    if (slot.Count < count)
                    {
                        Console.WriteLine("해당 아이템이 충분하지 않습니다.");
                        return;
                    }

                    slot.Decrease(count);

                    if (slot.Count <= 0)
                    {
                        _items.Remove(slot);
                    }

                    return;
                }
            }

            Console.WriteLine("해당 아이템이 없습니다.");
        }

        public void UseItem(int index)
        {
            var slot = _items[index - 1];
            var item = slot.Item;

            item.Use(); // 효과 메시지 출력

            if (item.IsPersistent())
            {
                Player player = GameManager.Instance.RequirePlayer();
                player.AddEffect(item);
            }

            slot.Decrease(1);
            if (slot.Count <= 0)
            {
                _items.Remove(slot);
            }
        }

        public bool ShowItems()
        {
            if (_items.Count == 0)
            {
                Console.WriteLine("보유 아이템이 존재하지 않습니다.");
                return false;
            }

            Console.WriteLine("보유 아이템 목록:");

            for (int i = 0; i < _items.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {GetItemName(_items[i].Item.ItemType)} x {_items[i].Count}");
            }

            return true;
        }

        public int SelectedItem(string input)
        {
            if (int.TryParse(input, out int idx))
            {
                if (idx > _items.Count)
                {
                    return -1;
                }

                return idx;
            }

            return -1;
        }
        #endregion
    }
}
