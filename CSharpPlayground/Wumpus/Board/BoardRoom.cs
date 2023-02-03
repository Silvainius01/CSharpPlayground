using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommandEngine;

namespace CSharpPlayground.Wumpus
{
    public class BoardRoomConnection
    {
        /// <summary> Arrows can pass through connections on the same plane  </summary>
        public int plane;
        /// <summary> Angle relative to center of the room. </summary>
        public float angle;
        /// <summary> Distance from the center of the room. </summary>
        public float distFromCenter;
        /// <summary> The Connected Room </summary>
        public BoardRoom room;

        public BoardRoomConnection(BoardRoom room, float angle, float distFromCenter, int plane)
        {
            this.room = room;
            this.angle = angle;
            this.distFromCenter = distFromCenter;
            this.plane = plane;
        }
    }

    /// <summary>
    /// The class used by the game. Inherited classes should only contain data neccessary for board generation, as it will not be accessed by entities.
    /// </summary>
    public abstract class BoardRoom
    {
        public readonly int index;
        Dictionary<int, BoardRoom> connections = new Dictionary<int, BoardRoom>();
        Dictionary<ulong, BoardEntity> entitiesInRoom = new Dictionary<ulong, BoardEntity>();
        Dictionary<Type, HashSet<BoardEntity>> entityTypesInRoom = new Dictionary<Type, HashSet<BoardEntity>>();
        Dictionary<ITEM_ID, int> itemsInRoom = new Dictionary<ITEM_ID, int>();

        public int NumConnections { get { return connections.Count; } }

        public BoardRoom(int index)
        {
            this.index = index;
        }

        public bool AddEntity<T>(T entity) where T : BoardEntity
        {
            if (entitiesInRoom.ContainsKey(entity.entity.Id))
                return false;
            entitiesInRoom.Add(entity.entity.Id, entity);
            AddEntityType(entity);
            return true;
        }
        private void AddEntityType<T>(T entity) where T : BoardEntity
        {
            var type = typeof(T);
            if (entityTypesInRoom.ContainsKey(type))
            {
                if (!entityTypesInRoom[type].Contains(entity))
                    entityTypesInRoom[type].Add(entity);
                return;
            }
            entityTypesInRoom.Add(type, new HashSet<BoardEntity>() { entity });
        }

        public bool AddEntity(BoardEntity entity, Type entityType)
        {
            if (entitiesInRoom.ContainsKey(entity.entity.Id))
                return false;
            entitiesInRoom.Add(entity.entity.Id, entity);
            AddEntityType(entity, entityType);
            return true;
        }
        private void AddEntityType(BoardEntity entity, Type entityType)
        {
            if (entityTypesInRoom.ContainsKey(entityType))
            {
                if (!entityTypesInRoom[entityType].Contains(entity))
                    entityTypesInRoom[entityType].Add(entity);
                return;
            }
            entityTypesInRoom.Add(entityType, new HashSet<BoardEntity>() { entity });
        }

        public bool RemoveEntity<T>(T entity) where T : BoardEntity
        {
            if (!entitiesInRoom.ContainsKey(entity.entity.Id))
                return false;
            entitiesInRoom.Remove(entity.entity.Id);
            RemoveEntityType(entity);
            return true;
        }
        private void RemoveEntityType<T>(T entity) where T : BoardEntity
        {
            var type = typeof(T);
            if (entityTypesInRoom.ContainsKey(type) && entityTypesInRoom[type].Contains(entity))
                entityTypesInRoom[type].Remove(entity);
        }

        public bool RemoveEntity(BoardEntity entity, Type entityType)
        {
            if (!entitiesInRoom.ContainsKey(entity.entity.Id))
                return false;
            entitiesInRoom.Remove(entity.entity.Id);
            RemoveEntityType(entity, entityType);
            return true;
        }
        private void RemoveEntityType(BoardEntity entity, Type entityType)
        {
            if (entityTypesInRoom.ContainsKey(entityType) && entityTypesInRoom[entityType].Contains(entity))
                entityTypesInRoom[entityType].Remove(entity);
        }

        public bool ContainsEntity(BoardEntity entity)
        {
            return entitiesInRoom.ContainsKey(entity.entity.Id);
        }
        public bool ContainsEntityType<T>() where T : BoardEntity
        {
            var type = typeof(T);
            return entityTypesInRoom.ContainsKey(type) && entityTypesInRoom[type].Count > 0;
        }

        public T[] GetEntitiesInRoom<T>() where T : BoardEntity
        {
            var type = typeof(T);
            if (!entityTypesInRoom.ContainsKey(type) || entityTypesInRoom[type].Count == 0)
                return null;
            return entityTypesInRoom[type].Cast<T>().ToArray();
        }

        public void AddItem(ITEM_ID itemId, int count)
        {
            if (itemsInRoom.ContainsKey(itemId))
                itemsInRoom[itemId] += count;
            else itemsInRoom.Add(itemId, count);
        }
        /// <summary> Removes all items if count is less than 0 </summary>
        public void RemoveItem(ITEM_ID itemId, int count)
        {
            if (itemsInRoom.ContainsKey(itemId))
            {
                int currCount = itemsInRoom[itemId];
                itemsInRoom[itemId] -= count < 0 || count < currCount ? count : currCount;
            }
            else itemsInRoom.Add(itemId, 0);
        }

        public bool ContainsItem(ITEM_ID itemId)
        {
            return itemsInRoom.ContainsKey(itemId);
        }
        public int GetItemCount(ITEM_ID itemId)
        {
            if (!itemsInRoom.ContainsKey(itemId))
                itemsInRoom.Add(itemId, 0);
            return itemsInRoom[itemId];
        }
        public KeyValuePair<ITEM_ID, int>[] GetItemCounts()
        {
            return itemsInRoom.Where(kvp => kvp.Value > 0).ToArray();
        }

        public bool ConnectRoom(BoardRoom room)
        {
            if (connections.ContainsKey(room.index))
                return false;
            connections.Add(room.index, room);
            return true;
        }

        public bool DisconnectRoom(BoardRoom room)
        {
            if (!connections.ContainsKey(room.index))
                return false;
            connections.Remove(room.index);
            return true;
        }

        public bool IsConnectedTo(BoardRoom room)
        {
            return connections.ContainsKey(room.index);
        }
        public bool IsConnectedTo(int index)
        {
            return connections.ContainsKey(index);
        }

        public BoardRoom[] GetConnectedRooms()
        {
            return connections.Values.ToArray();
        }
        public BoardRoom GetConnectedRoom(int index)
        {
            if (IsConnectedTo(index))
            {
                return connections[index];
            }
            return null;
        }


        public static explicit operator BoardRoom(BoardRoomConnection connection)
        {
            return connection.room;
        }
    }


    public class BoardRoom2D : BoardRoom
    {
        public Vector2 position;

        public BoardRoom2D(int index) : base(index)
        {
            position = Vector2.Zero;
        }
        public BoardRoom2D(int index, Vector2 position) : base(index)
        {
            this.position = position;
        }
    }
}
