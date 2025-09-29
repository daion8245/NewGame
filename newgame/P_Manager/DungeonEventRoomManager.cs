using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newgame.Manager
{
    internal class DungeonEventRoomManager
    {
        static DungeonEventRoomManager? instance;

        public static DungeonEventRoomManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DungeonEventRoomManager();
                }
                return instance;
            }
        }


    }
}
