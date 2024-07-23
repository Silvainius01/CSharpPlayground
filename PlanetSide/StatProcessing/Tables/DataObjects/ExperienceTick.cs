using System;

namespace PlanetSide
{
    public struct ExperienceTick : IDataObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float ScoreAmount { get; set; }

        public override string ToString()
        {
            return $"{Name} ({ScoreAmount})";
        }
    }

    // Must be a class since being a ref type makes it so much damn easier to use in the back end.
    public class CumulativeExperience : IDataObject
    {
        public int Id { get; set; }
        public int NumEvents { get; set; }
        public float CumulativeScore {  get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
