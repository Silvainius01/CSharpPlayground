using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    interface IInspectable
    {
        string BriefString();
        string InspectString(string prefix, int tabCount);
        string DebugString(string prefix, int tabCount);
    }
}
