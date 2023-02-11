using System;
using System.Collections.Generic;
using System.Text;

namespace RogueCrawler
{
    interface IInspectable
    {
        string BriefString();
        string InspectString(string prefix, int tabCount);
        string DebugString(string prefix, int tabCount);
    }
}
