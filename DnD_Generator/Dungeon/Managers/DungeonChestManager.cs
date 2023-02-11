using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RogueCrawler
{
    class DungeonChestManager : DungeonObjectManager<DungeonChest<IItem>>
    {
        public DungeonChestManager(DungeonRoomManager rm) : base(rm) { }
    }
}
