using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Morrowind
{
    class AlchemyMerchant
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Location { get; set; }
        public int Gold { get; set; }
        public List<string> Ingredients { get; set; }

        public AlchemyMerchant(string name) { Name = name; }
    }
}
