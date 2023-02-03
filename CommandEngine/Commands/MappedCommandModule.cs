using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public class MappedCommandModule<T> : CommandModule<T>
    {
        public delegate void OnSuccessDelegate(List<string> args, T value);

        public MappedCommandModule(string defaultPrompt, Dictionary<string, T> mappedValues) : base(defaultPrompt)
        {
            commands = new Dictionary<string, ConsoleCommand<T>>(mappedValues.Count);

            foreach(var kvp in mappedValues)
            {
                var value = kvp.Value;
                string valueString = kvp.Key;
                ConsoleCommand<T> command = new ConsoleCommand<T>(
                    valueString,
                    (List<string> args, out T result) => {
                        result = value;
                        return true;
                    }
                );
                commands.Add(valueString, command);
            }
        }
    }
}
