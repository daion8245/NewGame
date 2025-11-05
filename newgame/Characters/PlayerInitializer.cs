using newgame.Items;
using newgame.UI;

namespace newgame.Characters
{
    internal class PlayerInitializer
    {
        private Status status;
        private readonly Inventory inventory;
        private readonly Skills skills;

        public PlayerInitializer(Status status, Inventory inventory, Skills skills)
        {
            this.status = status ?? throw new ArgumentNullException(nameof(status));
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            this.skills = skills ?? throw new ArgumentNullException(nameof(skills));
        }

        public void AttachStatus(Status nextStatus)
        {
            status = nextStatus ?? throw new ArgumentNullException(nameof(nextStatus));
        }

        public Status Start()
        {
            Status created = Create();
            Console.Clear();
            return created;
        }

        public void SetDefStat(int atk, int hp, int def, int mp)
        {
            status.level = 1;
            status.ATK = 5 + atk;
            status.MaxHp = 40 + (hp * 5);
            status.Hp = status.MaxHp;
            status.DEF = 3 + def;
            status.MaxMp = 30 + mp;
            status.Mp = status.MaxMp;
            status.CriticalChance = 5;
            status.CriticalDamage = 110;
            status.exp = 0;
            status.nextEXP = 50;
            status.gold = 10;
        }

        public void ShowStat()
        {
            UiHelper.TxtOut(new string[]
            {
                $"이름 : {status.Name}",
                $"  레벨 : {status.level}",
                $"  체력 : {status.Hp}/{status.MaxHp}",
                $"  공격력 : {status.ATK}",
                $"  방어력 : {status.DEF}",
                $"  마나 : {status.Mp}/{status.MaxMp}",
                $"  치명타 확률 : {status.CriticalChance}",
                $"  치명타 피해 : {status.CriticalDamage}",
                $"  골드 : {status.gold}",
                $"  경험치 : {status.exp}/{status.nextEXP}"
            }, SlowTxtOut: true, SlowTxtLineTime: 0, SlowTxtOutTime: 1);
        }

        private Status Create()
        {
            status = new Status
            {
                charType = CharType.PLAYER
            };
            SetPlayerStarterItem();
            return status;
        }

        private void SetPlayerStarterItem()
        {
            for (int i = 1; i < (int)EquipType.MAX; i++)
            {
                inventory.SetEquip((EquipType)i, 1);
            }

            inventory.ShowEquipList();
        }
    }
}
