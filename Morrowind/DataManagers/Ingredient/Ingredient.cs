using CommandEngine;
using CommandEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Morrowind.Data
{
    class Ingredient
    {
        public string Name { get; set; }
        public string DataSet { get; set; }
        public int Value { get; set; }
        public float Weight { get; set; }
        public List<string> Effects { get; set; }

        public Ingredient(string name, string dataSet) { Name = name; DataSet = dataSet; }
    }
}
