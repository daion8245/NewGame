using newgame.Entity.Equip;
using newgame.Entity.Skill;
using newgame.Manager;
using System;

namespace newgame.Entity.Player
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
            status.ATK = 8 + atk;
            status.MaxHp = 45 + (hp * 10);
            status.Hp = status.MaxHp;
            status.DEF = 2 + def;
            status.MaxMp = 30 + (mp * 2);
            status.Mp = status.MaxMp;
            status.CriticalChance = 10;
            status.CriticalDamage = 150;
            status.exp = 0;
            status.nextEXP = 20;
            status.gold = 25;
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
            SetPlayerStarterSkill();
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

        private void SetPlayerStarterSkill()
        {
            skills.ClearAllCanUseSkills();
            skills.AddCanUseSkill("파이어볼");
            skills.AddCanUseSkill("아쿠아 볼");
        }
    }
}
