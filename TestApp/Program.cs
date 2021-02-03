using System;
using System.Collections.Generic;

namespace TestApp
{
    internal static class Program
    {
        internal static string IntArrToString(IEnumerable<int> arr) =>
            arr == null ? "NULL" : $"[{string.Join(",", arr)}]";

        internal static void Main(string[] args)
        {
            Console.WriteLine("bruh");

            // [[A6ADEB393E0370C0]]-0x20 on 64bit
            // [[A6ADEB393E0370C0]]-0x04 on 32bit
            // random attempt at creating a Thing with a predictable signature but also making sure that number doesnt show up somewhere else
            var thing = new Thing(-4580157255241192026L - 2000);

            // supper shitty commandline app that accepts commands to change values
            while (true)
            {
                Console.WriteLine(
                    $"i: {thing.Int} | s: {thing.String} | a: {IntArrToString(thing.IntArr)} | l: {IntArrToString(thing.IntList)}");
                var cmd = Console.ReadLine()?.Trim();
                if (cmd == null || cmd.Equals("exit")) break;

                if (cmd.StartsWith("ia")) // int add
                {
                    if (int.TryParse(cmd.Substring(2), out var add))
                    {
                        Console.WriteLine($"adding {add} to int");
                        thing.Int += add;
                    }
                    else
                    {
                        Console.WriteLine("not a valid number");
                    }
                }
                else if (cmd.StartsWith("is")) // int subtract
                {
                    if (int.TryParse(cmd.Substring(2), out var add))
                    {
                        Console.WriteLine($"subtracting {add} to int");
                        thing.Int -= add;
                    }
                    else
                    {
                        Console.WriteLine("not a valid number");
                    }
                }
                else if (cmd.StartsWith("ss")) // string set
                {
                    var newString = cmd.Substring(2);
                    Console.WriteLine("updating string");
                    thing.String = newString;
                }
                else if (cmd.StartsWith("an")) // array set null
                {
                    Console.WriteLine("set array to null");
                    thing.IntArr = null;
                }
                else if (cmd.StartsWith("ae")) // array set to empty
                {
                    Console.WriteLine("set array to empty");
                    thing.IntArr = Array.Empty<int>();
                }
                else if (cmd.StartsWith("aa")) // array append
                {
                    if (thing.IntArr == null)
                    {
                        Console.WriteLine("array is null, invalid operation");
                    }
                    else
                    {
                        if (int.TryParse(cmd.Substring(2), out var add))
                        {
                            Console.WriteLine($"appending {add} to array");
                            var newArr = new int[thing.IntArr.Length + 1];
                            Array.Copy(thing.IntArr, newArr, thing.IntArr.Length);
                            newArr[thing.IntArr.Length] = add;
                            thing.IntArr = newArr;
                        }
                        else
                        {
                            Console.WriteLine("not a valid number");
                        }
                    }
                }
                else if (cmd.StartsWith("ln")) // list set null
                {
                    Console.WriteLine("set list to null");
                    thing.IntList = null;
                }
                else if (cmd.StartsWith("le")) // list set to empty
                {
                    Console.WriteLine("set array to empty");
                    thing.IntList = new List<int>();
                }
                else if (cmd.StartsWith("la")) // l append
                {
                    if (thing.IntList == null)
                    {
                        Console.WriteLine("list is null, invalid operation");
                    }
                    else
                    {
                        if (int.TryParse(cmd.Substring(2), out var add))
                        {
                            Console.WriteLine($"appending {add} to list");
                            thing.IntList.Add(add);
                        }
                        else
                        {
                            Console.WriteLine("not a valid number");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("invalid cmd");
                }
            }
        }
    }
}