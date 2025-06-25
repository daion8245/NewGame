namespace newgame
{
    //능력치를 가지고 있는 모든 캐릭터의 부모 클래스
    internal class Character
    {
        Status Status;

        public Status MyStatus
        {
            get => Status;
            protected set => Status = value;
        }

        public bool IsDead = false;
        public bool isbattleRun = false;
        public virtual void Attack(Character target)
        {
            target.Status.hp -= MyStatus.ATK;
            Console.WriteLine($"{MyStatus.Name}의 공격!{target.Status.Name} 은 {MyStatus.ATK} 데미지를 받았다 {target.Status.Name} 의 남은 체력: {target.Status.hp}");

            if (target.Status.hp <= 0)
            {
                target.Dead(this);
            }
        }

        public virtual void Dead(Character target)
        {
            IsDead = true;

            if (target == GameManager.Instance.player)
            {
                Console.WriteLine($"{Status.Name}는/은 쓰러졌다!");
                Lobby lobby = new Lobby();
                lobby.Start();
            }
            else
            {
                Console.WriteLine("전투에서 패배했다..");
                Console.WriteLine("눈앞이 깜깜해졌다.");
                Thread.Sleep(2000);
                Tavern tavern = new Tavern();
                tavern.Start();
            }
        }
    }
}
