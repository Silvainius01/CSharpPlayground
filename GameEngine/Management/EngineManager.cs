using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GameEngine
{
    public class EngineManager
    {
        private static ulong __nextId__ = 0;
        internal static ulong NextId { get { return __nextId__++; } }

        static List<int> activeLayers = new List<int>();
        static Dictionary<int, Entity> entityDict = new Dictionary<int, Entity>();
        static Dictionary<int, HashSet<Entity>> layeredEntityDict = new Dictionary<int, HashSet<Entity>>();
        internal static Dictionary<int, List<Component>> newComponents = new Dictionary<int, List<Component>>();

        public static void FrameUpdate()
        {
            TimeManager.Update();
            EntityUpdate();
        }

        internal static void SetEntityLayer(Entity ent, int layer)
        {
            // Remove entity from it's current layer
            if (layeredEntityDict.ContainsKey(ent.Layer) && layeredEntityDict[ent.Layer].Contains(ent))
                layeredEntityDict[ent.Layer].Remove(ent);
         
            // If the new layer doesn't exist
            if (!layeredEntityDict.ContainsKey(layer))
            {
                layeredEntityDict.Add(layer, new HashSet<Entity>());
                newComponents.Add(layer, new List<Component>());
                activeLayers.Add(layer);
                activeLayers.Sort();
            }
            layeredEntityDict[layer].Add(ent);
        }

        internal static void EntityUpdate()
        {
            for(int i = 0; i < activeLayers.Count; ++i)
            {
                int layer = activeLayers[i];

                foreach (var c in newComponents[layer])
                    c.Start();
                newComponents[layer].Clear();
                foreach (var ent in layeredEntityDict[layer])
                    ent.Update();
            }
        }
    }
}