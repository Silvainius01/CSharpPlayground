using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using GameEngine;

// using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Windows.Forms.VisualStyles;

namespace CSharpPlayground.Wumpus
{
    public class WumpusGameManager : Component
    {
        static WumpusGameManager instance = null;

        WumpusWindow window;
        IBoardGenerator gameBoard;

        int currCommIndex = -1;
        List<string> commandLineHistory = new List<string>();
        Queue<string> commandq = new Queue<string>();

        public static uint wumpusCount = 0;

        public static bool WindowOpen { get; private set; } = true;

        public static WumpusGameManager CreateInstance(IBoardGenerator generator)
        {
            if (instance != null)
                throw new InvalidOperationException("An instnace of GameManager already exists.");
            instance = new Entity("GameManager", true, -1000).AddComponent<WumpusGameManager>(generator);
            return instance;
        }

        public WumpusGameManager(IBoardGenerator generator)
        {
            gameBoard = generator;
            window = WumpusWindow.StartWindow(this);
            window.FormClosed += OnWindowClose;
        }

        #region Instance Methods
        public override void Awake()
        {
            if (gameBoard == null)
                gameBoard = new DefaultBoardGenerator(new Vector2Int(5, 5), false);

            gameBoard.GenerateBoard();
        }

        public override void Start()
        {
            Player p = new Entity("Player").AddComponent<Player>(window.invWindow);
            p.Init(GetRandomRoom());
            p.GiveItem(ITEM_ID.BOW);
            p.GiveItem(ITEM_ID.ARROW, 5);

            for (uint i = 0; i < WumpusGameSettings.WUMPUS_COUNT; ++i)
            {
                Wumpus w = new Entity("Wumpus").AddComponent<Wumpus>();
                w.Init(GetRandomUnconnectedRoom(p.CurrentRoom), p);
                ++wumpusCount;
            }

            WriteLine("Wumpus Cleaned!");
        }

        public override void Update()
        {

        }

        public void ReceiveCommand(string command)
        {
            commandq.Enqueue(command);
            if(commandLineHistory.Count == 0 || commandLineHistory[commandLineHistory.Count-1] != command)
                commandLineHistory.Add(command);
            ResetCommandIndex();
        }
        public string GetCommandHistoryNext(int step)
        {
            currCommIndex = Mathc.Clamp(currCommIndex + step, 0, commandLineHistory.Count);
            if (currCommIndex == commandLineHistory.Count)
                return string.Empty;
            return commandLineHistory[currCommIndex];
        }
        public string GetCommandHistoryLast()
        {
            return GetCommandHistoryNext(-currCommIndex);
        }
        public void ResetCommandIndex()
        {
            currCommIndex = commandLineHistory.Count;
        }
        public void OnWindowClose(object sender, EventArgs e)
        {
            WindowOpen = false;
        }
        #endregion

        #region Static Methods
        /// <summary> Get a random room on the game board </summary>
        public static BoardRoom GetRandomRoom()
        {
            return instance.gameBoard.rooms[Mathc.Random.NextInt(0, instance.gameBoard.rooms.Count)];
        }
        /// <summary> Get a random room from a selection </summary>
        public static BoardRoom GetRandomRoom(params BoardRoom[] rooms)
        {
            return rooms[Mathc.Random.NextInt(0, rooms.Length)];
        }
        /// <summary> Get a random room connected to a room </summary>
        public static BoardRoom GetRandomConnectedRoom(BoardRoom room)
        {
            return GetRandomRoom(room.GetConnectedRooms());
        }
        /// <summary> Get a random room connected to or contained within a selecion of rooms </summary>
        public static BoardRoom GetRandomConnectedRoom(params BoardRoom[] rooms)
        {
            HashSet<BoardRoom> connectedRooms = new HashSet<BoardRoom>();
            foreach (var room in rooms)
            {
                if (!connectedRooms.Contains(room))
                    connectedRooms.Add(room);
                foreach (var croom in room.GetConnectedRooms())
                    if (!connectedRooms.Contains(croom))
                        connectedRooms.Add(croom);
            }
            return GetRandomRoom(connectedRooms.ToArray());
        }
        /// <summary> Get a random room that IS NOT connected to a room, or the room itself </summary>
        public static BoardRoom GetRandomUnconnectedRoom(BoardRoom room)
        {
            HashSet<int> invalidRooms = new HashSet<int>();

            // Step 1) Gather unwanted values
            invalidRooms.Add(room.index);
            foreach (var connection in room.GetConnectedRooms())
                invalidRooms.Add(connection.index);

            return GetRandomFilteredRoom(invalidRooms);
        }
        /// <summary> Get a random room that IS NOT connected to or contained in a selection of rooms </summary>
        public static BoardRoom GetRandomUnconnectedRoom(params BoardRoom[] rooms)
        {
            HashSet<int> invalidRooms = new HashSet<int>();

            // Step 1) Gather unwanted values
            foreach (var room in rooms)
            {
                invalidRooms.Add(room.index);
                foreach (var connection in room.GetConnectedRooms())
                    invalidRooms.Add(connection.index);
            }

            return GetRandomFilteredRoom(invalidRooms);
        }
        /// <summary> Internal method that uses an index map to filter valid rooms, then returns a random one.  </summary>
        /// <param name="invalidRooms">All the indicies for the rooms you want filtered out.</param>
        private static BoardRoom GetRandomFilteredRoom(HashSet<int> invalidRooms)
        {
            // Step 2) Figure out new range
            int effectiveCount = instance.gameBoard.rooms.Count - invalidRooms.Count;
            int oorIndex = effectiveCount - 2; // -2 because the for loop later increments by one
            Dictionary<int, int> indexMap = new Dictionary<int, int>(invalidRooms.Count);

            // Step 3) Map in-range invalid values to out-of-range valid values
            foreach (var index in invalidRooms)
            {
                // Increment by one so that the previous index last used isn't re-checked.
                for (oorIndex += 1; oorIndex < instance.gameBoard.rooms.Count; ++oorIndex)
                {
                    // If this index is valid, map it to invalid index.
                    if (!invalidRooms.Contains(oorIndex))
                    {
                        indexMap[index] = oorIndex;
                        break;
                    }
                }
            }

            // Step 4) Get the random room!
            int rIndex = Mathc.Random.NextInt(0, effectiveCount);
            if (indexMap.ContainsKey(rIndex))
                rIndex = indexMap[rIndex];

            return instance.gameBoard.rooms[rIndex];
        }

        public static string GetNextCommand()
        {
            if (instance.commandq.Count == 0)
                return null;
            return instance.commandq.Dequeue();
        }
        public static bool GetNextCommand(out string command)
        {
            command = null;
            if (instance.commandq.Count == 0)
                return false;
            command = instance.commandq.Dequeue();
            return true;
        }

        public static void WriteLine(string msg)
        {
            WumpusWindow.AppendToTextBoxSafe(msg, instance.window.consoleOutput);
            WumpusWindow.AppendToTextBoxSafe("\r\n", instance.window.consoleOutput);
        }
        #endregion
    }
}
