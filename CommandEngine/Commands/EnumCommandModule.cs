using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine
{
    public class EnumCommandModule<TEnum> : CommandModule<TEnum> where TEnum : struct, Enum
    {
        public delegate void OnSuccessDelegate(List<string> args, TEnum value);
        
        public bool AllowsIntegerShortcuts { get => integerShortcuts; set => SetAllowIntegerShortcuts(value); }

        bool integerShortcuts = false;
        bool shortcutsCreated = false;
        OnSuccessDelegate OnSuccessCallback;
        //Dictionary<string, ConsoleCommand<TEnum>> enumCommands;

        public EnumCommandModule(string defaultPrompt, bool allowIntegerShortcuts) : base(defaultPrompt)
        {
            var enumValues = EnumExt<TEnum>.Values;

            OnSuccessCallback = OnSuccess;
            AllowsIntegerShortcuts = allowIntegerShortcuts;
            commands = new Dictionary<string, ConsoleCommand<TEnum>>(enumValues.Length * (allowIntegerShortcuts ? 2 : 1));

            for (int i = 0; i < enumValues.Length; i++)
            {
                var value = EnumExt<TEnum>.Values[i];
                string valueString = EnumExt<TEnum>.Names[i];
                ConsoleCommand<TEnum> command = new ConsoleCommand<TEnum>(
                    valueString,
                    (List<string> args, out TEnum result) => { 
                        OnSuccessCallback(args, value); 
                        result = value; 
                        return true; 
                    }
                );
                commands.Add(valueString, command);
            }

            if (allowIntegerShortcuts)
                CreateIntegerShortcuts();
        }
        public EnumCommandModule(string defaultPrompt, bool allowIntegerShortcuts, OnSuccessDelegate OnSuccessCallback) : this(defaultPrompt, allowIntegerShortcuts)
        {
            this.OnSuccessCallback = OnSuccessCallback;
        }

        private void SetAllowIntegerShortcuts(bool value)
        {
            if (value && !shortcutsCreated)
                CreateIntegerShortcuts();
            integerShortcuts = value;
        }

        private void CreateIntegerShortcuts()
        {
            if (shortcutsCreated)
                return;

            for(int i = 0; i < EnumExt<TEnum>.Values.Length; ++i)
            {
                string name = EnumExt<TEnum>.Names[i];
                string numString = EnumExt<TEnum>.IntegerValues[i].ToString();
                commands.Add(numString, commands[name]);
            }
            shortcutsCreated = true;
        }

        // Empty to swallow the callback.
        private void OnSuccess(List<string> args, TEnum value) { }
    }
}
