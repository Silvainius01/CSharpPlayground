﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommandEngine;

namespace RogueCrawler
{
    enum PathingType { Patrol, Loop, Random }
    class DungeonCreatureManager : DungeonObjectManager<Creature>
    {
        public DungeonCreatureManager(DungeonRoomManager rm) : base(rm) { }


    }
}
