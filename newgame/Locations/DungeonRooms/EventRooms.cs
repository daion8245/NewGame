using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newgame.DungeonRooms
{
    //이벤트 방 내용 담을 델리게이트
    delegate void EventRoomAction();

    internal class EventRooms
    {
        private string eventName;
        private string eventDiscripsion;
        EventRoomAction eventAction;

        public EventRooms(string name, string discripsion, EventRoomAction action)
        {
            eventName = name;
            eventDiscripsion = discripsion;
            eventAction = action;

            EnterRoom();
        }

        public void EnterRoom()
        {
            Console.Clear();
            UiHelper.TxtOut(["\t이벤트",$"     '{eventName}'","\n" + eventDiscripsion,"\n"]);
            eventAction();
            Console.WriteLine();
            UiHelper.WaitForInput();
        }
    }
}
