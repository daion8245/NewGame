using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace newgame
{
    [JsonObject(MemberSerialization.Fields)]
    internal class Inventory
    {
        static Inventory instance;
        //외부에서 접근할 수 있도록 static 형식으로 자신을 호출할 때 사용한다.
        //Instance = 싱글톤 이라고도 한다.
        public static Inventory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Inventory();
                }

                return instance;
            }
        }

        private Dictionary<EquipType, Equipment> equips =
            new Dictionary<EquipType, Equipment>()
            {
                {EquipType.WEAPON, null },
                {EquipType.HELMET, null },
                {EquipType.SHIRT, null },
                {EquipType.PANTS, null },
                {EquipType.GLOVE, null },
                {EquipType.SHOES, null }
            };

        //착용 가능한 장비
        List<Equipment> canEquips = new List<Equipment>();

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

        public void SetEquip(int idx)
        {
            if (idx < 1 || idx > canEquips.Count)
            {
                Console.WriteLine("착용가능 장비 없음");
                return;
            }
            Equipment equip = canEquips[idx - 1];
            EquipType type = equip.GetEquipType;
            int id = equip.GetEquipID;

            SetEquip(type, id);

        }

        public void SetEquip(EquipType _type, int _id)
        {
            //현제 착용하고 있는 장비를 확인하는 함수
            Equipment item = GameManager.Instance.FindEquipment(_type, _id);//equipment 타입item변수에
                                                                            //착용 가능한 장비의
                                                                            //타입과 id를 넣는다.
            if (item == null)
            {
                return;
            }

            if (equips.ContainsKey(_type) == false)
            {
                return;
            }

            if (equips[_type] != null)
            {

                foreach (var can in canEquips)//그니깐 이건 리스트에 들어있는 모든 장비를 하나씩 꺼내서 can변수에 넣는 코드
                {
                    //canEquips 변수에 저장된 value 값과
                    //지금 착용하려는 아이템이 동일하다면,
                    //내가 현재 가지고 있는 장비 = 착용 가능한 장비
                    if (can == item)
                    {
                        canEquips.Remove(item); //장비 꺼내기, 착용 가능한 장비에서 삭제
                        break;
                    }
                }
                //현제 착용하고 있는 장비를 다시 canEquips 에 저장
                // 위에서 착용하지 않고 가지고 있던 장비를 꺼냈으니깐
                canEquips.Add(equips[_type]);
            }
            //해당 Key 값이 item에 추가되게 한다
            equips[_type] = item;
        }

        public Equipment GetEquip(EquipType _type)
        {
            if (equips[_type] == null)
            {
                return null;
            }
            return equips[_type];
        }

        #region 착용 장비 보이기
        public void ShowEquipList()
        {
            // 기존 구현은 콘솔 화면에 상자를 그리며 장비를 표시하려 했으나
            // 미완성된 상태여서 컴파일 오류가 발생하였다. 간단한 텍스트
            // 형식으로 현재 장비 목록을 출력하도록 수정한다.

            Console.WriteLine("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
            Console.WriteLine("┃          장착 장비           ┃");
            Console.WriteLine("┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫");

            foreach (EquipType type in Enum.GetValues(typeof(EquipType)))
            {
                if (type == EquipType.NONE || type == EquipType.MAX)
                {
                    continue;
                }

                string equipName = "없음";
                string? upType = null;
                if (equips.ContainsKey(type) && equips[type] != null)
                {
                    equipName = equips[type].GetEquipName;
                }

                Console.WriteLine($"┃ {type,-6} : {equipName,-14}");
            }

            Console.WriteLine("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");
        }
        #endregion

        public void AddEquip(Equipment equip)
        {
            canEquips.Add(equip);
        }

        public void RemoveCanEquip(int idx)
        {
            int temp = idx - 1;
            if (temp >= 0 && temp < canEquips.Count)
            {
                GameManager.Instance.player.MyStatus.gold += canEquips[idx - 1].GetPrice;
                canEquips.RemoveAt(idx - 1);
                return;
            }

            Console.WriteLine("해당 번호의 장비가 존재하지 않습니다.");
        }

        public int ShowCanEquips()
        {
            if (canEquips == null || canEquips.Count == 0)
            {
                return -1;
            }

            List<string> equipItemList = new List<string>();

            string? upType = null;

            for (int i = 0; i < canEquips.Count; i++)
            {
                if (canEquips[i].GetEquipType is EquipType.WEAPON or EquipType.HELMET or EquipType.GLOVE or EquipType.SHOES)
                {
                    upType = "공격력";
                }
                else
                {
                    upType = "방어력";
                }
                equipItemList.Add($"{canEquips[i].GetEquipName} -> {upType}+{canEquips[i].GetEquipStat} 증가");
                //equipItemList.Add($"┃ {canEquips[i].GetEquipName} ");
            }

            List<string> equipList = new List<string>();

            Console.WriteLine("┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
            Console.WriteLine("┃          인벤토리            ┃");
            Console.WriteLine("┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛");
            int SelectEquip = UiHelper.SelectMenu(equipItemList.ToArray());
            return SelectEquip;
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

                instance = JsonConvert.DeserializeObject<Inventory>(data, settings);
            }
        }

        #region 아이템 관련 추가
        public void AddItem(Item item, int count = 1)
        {
            if (items == null)
                items = new List<ItemSlot>();

            foreach (ItemSlot slot in items)
            {
                if (slot.Item.ItemType == item.ItemType)
                {
                    slot.Add(count);
                    return;
                }
            }

            items.Add(new ItemSlot(item, count));
        }

        public void RemoveItem(ItemType type, int count = 1)
        {
            foreach (ItemSlot slot in items)
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
                        items.Remove(slot);
                    }

                    return;
                }
            }

            Console.WriteLine("해당 아이템이 없습니다.");
        }

        public void UseItem(int index)
        {
            var slot = items[index - 1];
            var item = slot.Item;

            item.Use(); // 효과 메시지 출력

            if (item.IsPersistent())
            {
                GameManager.Instance.player.AddEffect(item);
            }

            slot.Decrease(1);
            if (slot.Count <= 0)
            {
                items.Remove(slot);
            }
        }

        public bool ShowItems()
        {
            if (items.Count == 0)
            {
                Console.WriteLine("보유 아이템이 존재하지 않습니다.");
                return false;
            }

            Console.WriteLine("보유 아이템 목록:");

            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {GetItemName(items[i].Item.ItemType)} x {items[i].Count}");
            }

            return true;
        }

        public int SelectedItem(string _input)
        {
            if (int.TryParse(_input, out int idx))
            {
                if (idx > items.Count)
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
