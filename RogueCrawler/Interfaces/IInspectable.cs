using CommandEngine;
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

        ColorStringBuilder BriefColor(ConsoleColor initialColor = ConsoleColor.Gray);
        ColorStringBuilder InspectColor(string prefix, int tabCount, ConsoleColor initialColor = ConsoleColor.Gray);
        ColorStringBuilder DebugColor(string prefix, int tabCount, ConsoleColor initialColor = ConsoleColor.Gray);
    }
}
