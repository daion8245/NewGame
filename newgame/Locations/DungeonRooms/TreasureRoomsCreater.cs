namespace newgame.Locations.DungeonRooms;

internal class TreasureRoomsCreater : EventRoomsCreater
{
    internal enum TreasureRoomsId
    {
        
    }
    
    public void CreateDungeonTreasureRoom()
    {
        int randomRoomSelect;
        Random rand = new Random();
            
        // 모든 이벤트 방이 사용되었으면 사용 기록 초기화
        if (_usedEventRoomIds.Count >= Enum.GetNames(typeof(TreasureRoomsId)).Length)
        {
            _usedEventRoomIds.Clear();
        }
            
        do
        {
            randomRoomSelect = rand.Next(0, Enum.GetNames(typeof(TreasureRoomsId)).Length);
        } while (_usedEventRoomIds.Contains(randomRoomSelect));
            
        // 선택된 ID를 사용된 목록에 추가
        _usedEventRoomIds.Add(randomRoomSelect);
            
        // 선택된 ID에 따라 이벤트 방 생성
        TreasureRoomsId selectedEventRoom = (TreasureRoomsId)randomRoomSelect;
            
        SelectedTreasureRoom(selectedEventRoom);
    }

    public void SelectedTreasureRoom(TreasureRoomsId eventRoomId)
    {
        switch (eventRoomId)
        {
            
        }
    }
}