using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class EnumCommandModule<TEnum> where TEnum : struct, Enum
    {
        public delegate void OnSuccessDelegate(List<string> args, TEnum value);
        
        public bool AllowsIntegerShortcuts { get=> integerShortcuts; set => SetAllowIntegerShortcuts(value); }

        bool integerShortcuts = false;
        bool shortcutsCreated = false;
        OnSuccessDelegate OnSuccessCallback;
        Dictionary<string, ConsoleCommand<TEnum>> enumCommands;

        public EnumCommandModule(bool allowIntegerShortcuts)
        {
            var enumValues = EnumExt<TEnum>.Values;

            OnSuccessCallback = OnSuccess;
            AllowsIntegerShortcuts = allowIntegerShortcuts;
            enumCommands = new Dictionary<string, ConsoleCommand<TEnum>>(enumValues.Length * (allowIntegerShortcuts ? 2 : 1));

            for (int i = 0; i < enumValues.Length; i++)
            {
                var value = EnumExt<TEnum>.Values[i];
                string valueString = EnumExt<TEnum>.Names[i];
                ConsoleCommand<TEnum> command = new ConsoleCommand<TEnum>(valueString, (List<string> args) => { OnSuccessCallback(args, value); return value; });
                enumCommands.Add(valueString, command);
            }

            if (allowIntegerShortcuts)
                CreateIntegerShortcuts();
        }
        public EnumCommandModule(bool allowIntegerShortcuts, OnSuccessDelegate OnSuccessCallback) : this(allowIntegerShortcuts)
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
                enumCommands.Add(numString, enumCommands[name]);
            }
            shortcutsCreated = true;
        }

        // Empty to swallow the callback.
        private void OnSuccess(List<string> args, TEnum value) { }

        public TEnum GetValueFromCommand(string message, bool newline) 
            => CommandManager.GetNextCommand(message, newline, enumCommands);
        public bool TryGetValueFromCommand(string message, bool newline, out TEnum value) 
            => CommandManager.GetNextCommand(message, newline, enumCommands, out value);
    }
}
