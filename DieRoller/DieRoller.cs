﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommandEngine;

namespace DieRoller
{
    public class DiceRoller
    {
        public delegate TResult TryParseDelegate<TInput, TOutParam, TResult>(TInput input, out TOutParam output);

        static System.Random rng = new System.Random();

        public static int RollDice(int numDice, int numSides)
        {
            int result = 0;
            for (int i = 0; i < numDice; ++i)
                result += rng.Next(numSides) + 1;
            return result;
        }

        public static (int Result, string Message) RollDice(DiceRollSet diceSet, DiceRollerMessageOptions options)
        {
            int result = 0;
            StringBuilder msg = new StringBuilder();
            if (options.RollSeperate)
            {
                int total = 0;

                foreach (var dice in diceSet)
                {
                    msg.Append($"\n{dice.NumDice}d{dice.NumSides}: ");
                    if (options.DisplayIndividualRolls)
                    {
                        var rolls = RollSeperate(dice);
                        result = rolls.Result;
                        total += result;
                        msg.Append($"{result} [{rolls.Message}]");
                    }
                    else
                    {
                        result = dice.Roll();
                        total += result;
                        msg.Append($"{result}");
                    }

                    if (options.DisplaySeperateStats)
                    {
                        msg.Append($"\n{dice.GetStats()}\n");
                    }
                }

                msg.Append($"\nTotal: {total}");
            }
            else
            {
                if (options.DisplayIndividualRolls)
                {
                    var rolls = RollSeperate(diceSet);
                    msg.Append($"\nTotal: {rolls.Result} [{rolls.Message}]");
                }
                else
                {
                    result = diceSet.Roll();
                    msg.Append($"\nTotal: {result}");
                }

                if (options.DisplaySeperateStats)
                {
                    foreach (var roll in diceSet)
                        msg.Append($"\n{roll.GetStats()}");
                }
            }

            if (options.DisplayFullStats)
                msg.Append($"\n{GetFullRollStats(diceSet)}");

            if (options.DisplayTotalStats)
                msg.Append($"\n{diceSet.GetStats()}");

            return (result, msg.ToString());
        }
        public static (int Result, string Message) RollDice2(DiceRollSet diceSet, DiceRollerMessageOptions options)
        {
            int result = 0;
            StringBuilder msg = new StringBuilder();
            StringBuilder rollStr = new StringBuilder();

            foreach (var dice in diceSet)
            {
                var rolls = RollSeperate(dice);

                result += rolls.Result;

                if (options.RollSeperate)
                {
                    msg.Append($"\n{dice.NumDice}d{dice.NumSides}: ");
                    msg.Append($"{rolls.Result}");
                    if (options.DisplayIndividualRolls)
                        msg.Append($" [{rolls.Message}]");
                }
                else if (options.DisplayIndividualRolls)
                    rollStr.Append(rolls.Message);

                if (options.DisplaySeperateStats)
                    msg.Append($"\n{dice.GetStats()}\n");
            }

            if (options.DisplayFullStats)
                msg.Append($"\n{GetFullRollStats(diceSet)}");

            if (options.DisplayTotalStats)
                msg.Append($"\n{diceSet.GetStats()}");


            if (rollStr.Length > 0)
                msg.Append($"\nTotal: {result} [{rollStr}]");
            msg.Append($"\nTotal: {result}");
            
            return (result, msg.ToString());
        }

        static (int Result, string Message) RollSeperate(IDiceRoll diceRoll)
        {
            (IList<int> Rolls, int Total) rolls = diceRoll.RollSeperate();
            StringBuilder msgBuilder = new StringBuilder();

            foreach(int roll in rolls.Rolls)
            {
                msgBuilder.Append($"{roll} ");
            }

            msgBuilder.Remove(msgBuilder.Length - 1, 1);
            return (rolls.Total, msgBuilder.ToString());
        }
        static (int Result, string Message) RollSeperate(DiceRollSet diceRolls)
        {
            int total = 0;
            StringBuilder msgBuilder = new StringBuilder();

            foreach (IDiceRoll roll in diceRolls)
            {
                var rollResult = roll.RollSeperate();
                total += rollResult.Total;
                foreach (int r in rollResult.Rolls)
                    msgBuilder.Append($"{r} ");
            }

            msgBuilder.Remove(msgBuilder.Length - 1, 1);
            return (total, msgBuilder.ToString());
        }

