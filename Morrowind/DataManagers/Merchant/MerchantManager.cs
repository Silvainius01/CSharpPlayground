using CommandEngine;
using CommandEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morrowind
{
    class MerchantManager : TypeManager<AlchemyMerchant, MerchantManager>
    {
        public static List<AlchemyMerchant> MerchantList { get; private set; } = new();
        public static Dictionary<string, AlchemyMerchant> MerchantDict { get; private set; } = new();

        protected override void AddTypeEntry(AlchemyMerchant item)
        {
            if (!MerchantDict.ContainsKey(item.Name))
            {
                MerchantList.Add(item);
                MerchantDict.Add(item.Name, item);
            }
            else ConsoleExt.WriteWarningLine($"SpellEffect with name {item.Name} already exists.");
        }

        protected override string GetDataPath()
            => $"{Directory.GetCurrentDirectory()}\\Data\\Merchants.json";

        protected override List<AlchemyMerchant> GetDefaultTypesInternal()
        {
            if (_IsLoaded)
                return MerchantList;

            // Damage attribute base = 8
            // Drain attribute base = 1
            // Restore attribute base = 8
            // Fortify attribute base = 1

            // Drain skill base = 1

            List<AlchemyMerchant> merchants = new List<AlchemyMerchant>(32)
            {
                new AlchemyMerchant("Ajira")
                {
                    City = "Balmora",
                    Location = "Guild of Mages",
                    Gold = 800,
                    Ingredients = new List<string>(8)
                    {
                        "Black Anther",
                        "Comberry",
                        "Crab Meat",
                        "Heather",
                        "Hound Meat",
                        "Kwamma Cuttle",
                        "Scales",
                        "Small Kwama Egg",
                        "Willow Anther",
                    }
                },
                new AlchemyMerchant("Anarenen")
                {
                    City= "Ald'ruhn",
                    Location = "Guild of Mages",
                    Gold = 400,
                    Ingredients = new List<string>(8)
                    {
                        "Bloat",
                        "Bonemeal",
                        "Comberry",
                        "Crab Meat",
                        "Diamond",
                        "Fire Salts",
                        "Heather",
                        "Vampire Dust",
                    }
                },
                new AlchemyMerchant("Andil")
                {
                    City = "Tel Vos",
                    Location = "Services Tower",
                    Gold = 400,
                    Ingredients = new List<string>(8)
                    {
                        "Black Anther",
                        "Gold Kanet",
                        "Kresh Fiber",
                        "Marshmerrow",
                        "Scathecraw",
                        "Shalk Resin",
                        "Stoneflower Petals",
                    },
                },
                new AlchemyMerchant("Andilu Drothan")
                {
                    City = "Vivec - Foreign Quarter",
                    Location = "Andilu Drothan: Alchemist",
                    Gold = 200,
                    Ingredients = new List<string>(8)
                    {
                        "Comberry",
                        "Gold Kanet",
                        "Heather",
                        "Muck",
                        "Scrap Metal",
                        "Sload Soap",
                        "Trama Root",
                    }
                },
                new AlchemyMerchant("Anis Seloth")
                {
                    City = "Sadrith Mora",
                    Location = "Anis Seloth: Alchemist",
                    Gold = 800,
                    Ingredients = new List<string>(8)
                    {
                        "Alit Hide",
                        "Bloat",
                        "Bonemeal",
                        "Coda Flower",
                        "Daedra Skin",
                        "Daedra's Heart",
                        "Diamond",
                        "Fire Salts",
                        "Frost Salts",
                        "Ghoul Heart",
                        "Hackle-Lo Leaf",
                        "Heather",
                        "Hypha Facia",
                        "Kwama Cuttle",
                        "Luminous Russula",
                        "Muck",
                        "Pearl",
                        "Ruby",
                        "Scales",
                        "Scrap Metal",
                        "Sload Soap",
                        "Trama Root",
                        "Vampire Dust",
                        "Wickwheat",
                        "Willow Anther",
                    }
                },
                new AlchemyMerchant("Arnand Liric")
                {
                    City = "Buckmoth Legion Fort",
                    Location = "Interior",
                    Gold = 250,
                    Ingredients = new List<string>(8)
                    {
                        "Corprus Weepings",
                        "Gravedust",
                        "Hound Meat",
                        "Netch Leather",
                        "Scamp Skin",
                        "Scrib Jelly",
                    }
                },
                new AlchemyMerchant("Arille")
                {
                    City = "Seyda Neen",
                    Location = "Arille's Tradehouse",
                    Gold = 800,
                    Ingredients = new List<string>(8)
                    {
                        "Corkbulb Root",
                        "Scrib Jerky",
                        "Shalk Resin",
                    }
                },
                new AlchemyMerchant("Aunius Autrus")
                {
                    City = "Wolverine Hall",
                    Location = "Imperial Shrine",
                    Gold = 200,
                    Ingredients = new List<string>(8)
                    {
                        "Ash Yam",
                        "Bloat",
                        "Gravedust",
                        "Guar Hide",
                        "Netch Leather",
                        "Scamp Skin",
                        "Scrib Jelly",
                    }
                },
                new AlchemyMerchant("Aurane Frernis")
                {
                    City = "",
                    Location = "Arille's Tradehouse",
                    Gold = 300,
                    Ingredients = new List<string>(8)
                    {
                        "Black Anther",
                        "Coda Flower",
                        "Corprus Weepings",
                        "Green Lichen",
                        "Hypha Facia",
                        "Red Lichen",
                        "Resin",
                        "Shalk Resin",
                    }
                },
                new AlchemyMerchant("Bilren Areleth")
                {
                    City = "Tel Aruhn",
                    Location = "Bildren Areleth: Apothecary",
                    Gold = 325,
                    Ingredients = new List<string>(8)
                    {
                        "Bittergreen Petals",
                        "Ectoplasm",
                        "Heather",
                        "Kagouti Hide",
                        "Kresh Fiber",
                        "Resin",
                        "Stoneflower Petals",
                    }
                },
                new AlchemyMerchant("Brarayni Sarys")
                {
                    City = "Tel Aruhn",
                    Location = "Tower Entrey",
                    Gold = 450,
                    Ingredients = new List<string>(8)
                    {
                        "Black Anther",
                        "Coda Flower",
                        "Daedra Skin",
                        "Diamond",
                        "Ghoul Heart",
                        "Heather",
                        "Muck",
                        "Racer Plumes",
                        "Sload Soap",
                        "Spore Pod",
                        "Vampire Dust",
                    }
                },
                new AlchemyMerchant("Chaplain Ogrul")
                {
                    City = "Gnisis",
                    Location = "Fort Darius",
                    Gold = 250,
                    Ingredients = new List<string>(8)
                    {
                        "Ash Yam",
                        "Crab Meat",
                        "Marshmerrow",
                        "Rat Meat",
                        "Saltrice",
                        "Wickwheat",
                    }
                },
                new AlchemyMerchant("Cienne Sintieve")
                {
                    City = "Ald'ruhn",
                    Location = "Cienne Sintieve: Alchemist",
                    Gold = 300,
                    Ingredients = new List<string>(8)
                    {
                        "Ampoule Pod",
                        "Coda Flower",
                        "Frost Salts",
                        "Hound Meat",
                        "Kwama Cuttle",
                        "Racer Plumes",
                        "Scrap Metal",
                        "Small Kwama Egg",
                        "Spore Pod",
                        "Violet Coprinus",
                        "Void Salts",
                    }
                },
                new AlchemyMerchant("Cocistian Quaspus")
                {
                    City = "Buckmoth Legion Fort",
                    Location = "",
                    Gold = 200,
                    Ingredients = new List<string>(8)
                    {
                        "Bittergreen Petals",
                        "Emerald",
                        "Fire Petal",
                        "Kresh Fiber",
                        "Rat Meat",
                        "Scathe Craw",
                        "Stoneflower Petals",
                    }
                },
                new AlchemyMerchant("Craetia Jullalian")
                {
                    City = "Vivec - Foreign Quarter",
                    Location = "Guild of Mages",
                    Gold = 400,
                    Ingredients = new List<string>(8)
                    {
                        "Alit Hide",
                        "Ampoule Pod",
                        "Bloat",
                        "Corkbulb Root",
                        "Crab Meat",
                        "Daedra's Heart",
                        "Diamond",
                        "Emerald",
                        "Fire Salts",
                        "Frost Salts",
                        "Ghoul Heart",
                        "Pearl",
                        "Race Plumes",
                        "Ruby",
                    }
                },
                new AlchemyMerchant("Danoso Andrano")
                {
                    City = "Ald'ruhn",
                    Location = "Temple",
                    Gold = 300,
                    Ingredients = new List<string>(8)
                    {
                        "Bungler's Bane",
                        "Coda Flower",
                        "Dreugh Wax",
                        "Ectoplasm",
                        "Hypha Facia",
                        "Marshmerrow",
                        "Pearl",
                        "Roobrush",
                        "Spore Pod",
                        "Wickwheat",
                    }
                },
                new AlchemyMerchant("Daynali Dren")
                {
                    City = "Tel Mora",
                    Location = "Lower Tower",
                    Gold = 3999,
                    Ingredients = new List<string>(8)
                    {
                        "Alit Hide",
                        "Black Anther",
                        "Bloat",
                        "Crab Meat",
                        "Diamond",
                        "Frost Salts",
                        "Gold Kanet",
                        "Hackle-Lo Leaf",
                        "Hound Meat",
                        "Racer Plumes",
                        "Ruby",
                        "Scales",
                        "Scrap Metal",
                        "Trama Root",
                    } 
                },
                new AlchemyMerchant("Dralval Andrano")
                {
                    City = "Balmora",
                    Location = "Temple",
                    Gold = 250,
                    Ingredients = new List<string>(8)
                    {
                        "Black Lichen",
                        "Bungler's Bane",
                        "Green Lichen",
                        "Hypha Facia",
                        "Kagouti Hide",
                        "Marshmerrow",
                        "Red Lichen",
                        "Scathecraw",
                        "Wickwheat",
                    }
                },
                new AlchemyMerchant("Dulian")
                {
                    City = "Buckmoth Legion Fort",
                    Location = "Interior",
                    Gold = 200,
                    Ingredients = new List<string>(8)
                    {
                        "Ash Yam",
                        "Kwama Cuttle",
                        "Muck",
                        "Resin",
                        "Scrib Jerky",
                        "Trama Root",
                        "Willow Anther",
                    }
                },
                new AlchemyMerchant("Eldrilu Dalen")
                {
                    City = "Vos",
                    Location = "Vos Chapel",
                    Gold = 250,
                    Ingredients = new List<string>(8)
                    {
                        "Ash Salts",
                        "Ash Yam",
                        "Corkbulb Root",
                        "Fire Salts",
                        "Guar Hide",
                        "Netch Leather",
                        "Saltrice",
                        "Scrib Jelly",
                        "Scrib Jerky",
                    }
                },
            };

            return merchants;
        }
    }
}
