using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide.StatProcessing.StatObjects
{
    public interface IStatObject<T> where T : IDataObject
    {
        T Data { get; set; }
        PlanetStats Stats { get; set; }
    }
}
