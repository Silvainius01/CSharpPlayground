using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide
{
    public interface IDataObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public interface ITeamDataObject : IDataObject
    {
        public int FactionId { get; set; }
        public int TeamId { get; set; }
    }
}
