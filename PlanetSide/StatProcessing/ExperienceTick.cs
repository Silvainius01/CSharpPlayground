using System;

namespace PlanetSide
{
    public struct ExperienceTick
    {
        public int Id;
        public string Name;
        public float ScoreAmount;

        public override string ToString()
        {
            return $"{Name} ({ScoreAmount})";
        }
    }

    public class CumulativeExperience
    {
        public int Id;
        public int NumEvents;
        public float CumulativeScore;
        public string Name;

        public override string ToString()
        {
            return Name;
        }
    }
}
