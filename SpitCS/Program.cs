using Mono.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

/*
 * TODO (list)
 * Cleanup
 * Multiple one-liners per line
 * Force in one-liners
 * Multiple files in cmd line
 * NewLine!!
 * Yield not at beginning of line??
 * No hash when InFile != OutFile
 * Dubious restriction on stuff same line as start/end tokens
 */

namespace SpitCS
{
    public class Program
    {
        static void Main(string[] args)
        {
            var fileCfg = ParseCommandLine(args, out string cfgFilename);

            var stackCfg = new StackCfg();

            if (cfgFilename != null)
            {
                try
                {
                    stackCfg = StackCfg.LoadJson(cfgFilename);
                }
                catch (Exception e)
                {
                    var defaultCfgFile = "spit-default.json";
                    var excMessage = $"Could not load config file {cfgFilename}.";
                    var helpfulMessage = excMessage
                        + $"Generating {defaultCfgFile}. Use it with /cfg={defaultCfgFile})";

                    Console.WriteLine(helpfulMessage);
                    File.WriteAllText(
                        defaultCfgFile,
                        JsonConvert.SerializeObject(StackCfg.Default(), new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            NullValueHandling = NullValueHandling.Ignore,
                        }));
                    throw new AggregateException(excMessage, e);
                }
            }

            // NOTE cfg.PrepareForUse unnecessary, will get called in SpitJob ctor
            SpitGlobalJob.Execute(fileCfg, stackCfg);
        }

        public static FileCfg ParseCommandLine(string[] args, out string jsonFilename)
        {
            var firstJob = new FileCfg();
            string jsonFilenameLocal = null;
            bool showHelp = false;

            var options = new OptionSet
            {
                { "i|input=",    "the file to preprocess",          i => firstJob.InputFileName = i            },
                { "g|generate",  "generate code",                   g => firstJob.Generate = (g != null)       },
                { "c|clean",     "deletes generated code",          c => firstJob.DeleteOutput = (c != null)   },
                { "p|private",   "deletes spit generator code",     p => firstJob.DeleteSpitCode = (p != null) },
                { "cfg|config=", "json file holding configuration", f => jsonFilenameLocal = f                 },
                { "h|help",      "show this message and exit",      v => showHelp = (v != null)                },
                {
                    "x|skip",
                    "doesn't evaluate spit code (and therefore does not generate it). Still tries to eval lines beginning " +
                    $"with force directive (default  \"{FormatCfg.Default().ForceDirective}\"",
                    x => firstJob.SkipCompute = (x != null)
                },
                {
                    "o|output=",
                    "where the file with generated code added gets written to. Leave blank to replace existing file",
                    o => firstJob.OutputFileName = o
                },
            };

            try { options.Parse(args); }
            catch (OptionException e)
            {
                options.WriteOptionDescriptions(Console.Out);
                throw e;
            }

            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Out);
                Environment.Exit(0);
            }

            jsonFilename = jsonFilenameLocal;

            return firstJob;
        }
    }

    public static class Util
    {
        public static void Assert(bool condition, string message,
            [CallerFilePath] string sourceFile = "UNKNOWN",
            [CallerLineNumber] int lineNumber = -1)
        {
            if (!condition)
            {
                throw new Exception($"SpitError at {sourceFile}({lineNumber},0): {message}");
            }
        }

        public static void AssertWarning(bool condition, string message,
            [CallerFilePath] string sourceFile = "UNKNOWN",
            [CallerLineNumber] int lineNumber = -1)
        {
            if (!condition)
            {
                Console.Error.WriteLine($"SpitWarning at {sourceFile}({lineNumber},0): {message}");
            }
        }

        public static IList<T> Slice<T>(this IList<T> list, int from, int to)
        {
            return list.Skip(from).Take(to - from).ToList();
        }

        public static string AggLines(this IEnumerable<string> lines, string newL)
        {
            return string.Join(newL, lines);
        }

        public static MD5 md5 = System.Security.Cryptography.MD5.Create();

        public static string CalculateMD5Hash(string input)
        {
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            return string.Join("",
                hash.Select(b => b.ToString("X2")));
        }

        public static string[] EzSplit(this string s, params string[] splitters)
        {
            return s == "" ? new string[0] : s.Split(splitters, StringSplitOptions.None);
        }

        public static string AtString(string text, string newLine)
        {
            var lines = text.EzSplit(newLine).ToList();
            if (lines[0].Trim() == "") { lines.RemoveAt(0); }
            if (lines[lines.Count - 1].Trim() == "") { lines.RemoveAt(lines.Count - 1); }

            var minIndent = lines.Min(l => text.TakeWhile(c => c == ' ').Count());

            return string.Join(newLine, lines.Select(l => l.Substring(minIndent)));
        }

        public static T[] Array1<T>(this T val)
        {
            return new[] {val};
        }
    }
}
