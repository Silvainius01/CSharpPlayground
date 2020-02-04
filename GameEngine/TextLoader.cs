using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GameEngine
{
    public class TextLoader
    {
        public List<string> lines;

        public TextLoader(string path)
        {
            lines = File.ReadAllLines(path).ToList();
        }
    }
}
