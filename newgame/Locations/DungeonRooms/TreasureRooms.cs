using newgame.UI;

namespace newgame.Locations.DungeonRooms;
using System;
using static newgame.UI.UiHelper;

public class TreasureRooms
{
    public delegate void TreasureRoomAction();
    
    private string roomName; //방 이름
    private string roomDiscripsion; //방 설명
    private TreasureRoomAction roomAction; //방에서 일어나는 행동
    
    public TreasureRooms(string name, string discripsion, TreasureRoomAction action)
    {
        roomName = name;
        roomDiscripsion = discripsion;
        roomAction = action;

        EnterRoom();
    }

    public void EnterRoom()
    {
        Console.Clear();
        UiHelper.TxtOut(new string[]
        {
            $"{roomName}",
            "\n" + roomDiscripsion,
            "\n"
        });
        
        roomAction();
        Console.WriteLine();
        UiHelper.WaitForInput();
    }
}