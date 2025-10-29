using Newtonsoft.Json;

namespace newgame.Items
{
    [JsonObject(MemberSerialization.Fields)]
    internal class ItemSlot
    {
        public Item Item { get; private set; }
        public int Count { get; private set; }

        public ItemSlot(Item item, int count)
        {
            Item = item;
            Count = count;
        }

        public void Add(int amount) => Count += amount;
        public void Decrease(int amount) => Count -= amount;
    }
}
