using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newgame
{
    internal class DataManager
    {
        static DataManager instance;

        public static DataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataManager();
                }
                return instance;
            }
        }

        public void LoadAllEquipment()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;

            for(int i = 1; i < (int)EquipType.MAX; i++)
            {
                string fileName = $"Equip_{(EquipType)i}.txt";
                string filePath = Path.Combine(exePath, fileName);

                if(!File.Exists(filePath))
                {
                    Console.WriteLine($"파일이 존재하지 않습니다: {filePath}");
                    continue;
                }


            }
        }

        void SetEquipDeta(string filePath, Equipment _type)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                string name = string.Empty;
                int[] data = new int[3];

                foreach (string line in lines)
                {
                    string[] curLine = line.Split(':');
                    if (curLine[0] == "ID")
                    {
                        data[0] = int.Parse(curLine[1]);
                    }
                    else if (curLine[0] == "Name")
                    {
                        name = curLine[1];
                    }
                    else if (curLine[0] == "Stat")
                    {
                        data[1] = int.Parse(curLine[1]);
                    }
                    else if (curLine[0] == "Price")
                    {
                        data[1] = int.Parse(curLine[1]);
                    }
                    Equipment eq = new Equipment(_type, data[0], name, data[1], data[2]);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"파일을 읽는 중 오류 발생: {ex.Message}");
            }
        }
    }
}
