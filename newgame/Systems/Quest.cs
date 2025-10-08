using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newgame.Systems
{
    delegate void QuestMobDeadHandler(string mob);
    internal class Quest
    {
        public static event QuestMobDeadHandler MobDeadEvent;

        public static void OnMobDead(string mob)
        {
            
        }
    }
}
