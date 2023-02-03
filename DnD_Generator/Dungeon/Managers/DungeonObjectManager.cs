using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommandEngine;

namespace DnD_Generator
{
    class DungeonObjectManager<TObject> where TObject : class, IDungeonObject
    {
        DungeonRoomManager roomManager;
        Dictionary<int, TObject> ObjectsById = new Dictionary<int, TObject>();
        Dictionary<DungeonRoom, HashSet<int>> ObjectsPerRoom = new Dictionary<DungeonRoom, HashSet<int>>();

        public DungeonObjectManager(DungeonRoomManager rm)
        {
            roomManager = rm;
        }

        public void AddObject(TObject obj, DungeonRoom room)
        {
            if (ObjectsById.ContainsKey(obj.ID))
                return;

            ObjectsById.Add(obj.ID, obj);

            if (ObjectsPerRoom.ContainsKey(room))
                ObjectsPerRoom[room].Add(obj.ID);
            else ObjectsPerRoom.Add(room, new HashSet<int>() { obj.ID });
        }

        public bool RemoveObject(int objId, DungeonRoom room)
        {
            if (!IsObjectInRoom(objId, room))
                return false;

            ObjectsById.Remove(objId);
            ObjectsPerRoom[room].Remove(objId);
            return true;
        }
        public bool RemoveObject(int objId, DungeonRoom room, out TObject removedObj)
        {
            removedObj = GetObject(objId);
            return RemoveObject(objId, room);
        }
        public bool RemoveObject(TObject obj, DungeonRoom room)
            => RemoveObject(obj.ID, room);

        public bool IsObjectInRoom(int objId, DungeonRoom room)
            => ObjectsPerRoom.ContainsKey(room) ? ObjectsPerRoom[room].Contains(objId) : false;
        public bool IsObjectInRoom(TObject obj, DungeonRoom room)
            => IsObjectInRoom(obj.ID, room);

        public TObject GetObject(int objId)
            => ObjectsById.ContainsKey(objId) ? ObjectsById[objId] : null;

        public TObject GetObjectInRoom(DungeonRoom room, int objId)
            => GetObjectCount(room) > 0 ? GetObject(objId) : null;

        public List<TObject> GetObjectsInRoom(DungeonRoom room)
            => GetObjectCount(room) > 0 ? ObjectsPerRoom[room].Select((index) => ObjectsById[index]).ToList() : new List<TObject>();

        public TObject GetRandomObject(DungeonRoom room)
            => GetObjectCount(room) > 0 ? ObjectsById[ObjectsPerRoom[room].RandomItem()] : null;

        public IEnumerable<TObject> GetAllObjects()
            => ObjectsById.Values;

        public bool RoomContainsObjects(DungeonRoom room)
            => GetObjectCount(room) > 0;

        public int GetObjectCount()
            => ObjectsById.Count;
        public int GetObjectCount(DungeonRoom room)
            => ObjectsPerRoom.ContainsKey(room) ? ObjectsPerRoom[room].Count : 0;
    }
}
