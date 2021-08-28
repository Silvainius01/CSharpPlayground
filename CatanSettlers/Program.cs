using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GameEngine;

namespace CatanSettlers
{
    class HexGridGenerator
    {
        int layerCount = 0;
        List<HexTile> allTiles = new List<HexTile>();
        Dictionary<int, List<HexTile>> tileLayerDict = new Dictionary<int, List<HexTile>>();

        public void GenerateGridCirc(int numLayers)
        {
            Queue<HexTile> q = new Queue<HexTile>();

            this.layerCount = numLayers;
            allTiles.Add(new HexTile(Vector2.Zero, 0, 0));
            q.Enqueue(allTiles[0]);

            // numLayers-1, as this algo will create up to the INDEX, instead of the COUNT.
            for (int i = 0; i < numLayers-1; ++i)
            {
                int qLimit = q.Count;
                for (int j = 0; j < qLimit; ++j)
                {
                    HexTile currTile = q.Dequeue();
                    FillTileConnections(currTile, in q);
                }
            }
        }

        public void AddLayer()
        {
            if(layerCount <= 0)
            {
                layerCount = 1;
                allTiles.Add(new HexTile(Vector2.Zero, 0, 0));
                return;
            }

            Queue<HexTile> q = new Queue<HexTile>(allTiles.Where(tile => tile.layer == layerCount - 1));

            foreach(var tile in allTiles.Where(tile => tile.layer == layerCount - 1))
            {
                FillTileConnections(tile, in q);
            }
        }

        private void FillTileConnections(HexTile currTile, in Queue<HexTile> q)
        {
            // Create 0th tile if it does not exist
            if (currTile.connections[0] == null)
            {
                HexTile newTile = (currTile.CreateConnectedTile(0, allTiles.Count));
                q.Enqueue(newTile);
                allTiles.Add(newTile);
            }

            // Create and/or connect tiles around currTile
            for (int i = 1; i < currTile.connections.Length; ++i)
            {
                if (currTile.connections[i] == null)
                {
                    HexTile newTile = (currTile.CreateConnectedTile(i, allTiles.Count));
                    q.Enqueue(newTile);
                    allTiles.Add(newTile);
                }

                // Connect to the tile around the parent that is "behind" it
                HexTile.Connect((i + 4) % 6, currTile.connections[i], currTile.connections[i - 1]);
            }

            // Connect first tile to last tile.
            HexTile.Connect(4, currTile.connections[0], currTile.connections[5]);
        }

        private void AddTile(HexTile hexTile)
        {
            if (!tileLayerDict.ContainsKey(hexTile.layer))
                tileLayerDict.Add(hexTile.layer, new List<HexTile>());
            tileLayerDict[hexTile.layer].Add(hexTile);
            allTiles.Add(hexTile);

        }

        public string GetBoardInfo()
        {
            StringBuilder msg = new StringBuilder($"Num Layers: {layerCount}\n");
            StringBuilder tmsg = new StringBuilder();
            Dictionary<int, int> layerDict = new Dictionary<int, int>();
                       
            foreach(var tile in allTiles)
            {
                if (!layerDict.ContainsKey(tile.layer))
                    layerDict.Add(tile.layer, 1);
                else ++layerDict[tile.layer];
                tmsg.Append(tile.GetConnectionSummary());
            }

            foreach (var kvp in layerDict)
                msg.Append($"\nLayer {kvp.Key}: {kvp.Value}");

            msg.Append($"\n\nTile Summary:\n");
            msg.Append(tmsg.ToString());
            return msg.ToString();
        }
    }

    class HexTile
    {
        public int layer;
        public int tileNum;
        public Vector2 pos;
        public HexTile[] connections = new HexTile[6];

        public HexTile(Vector2 position, int layer, int tileNum) 
        {
            pos = position;
            this.layer = layer;
            this.tileNum = tileNum;
        }

        public static void Connect(int cIndex, HexTile firstTile, HexTile secondTile)
        {
            firstTile.connections[cIndex] = secondTile;
            secondTile.connections[GetOppIndex(cIndex)] = firstTile;
        }

        static int GetOppIndex(int index) { return (index + 3) % 6; }

