using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandEngine;

namespace StarbaseTesting
{
    class ThrusterStats
    {
        public int RawThrust { get; }
        public float BaseMass { get; }
        public float FramedMass { get; }
        /// <summary> BaseThrust is the effective produced thrust when accounting for the mass of the thruster itself.</summary>
        public float BaseThrust { get; }

        public ThrusterStats(int thrust, float baseMass)
        {
            float maxSpeedMult = StarbaseShipDesignHelper.MaxSpeedThrustWeightRatio;
            RawThrust = thrust;
            BaseMass = baseMass;
            BaseThrust = RawThrust - (BaseMass * maxSpeedMult);
        }
    }

    class StarbaseShipDesignHelper
    {
        static readonly ThrusterStats BoxThrusterTier1 = new ThrusterStats(
            thrust: 550000,
            baseMass: 19114.3f);
        static readonly ThrusterStats BoxThrusterTier2 = new ThrusterStats(
            thrust: 550000,
            baseMass: 19114.3f);
        static readonly ThrusterStats BoxThrusterTier3 = new ThrusterStats(
            thrust: 550000,
            baseMass: 19114.3f);

        static readonly ThrusterStats TriThrusterTier1 = new ThrusterStats(
            thrust: 330000,
            baseMass: 19114.3f);
        static readonly ThrusterStats TriThrusterTier2 = new ThrusterStats(
            thrust: 330000,
            baseMass: 11317.2f);
        static readonly ThrusterStats TriThrusterTier3 = new ThrusterStats(
            thrust: 330000,
            baseMass: 19114.3f);

        public const int MaxShipSpeed = 150;
        public const float MaxSpeedThrustWeightRatio = 5f;
        public const int VirtualMassPerCrate = 26400;

        public CommandModule sscCommands = new CommandModule(
            defaultPrompt: "\nEnter SSC Command",
            invalidMsg: "Not an SSC command."
            );

        public StarbaseShipDesignHelper()
        {
            sscCommands.Add("thrustCalc", StarbaseShipDesignHelper.ShipWeightThrustTest);
        }



        static void ShipWeightThrustTest(List<string> args)
        {
            if (args.Count < 1 || !float.TryParse(args[0], out float shipWeight))
            {
                ConsoleExt.WriteErrorLine("The first argument of this command must be a valid ship weight.");
                return;
            }

            int numCrates = 0;
            int numTri = 0;
            int numBox = 0;
            

            for (int i = 1; i < args.Count - 1; ++i)
            {
                string arg = args[i];
                string num = args[++i];
                bool validValue = float.TryParse(num, out float amt);

                if (!validValue)
                {
                    ConsoleExt.WriteLine($"Value '{num}' is not a valid number, skipping entry '{arg}'", ConsoleColor.Yellow);
                    continue;
                }

                switch (arg)
                {
                    case "-c":  // Num crates
                        numCrates = (int)amt;
                        break;
                    case "-t": // Desired thruster tier
                        break;
                    case "-nt": // Number of triangle thrusters
                        numTri = (int)amt;
                        break;
                    case "-nb": // Number of box thrusters
                        numBox = (int)amt;
                        break;
                    case "-np": break; // Number of base plasma thrusters
                    case "-nr": break; // Number of plasma thruster rings
                }
            }

            float shipWeightFull = shipWeight + (numCrates * VirtualMassPerCrate);
            float currentThrust = numTri * TriThrusterTier2.RawThrust + numBox * BoxThrusterTier2.RawThrust;

            float neededThrustEmpty = (shipWeight * MaxSpeedThrustWeightRatio);
            float neededThrustFull = (shipWeightFull * MaxSpeedThrustWeightRatio);

            float maxSpeedEmpty = (currentThrust / neededThrustEmpty) * MaxShipSpeed;
            float maxSpeedFull = (currentThrust / neededThrustFull) * MaxShipSpeed;

            // Adjust needed thrust to account for current thrusters.
            neededThrustFull = Math.Max(0, neededThrustFull - currentThrust);
            neededThrustEmpty = Math.Max(0, neededThrustEmpty - currentThrust);

            int tabCount = 0;
            SmartStringBuilder sb = new SmartStringBuilder("  ");
            sb.AppendNewline(tabCount, "Ship Thrust Profile:");
            ++tabCount;
            sb.AppendNewline(tabCount, $"Weight: ");
            ++tabCount;
            sb.AppendNewline(tabCount, $"Empty: {shipWeight.ToString("N")}");
            sb.AppendNewline(tabCount, $"Full : {shipWeightFull.ToString("N")}");
            --tabCount;
            sb.AppendNewline(tabCount, $"Current Speeds:");
            ++tabCount;
            sb.AppendNewline(tabCount, $"Empty: {maxSpeedEmpty.ToString("N")}");
            sb.AppendNewline(tabCount, $"Full : {maxSpeedFull.ToString("N")}");
            --tabCount;
            sb.AppendNewline(tabCount, $"Thrust Needed:");
            ++tabCount;
            sb.AppendNewline(tabCount, $"Empty: {neededThrustEmpty.ToString("N")}");
            sb.AppendNewline(tabCount, $"Full : {neededThrustFull.ToString("N")}");
            --tabCount;
            sb.AppendNewline(tabCount, $"Tier 2 Thrusters Needed:");
            ++tabCount;
            sb.AppendNewline(tabCount, $"Box: {Math.Ceiling(neededThrustEmpty / BoxThrusterTier2.BaseThrust)} - {Math.Ceiling(neededThrustFull / BoxThrusterTier2.BaseThrust)}");
            sb.AppendNewline(tabCount, $"Tri: {Math.Ceiling(neededThrustEmpty / TriThrusterTier2.BaseThrust)} - {Math.Ceiling(neededThrustFull / TriThrusterTier2.BaseThrust)}");
            --tabCount;
            Console.WriteLine(sb.ToString());
        }
    }
}

// thrustCalc 5199080.5 -c 205 -nb 24 -nt 40
