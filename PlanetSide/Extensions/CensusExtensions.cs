using DaybreakGames.Census.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public static class CensusExtensions
    {
        /// <summary>
        /// Merge character, world, and event filteres from one subscription into this one.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="other"></param>
        public static void Merge(this CensusStreamSubscription subscription, CensusStreamSubscription other)
        {
            subscription.Characters = subscription.Characters.Union(other.Characters).ToArray();
            subscription.Worlds = subscription.Worlds.Union(other.Worlds).ToArray();
            subscription.EventNames = subscription.EventNames.Union(other.EventNames).ToArray();
        }
    }
}
