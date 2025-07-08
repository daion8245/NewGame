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
            #region 박스 출력
            for (int i = 0; i <= 7; i++)
            {
                for(int j = 0; j <= 30; j++)
                {
                    if ((i, j) == (0, 0))
                    {
                        Console.Write('┏');
                    }
                    else if ((i, j) == (0, 30))
                    {
                        Console.Write('┓');
                    }
                    else if (i == 0 && j == 20)
                    {
                        for (int l = 0; l <= 8; l++)
                        {
                            Console.Write("\b \b");
                        }
                        Console.Write(" 인벤토리 ");
                    }
                    else if ((i, j) == (7, 30))
                    {
                        Console.Write('┛');
                    }
                    else if ((i, j) == (7, 0))
                    {
                        Console.Write('┗');
                    }
                    else if (j == 0 || j == 30)
                    {
                        Console.Write('┃');
                    }
                    else if (i == 0 || i == 7 || j < 0 || j > 30)
                    {
                        Console.Write('━');
                    }
                    else
                    {
                        Console.Write(' ');
                    }


                    if(i >= 2 && i <= 5)
                    {
                        if()
                        {

                        }
                    }
                }
                Console.WriteLine();
            }
            #endregion


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

        public bool ShowCanEquips()
        {
            if (canEquips.Count == 0 || canEquips == null)
            {
                return false;
            }

            Console.WriteLine("착용 가능한 장비 목록:");
            Console.WriteLine("--------------------");

            for (int i = 0; i < canEquips.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {canEquips[i].GetEquipName} - 가격: {canEquips[i].GetPrice}");
            }
            Console.WriteLine("--------------------");
            Console.WriteLine();
            return true;
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
    }
}
