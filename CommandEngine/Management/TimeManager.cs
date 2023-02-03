using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public class TimeManager
    {
        private static readonly DateTime startTime = DateTime.Now;
        private static DateTime timeLastFrame;
        private static DateTime timeThisFrame;
        public static int TotalFrames { get; private set; }

        public static float DeltaTime { get; private set; }
        public static float AvgDeltaTime { get; private set; }
        public static double DeltaTime_64 { get; private set; }
        public static double AvgDeltaTime_64 { get; private set; }

        public static int FPS { get { return (int)(DeltaTime / 1); } }
        public static int AvgFPS { get { return (int)(AvgDeltaTime / 1); } }
        
        internal static void Update()
        {
            timeLastFrame = timeThisFrame;
            timeThisFrame = DateTime.Now;
            DeltaTime_64 = (timeThisFrame - timeLastFrame).TotalSeconds;
            DeltaTime = (float)DeltaTime_64;
            ++TotalFrames;

            AvgDeltaTime_64 = ((AvgDeltaTime_64 * (TotalFrames - 1)) + DeltaTime_64) / TotalFrames;
            AvgDeltaTime = (float)AvgDeltaTime_64;
        }
    }
}