        public static string GetFullRollStats(IDiceRoll diceRoll)
        {
            return GetFullRollStatsInternal(GetDiceSides(diceRoll));
        }
        public static string GetFullRollStats(IEnumerable<IDiceRoll> diceRolls)
        {
            return GetFullRollStatsInternal(GetDiceSides(diceRolls));
        }
        static string GetFullRollStatsInternal(IEnumerable<int> resultEnum)
        {
            double average = 0;
            int totalRolls = 0;
            Dictionary<int, int> totals = new Dictionary<int, int>();
            StringBuilder msg = new StringBuilder();

            foreach (var rollResult in resultEnum)
            {
                if (totals.ContainsKey(rollResult))
                {
                    ++totals[rollResult];
                }
                else totals.Add(rollResult, 1);

                ++totalRolls;
                average += rollResult;
            }
            average /= totalRolls;

            var sortedKeyList = totals.Keys.ToList();
            sortedKeyList.Sort();
            foreach (var key in sortedKeyList)
            {
                double chance = ((double)totals[key] / (double)totalRolls) * 100;
                msg.Append($"{key}: {totals[key]} ({chance}%)\n");
            }
            msg.Append($"Avg: {average}");
            return msg.ToString();
        }
        
        public static string GetFullRollStatsPolynomial(IEnumerable<IDiceRoll> diceRolls)
        {
            double Generator(int n)
            {

                return 0.0;
            }
            return null;
        }

        static IEnumerable<int> GetDiceSides(IDiceRoll diceRoll)
        {
            return GetDiceSides(diceRoll, 0);
        }
        static IEnumerable<int> GetDiceSides(IDiceRoll diceRoll, int currIndex)
        {
            var sideEnum = diceRoll.GetDieSides();
            var nextDiceEnum = GetDiceSides(diceRoll, currIndex + 1);

            foreach (var side in sideEnum)
            {
                int total = side;

                if (currIndex < diceRoll.NumDice - 1)
                {
                    foreach (var nextDiceTotal in nextDiceEnum)
                        yield return total + nextDiceTotal;
                }
                else yield return total;
            }
        }
        static IEnumerable<int> GetDiceSides(IEnumerable<IDiceRoll> diceRolls)
        {
            return GetDiceSides(diceRolls, 0, diceRolls.Count());
        }
        static IEnumerable<int> GetDiceSides(IEnumerable<IDiceRoll> diceRolls, int currIndex, int count)
        {
            var diceRollEnum = GetDiceSides(diceRolls.ElementAt(currIndex));
            if (currIndex < count - 1)
            {
                var nextDiceEnum = GetDiceSides(diceRolls, currIndex + 1, count);

                foreach (var result in diceRollEnum)
                {
                    foreach (var nextResult in nextDiceEnum)
                        yield return result + nextResult;
                }
            }
            else
            {
                foreach (var result in diceRollEnum)
                    yield return result;
            }
        }

        public static void DiceRollPrompt<T>(TryParseDelegate<string, T, bool> TryParse) where T : IDiceRoll
        {
            Console.WriteLine("\n\nEnter Dice to Roll:");

            string input = Console.ReadLine();
            string[] diceRollsRaw = input.Split(' ');

            DiceRollPrompt(diceRollsRaw, TryParse);
        }
        public static void DiceRollPrompt<T>(string[] diceRollsRaw, TryParseDelegate<string, T, bool> TryParse) where T : IDiceRoll
        {
            DiceRollSet rollSet = new DiceRollSet();
            DiceRollerMessageOptions options = new DiceRollerMessageOptions();

            for (int i = 0; i < diceRollsRaw.Length; ++i)
            {
                bool success = TryParse(diceRollsRaw[i], out var diceRoll);
                if (success)
                    rollSet.Add(diceRoll);
                else if (ParseBasicDiceRollOption(diceRollsRaw[i], ref options))
                    continue;
                else
                {
                    int num = 0;
                    switch(diceRollsRaw[i])
                    {
                        case "-h":
                            if (int.TryParse(diceRollsRaw[++i], out num))
                                options.NumHighest = num;
                            options.TakeHighest = num > 0;
                            break;
                        case "-l":
                            if (int.TryParse(diceRollsRaw[++i], out num))
                                options.NumLowest = num;
                            options.TakeLowest = num > 0;
                            break;
                        case "-r":
                            if (!int.TryParse(diceRollsRaw[++i], out num))
                            {
                                ConsoleExt.WriteWarningLine("-r Requires a valid value, command skipped.");
                                break;
                            }
                            else options.RerollValue = num;

                            if (int.TryParse(diceRollsRaw[i + 1], out num))
                            {
                                ++i; // Only increment if the next entry is a valid number, otherwise process it normally next iter.
                                options.RerollAttempts = num;
                            }
                            options.AllowReroll = true;
                            break;
                    }
                }
            }

            var roll = DiceRoller.RollDice(rollSet, options);
            Console.WriteLine(roll.Message);
        }

        static bool ParseBasicDiceRollOption(string input, ref DiceRollerMessageOptions options)
        {
            switch (input)
            {
                case "s": options.RollSeperate ^= true; return true;
                case "i": options.DisplayIndividualRolls ^= true; return true;
                case "st": options.DisplaySeperateStats ^= true; return true;
                case "t": options.DisplayTotalStats ^= true; return true;
                case "f": options.DisplayFullStats ^= true; return true;
            }
            return false;
        }
    }
}
