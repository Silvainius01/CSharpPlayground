using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine;

namespace CSharpPlayground.Wumpus
{
    /// <summary>
    /// Generates a 2D board that connects rooms that are directly adjacent to one another. 
    /// </summary>
    class DefaultBoardGenerator : IBoardGenerator
    {
        bool connectDiagnols;
        public Vector2Int dimensions;

        public List<BoardRoom> rooms { get; set; }

        public DefaultBoardGenerator(int width, int height, bool allowDiagnolConnections)
        {
            dimensions = new Vector2Int(width, height);
            connectDiagnols = allowDiagnolConnections;
        }
        public DefaultBoardGenerator(Vector2Int dimensions, bool allowDiagnolConnections) : this(dimensions.X, dimensions.Y, allowDiagnolConnections)
        {
        }

        public void GenerateBoard()
        {
            rooms = new List<BoardRoom>(dimensions.X * dimensions.Y);

            // pos = x + (x * y)
            //   y = floor(pos / y)
            //   x = pos % X

            // Construct Rooms
            for (int i = 0; i < rooms.Capacity; ++i)
            {
                rooms.Add(new BoardRoom2D(i, new Vector2(i % dimensions.X, i / dimensions.Y)));
            }

            // Connect rooms
            for (int i = 0; i < rooms.Count; ++i)
            {
                var currRoom = rooms[i] as BoardRoom2D;

                // If room is not on the bottom row
                if (currRoom.position.Y > 0)
                {
                    // Connect to room below
                    currRoom.ConnectRoom(rooms[i - dimensions.Y]);
                }
                // If currRoom is not on the top row
                if (currRoom.position.Y < dimensions.Y - 1)
                {
                    // Connect to room above
                    currRoom.ConnectRoom(rooms[i + dimensions.Y]);
                }
                // If room is not on the left most column
                if (currRoom.position.X > 0)
                {
                    // Connect to room on left
                    currRoom.ConnectRoom(rooms[i - 1]);
                }
                // If room is not in the right most column
                if (currRoom.position.X < dimensions.X - 1)
                {
                    // Connect to room on the right
                    currRoom.ConnectRoom(rooms[i + 1]);
                }

                if (connectDiagnols)
                {
                    // Not on the top row
                    if (currRoom.position.Y < dimensions.Y - 1)
                    {
                        // If room is not in the right most column
                        if (currRoom.position.X < dimensions.X - 1)
                        {
                            // Connect right diagnol up
                            currRoom.ConnectRoom(rooms[i + 1 + dimensions.Y]);
                        }
                        // If room is not on the left most column
                        if (currRoom.position.X > 0)
                        {
                            // Connect left diagnol up
                            currRoom.ConnectRoom(rooms[i - 1 + dimensions.Y]);
                        }
                    }
                    // If room is not on the bottom row
                    if (currRoom.position.Y > 0)
                    {
                        // If room is not in the right most column
                        if (currRoom.position.X < dimensions.X - 1)
                        {
                            // Connect right diagnol down
                            currRoom.ConnectRoom(rooms[i + 1 - dimensions.Y]);
                        }
                        // If room is not on the left most column
                        if (currRoom.position.X > 0)
                        {
                            currRoom.ConnectRoom(rooms[i - 1 - dimensions.Y]);
                        }
                    }
                }
            }
        }

        public BoardRoom GetRoomFromIndex(int index)
        {
            return rooms[index];
        }
    }
}