        public HexTile CreateConnectedTile(int cIndex, int tileNum)
        {
            double ang = ((Mathc.TwoPi / 6) * cIndex) + Mathc.HalfPi; // Yay radians
            Vector2 pos = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang));
            HexTile newTile = new HexTile(pos, this.layer+1, tileNum);
            Connect(cIndex, this, newTile);
            return newTile;
        }

        public string GetConnectionSummary()
        {
            StringBuilder msg = new StringBuilder($"\n {tileNum}[{layer}]: ");

            for (int i = 0; i < 6; ++i)
            {
                if (connections[i] == null)
                {
                    msg.Append($"[{i}, null] ");
                    continue;
                }

                Vector2 nPos = connections[i].pos - pos;
                double ang = Math.Atan2(nPos.Y, nPos.X);
                ang = Mathc.AnglePiToAngle2Pi(ang) * Mathc.RAD2DEG;
                msg.Append($"[{i}, {ang.ToString("F1")}, {nPos.ToString("F2")}] ");
            }

            return msg.ToString();
        }
    }

    enum TerrainType
    {
        DESERT,
        WOOD,
        CLAY,
        WHEAT,
        SHEEP,
        STONE,
        WATER,
        PORT_MISC,
        PORT_wOOD,
        PORT_CLAY,
        PORT_WHEAT,
        PORT_SHEEP,
        PORT_STONE
    }

    class Test
    {
        public int Test0 { get; set; }
        public int Test1 { get; set; }
        public double Test2 { get; set; }
        public double Test3 { get; set; }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                ["Test0"] = "0",
                ["Test1"] = "0.0"
            };

            string v = string.Empty;
            var t = new Test()
            {
                Test0 = dict.TryGetValue("Test", out v) ? int.Parse(v) : -1,
                Test1 = dict.TryGetValue("Test0", out v) ? int.Parse(v) : -1,
                Test2 = dict.TryGetValue("Test", out v) ? double.Parse(v) : -1,
                Test3 = dict.TryGetValue("Test1", out v) ? double.Parse(v) : -1
            };

            Console.ReadLine();
        }

        static void RedditPmFillerCommandGenerator()
        {
            TextLoader users = new TextLoader(@"c:\users\v-anad\Desktop\RedditGiveAwayWinnersList.txt");
            TextLoader codes = new TextLoader(@"c:\users\v-anad\Desktop\GiveAwayCodes.txt");

            for (int i = 141; i < 1000; ++i)
            {
                StringBuilder msg = new StringBuilder();

                // Setter for body
                msg.Append("document.getElementsByName(\"text\")[1].value = \"");
                msg.Append($"Hi u/{users.lines[i]},");
                msg.Append($"\\n\\nYou commented on the [Collidalot Give Away](https://www.reddit.com/r/NintendoSwitch/comments/euy3pu/giveaway_i_have_500_codes_for_collidalot_a/) I posted last week, and have been selected as a winner! Here is your code:");
                msg.Append($"\\n\\n{codes.lines[i]}");
                msg.Append($"\\n\\nHave fun dominating the wasteland!");
                msg.Append($"\\n\\n- Connor \\\"Silvainius\\\" Adam");
                msg.Append($"\\n\\nHere are some statistics, in case it interests you:");
                msg.Append($"\\n\\n- Placement: {i + 1}");
                msg.Append($"\\n- Number of qualified users: ~3100");
                msg.Append($"\\n- Chance of selection: {((1000.0 / 3100.0) * 100).ToString("F3")}%");
                msg.Append($"\";\n");

                // Set subject
                msg.Append("document.getElementsByName(\"subject\")[0].value = \"");
                msg.Append("Collidalot Code");
                msg.Append("\";\n");

                //Set To
                msg.Append("document.getElementsByName(\"to\")[0].value = \"");
                msg.Append($"{users.lines[i]}");
                msg.Append("\";\n");

                msg.Append("document.getElementById(\"send\").click();");

                Console.WriteLine(msg.ToString());
                Console.ReadLine();
                Console.Clear();
                msg.Clear();
            }
        }

        static void HexGridGubbins()
        {
            HexGridGenerator generator = new HexGridGenerator();
            List<TerrainType> TerrainDeck = new List<TerrainType>(19)
            {
                TerrainType.DESERT,
                TerrainType.WOOD, TerrainType.WOOD, TerrainType.WOOD, TerrainType.WOOD,
                TerrainType.CLAY, TerrainType.CLAY, TerrainType.CLAY,
                TerrainType.WHEAT, TerrainType.WHEAT, TerrainType.WHEAT, TerrainType.WHEAT,
                TerrainType.SHEEP, TerrainType.SHEEP, TerrainType.SHEEP, TerrainType.SHEEP,
                TerrainType.STONE, TerrainType.STONE, TerrainType.STONE,
                TerrainType.WOOD, TerrainType.WOOD, TerrainType.WOOD, TerrainType.WOOD,
            };

            generator.GenerateGridCirc(2);
            Console.WriteLine(generator.GetBoardInfo());
        }
    }
}
