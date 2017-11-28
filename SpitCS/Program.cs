using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

/*
 * TODO (list)
 * Make yield and force directives configurable
 * Make load directive take in job description object + make scripts able to access clone of current file's job desc
 * Indent handling as separate subroutine
 * 
 * DECIDED AGAINST
 * fancy dependency graph nonsense
 * more sophisticated logging
 */

namespace SpitCS
{
    public class Program
    {
        static void Main(string[] args)
        {
            var job = new SpitJob(args, Console.Out);
            job.Run();
            job.WriteAll();
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

        public static IEnumerable<KeyValuePair<int, T>> Indexed<T>(this IEnumerable<T> list)
        {
            return list.Select((obj, ind) => new KeyValuePair<int, T>(ind, obj));
        }

        public static IEnumerable<T> Slice<T>(this IEnumerable<T> seq, int from, int to)
        {
            return seq.Skip(from).Take(to - from);
        }

        public static string AggLines(this IEnumerable<string> lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        public static IEnumerable<T> CombineSeqs<T>(params IEnumerable<T>[] seqs)
        {
            return seqs.SelectMany(s => s);
        }

        public static IEnumerable<T> Linqify<T>(this T obj)
        {
            return new[] {obj};
        }

        public static IEnumerable<T> TakeAllButLast<T>(this IEnumerable<T> source)
        {
            var it = source.GetEnumerator();
            bool hasRemainingItems = false;
            bool isFirst = true;
            T item = default(T);

            do
            {
                hasRemainingItems = it.MoveNext();
                if (hasRemainingItems)
                {
                    if (!isFirst) yield return item;
                    item = it.Current;
                    isFirst = false;
                }
            } while (hasRemainingItems);
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
    }
}
