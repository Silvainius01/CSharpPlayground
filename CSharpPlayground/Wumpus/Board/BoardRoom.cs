using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine;

namespace CSharpPlayground.Wumpus
{
    /// <summary>
    /// The class used by the game. Inherited classes should only contain data neccessary for board generation, as it will not be accessed by entities.
    /// </summary>
    public abstract class BoardRoom
    {
        public readonly int index;
        Dictionary<int, BoardRoom> connections = new Dictionary<int, BoardRoom>();
        Dictionary<ulong, BoardEntity> entitiesInRoom = new Dictionary<ulong, BoardEntity>();

        public int NumConnections { get { return connections.Count; } }

        public BoardRoom(int index)
        {
            this.index = index;
        }

        public bool RemoveEntity(BoardEntity entity)
        {
            if (!entitiesInRoom.ContainsKey(entity.entity.Id))
                return false;
            entitiesInRoom.Remove(entity.entity.Id);
            return true;
        }
        public bool AddEntity(BoardEntity entity)
        {
            if (entitiesInRoom.ContainsKey(entity.entity.Id))
                return false;
            entitiesInRoom.Add(entity.entity.Id, entity);
            return true;
        }
        public bool ContainsEntity(BoardEntity entity)
        {
            return entitiesInRoom.ContainsKey(entity.entity.Id);
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
    }


    public class BoardRoom2D : BoardRoom
    {
        public Vector2Int position;

        public BoardRoom2D(int index) : base(index)
        {
            position = Vector2Int.Zero;
        }
        public BoardRoom2D(int index, Vector2Int position) : base(index)
        {
            this.position = position;
        }
    }
}
