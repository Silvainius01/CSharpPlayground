using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RogueCrawler
{
    enum CrawlerState { Menu, Game }
    partial class DungeonCrawlerManager
    {
        public static string TextPath = $"{Directory.GetCurrentDirectory()}\\TextDocs";
        public static string SavePath = $"{TextPath}\\Saves";

        public Dungeon dungeon;
        public PlayerCharacter player;

        public static DungeonCrawlerManager Instance { get; private set; }
        public static int PlayerLevel { get => Instance?.player.Level ?? 0; }

        public CrawlerState currentState = CrawlerState.Menu;
        public Dictionary<CrawlerState, BaseCrawlerStateManager> stateManagers = new Dictionary<CrawlerState, BaseCrawlerStateManager>();

        public DungeonCrawlerManager()
        {
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            Instance = this;
            MaterialTypeManager.LoadMaterials();
            WeaponTypeManager.LoadWeaponTypes();
            ArmorTypeManager.LoadArmorTypes();

            stateManagers.Add(CrawlerState.Menu, new CrawlerMenuManager(this));
            stateManagers.Add(CrawlerState.Game, new CrawlerGameManager(this));
            player = new PlayerCharacter();

        }

        public void UpdateLoop()
        {
            CrawlerState nextState = stateManagers[currentState].UpdateCrawlerState();
            if (nextState != currentState)
            {
                stateManagers[currentState].EndCrawlerState();
                stateManagers[nextState].StartCrawlerState();
                currentState = nextState;
            }
        }

        public Dungeon GenerateDungeon(DungeonSize size)
        {
            Vector2Int GetRoomRange()
            {
                switch (size)
                {
                    case DungeonSize.Small: return new Vector2Int(20, 35); // 4x5, 5x5, 5x6, =5x7
                    case DungeonSize.Medium: return new Vector2Int(35, 50); // 5x7, 6x6, 6x7, 6x8, 6x9, 7x7, >7x8
                    case DungeonSize.Large: return new Vector2Int(50, 75); // 7x8, 7x9, 8x8, 8x9, >8x10
                    case DungeonSize.Huge: return new Vector2Int(75, 100); // <8x10, 9x9, 9x10, 9x11, =10x10
                }
                return new Vector2Int(25, 25); // =5x5
            }
            var roomRange = GetRoomRange();

            DungeonGenerationParameters dParams = new DungeonGenerationParameters(3, DungeonGenerator.GetRandomQuality)
            {
                PlayerLevel = player.Level,
                RoomRange = roomRange,
                ConnectionRange = new Vector2Int(1, 3),
                RoomHeightRange = new Vector2Int(1, 1),
                RoomWidthRange = new Vector2Int(1, 1),
                MaxCreaturesPerRoom = 3,
                CreatureProbability = 0,//0.33f,
                ChestProbability = 1//0.2f
            };
            return DungeonGenerator.GenerateDungeon(dParams);
        }

        public void SaveSerializableObject<T, U>(ISerializable<T, U> serializable, string path) where T : ISerialized<U>
        {
            if (!path.EndsWith(".json"))
                path = $"{path}.json";

            using StreamWriter writer = new StreamWriter(path);
            var toSerialize = serializable.GetSerializable();
            writer.Write(JsonConvert.SerializeObject(toSerialize));
            writer.Close();
        }

        public void SaveObject<T>(T obj, string path)
        {
            if (!path.EndsWith(".json"))
                path = $"{path}.json";

            using StreamWriter writer = new StreamWriter(path);
            writer.Write(JsonConvert.SerializeObject(obj));
            writer.Close();
        }

        public T LoadObject<T>(string path)
        {
            if (!path.EndsWith(".json"))
                path = $"{path}.json";

            using StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            reader.Close();

            var serializer = JsonSerializer.CreateDefault();
            var jObject = JsonConvert.DeserializeObject<JObject>(json);
            return (T)serializer.Deserialize(new JTokenReader(jObject), typeof(T));
            //return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
