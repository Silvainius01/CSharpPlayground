using System;
using System.Text.Json;

namespace PlanetSide
{
    public static class JsonElementExtensions
    {
        public static bool TryGetStringElement(this JsonElement payload, string name, out string value)
        {
            if (payload.TryGetProperty(name, out var childElement))
            {
                value = childElement.GetString();// ?? throw new NullReferenceException("Received Null Char Id");
                return true;
            }

            value = "InvalidResponse";
            return false;
        }

        public static bool TryGetCensusInteger(this JsonElement payload, string name, out int value)
        {
            if(payload.TryGetProperty(name, out var childElement))
            {
                value = int.Parse(childElement.GetString());
                return true;
            }

            value = 0;
            return false;
        }

        public static bool TryGetCensusFloat(this JsonElement payload, string name, out float value)
        {
            if (payload.TryGetProperty(name, out var childElement))
            {
                value = float.Parse(childElement.GetString());
                return true;
            }

            value = 0;
            return false;
        }

        public static bool TryGetCensusBool(this JsonElement payload, string name, out bool value)
        {
            if (payload.TryGetProperty(name, out var childElement))
            {
                value = childElement.GetString().Equals("1");
                return true;
            }

            value = false;
            return false;
        }
    }
}
