using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;

namespace CatanSettlers
{
    class BoardGenerator
    {
        int layerCount = 0;
        List<HexTile> tiles = new List<HexTile>();
        public void GenerateGridCirc(int numLayers)
        {
            Queue<HexTile> q = new Queue<HexTile>();

            this.layerCount = numLayers;
            tiles.Add(new HexTile(Vector2.Zero, 0, 0));
            q.Enqueue(tiles[0]);

            // numLayers-1, as this algo will create up to the INDEX, instead of the COUNT.
            for (int i = 0; i < numLayers-1; ++i)
            {
                int qLimit = q.Count;
                for (int j = 0; j < qLimit; ++j)
                {
                    HexTile currTile = q.Dequeue();

                    // Create 0th tile if it does not exist
                    if (currTile.connections[0] == null)
                    {
                        HexTile newTile = (currTile.CreateConnectedTile(0, tiles.Count));
                        q.Enqueue(newTile);
                        tiles.Add(newTile);
                    }

                    // Create and/or connect tiles around currTile
                    for(int k = 1; k < currTile.connections.Length; ++k)
                    {
                        if(currTile.connections[k] == null)
                        {
                            HexTile newTile = (currTile.CreateConnectedTile(k, tiles.Count));
                            q.Enqueue(newTile);
                            tiles.Add(newTile);
                        }

                        // Connect to the tile around the parent that is "behind" it
                        HexTile.Connect((i + 4) % 6, currTile.connections[k], currTile.connections[k - 1]);
                    }

                    // Connect first tile to last tile.
                    HexTile.Connect(4, currTile.connections[0], currTile.connections[5]);
                }
            }
        }


        public string GetBoardInfo()
        {
            StringBuilder msg = new StringBuilder($"Num Layers: {layerCount}\n");
            StringBuilder tmsg = new StringBuilder();
            Dictionary<int, int> layerDict = new Dictionary<int, int>();
                       
            foreach(var tile in tiles)
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
            double ang = ((Mathc.TWO_PI / 6) * cIndex) + Mathc.HALF_PI; // Yay radians
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


    class Program
    {
        static void Main(string[] args)
        {
            BoardGenerator generator = new BoardGenerator();

            generator.GenerateGridCirc(2);
            Console.WriteLine(generator.GetBoardInfo());
            Console.ReadLine();
        }
    }
}
