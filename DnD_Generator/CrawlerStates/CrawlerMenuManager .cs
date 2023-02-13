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
    class CrawlerMenuManager : BaseCrawlerStateManager
    {
        CommandModule commands = new CommandModule("\nEnter next menu command");
        SmartStringBuilder staticBuilder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);

        public bool startGame = false;
        public CrawlerMenuManager(DungeonCrawlerManager manager) : base(manager)
        {
            commands.Add(new ConsoleCommand("newGame", NewGame));
            commands.Add(new ConsoleCommand("again", NewDungeon));
            commands.Add(new ConsoleCommand("save", SaveCharacter));
            commands.Add(new ConsoleCommand("load", LoadCharacter));
            commands.Add(new ConsoleCommand("test", TestCommand));
        }

        public override void StartCrawlerState()
        {
            startGame = false;
        }
        public override CrawlerState UpdateCrawlerState()
        {
            commands.NextCommand(true);
            return startGame ? CrawlerState.Game : CrawlerState.Menu;
        }
        public override void EndCrawlerState()
        {
        }

        #region Commands
        public void NewGame(List<string> args)
        {
            crawlerManager.player = CharacterCreator.CharacterCreationPrompt();
            startGame = true;
        }

        public void NewDungeon(List<string> args)
        {
            if (player == null || player.Health <= 0)
            {
                Console.WriteLine("Cannot enter a new dungeon, YOU'RE DEAD.");
                return;
            }
            startGame = true;
        }

        public void SaveCharacter(List<string> args)
        {
            crawlerManager.SaveSerializableObject(player, $"{DungeonCrawlerManager.SavePath}\\{player.Name}");
        }

        public void LoadCharacter(List<string> args)
        {
            if (args.Count < 1)
            {
                ConsoleExt.WriteErrorLine("Must provide a character name.");
                return;
            }

            string saveFilePath = $"{DungeonCrawlerManager.SavePath}\\{args[0]}.json";
            if (!File.Exists(saveFilePath))
            {
                ConsoleExt.WriteErrorLine($"Could not find save for character '{args[0]}'.");
                return;
            }

            StreamReader reader = new StreamReader(saveFilePath);
            string json = reader.ReadToEnd();
            reader.Close();

            var serializer = JsonSerializer.CreateDefault();
            var jObject = JsonConvert.DeserializeObject<JObject>(json);
            var serializedPlayer = (SerializedCharacter)serializer.Deserialize(new JTokenReader(jObject), typeof(SerializedCharacter));

            player = new PlayerCharacter();
            player.Name = serializedPlayer.Name;
            player.Experience = serializedPlayer.Experience;
            player.HitPoints = serializedPlayer.HitPoints;
            player.Level = serializedPlayer.Level;
            player.PrimaryWeapon = DungeonGenerator.GenerateWeaponFromSerialized(serializedPlayer.PrimaryWeapon);
            player.MaxAttributes = serializedPlayer.Attributes.GetDeserialized();
            player.Afflictions = serializedPlayer.Afflictions.GetDeserialized();
            player.Profeciencies = serializedPlayer.Profeciencies.GetDeserialized();

            foreach (var kvp in serializedPlayer.InventoryItems)
            {
                Type type = kvp.Key;
                foreach (JObject obj in kvp.Value)
                {
                    SerializedItem converted = (SerializedItem)serializer.Deserialize(new JTokenReader(obj), type);
                    player.Inventory.AddItem(converted.GetDeserialized());
                }
            }
        }

        public void TestCommand(List<string> args)
        {
            int tabCount = 0;
            staticBuilder.Clear();
            staticBuilder.Append(tabCount, "Available characters to load:");
            tabCount++;
            foreach(var filePath in Directory.GetFiles(DungeonCrawlerManager.SavePath))
            {
                var parts = filePath.Split('/', '\\', '.');
                if(parts[parts.Length-1] == "json")
                    staticBuilder.NewlineAppend(tabCount, parts[parts.Length-2]);
            }
            Console.WriteLine(staticBuilder.ToString());
        }
        #endregion
    }
}
