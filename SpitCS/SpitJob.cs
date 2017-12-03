using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Mono.Options;
using Newtonsoft.Json;

namespace SpitCS
{
    public class SpitGlobalJob : ISpitGlobals
    {
        public ScriptState ScriptState = null;

        public readonly HashSet<string> InputFiles = new HashSet<string>();
        public readonly HashSet<string> OutputFiles = new HashSet<string>();
        public readonly HashSet<string> Includes = new HashSet<string>();
        public readonly HashSet<string> Assemblies = new HashSet<string>();
        public readonly HashSet<string> Usings = new HashSet<string>();
        public readonly List<SpitJob> jobWriteList = new List<SpitJob>();
        public readonly Stack<SpitJob> jobStack = new Stack<SpitJob>();

        public ISpitGlobals Globals => this;

        public SpitJob CurrentJob => jobStack.Peek();

        #region ISplitGlobals implementation
        IReadOnlyList<string> ISpitGlobals.PrevCodeLines => CurrentJob.PrevCodeLines;
        IReadOnlyList<string> ISpitGlobals.PrevContentLines => CurrentJob.PrevContentLines;
        IReadOnlyList<string> ISpitGlobals.PrevOutputLines => CurrentJob.PrevOutputLines;
        string ISpitGlobals.PrevCode => CurrentJob.PrevCodeLines.AggLines();
        string ISpitGlobals.PrevContent => CurrentJob.PrevContentLines.AggLines();
        string ISpitGlobals.PrevOutput => CurrentJob.PrevOutputLines.AggLines();
        TextWriter ISpitGlobals.SpitOut => CurrentJob.SpitOut;
        string ISpitGlobals.AtString(string s) => Util.AtString(s, CurrentJob.FileCfg.FormatCfg.NewL);
        IReadOnlyDictionary<string, object> ISpitGlobals.Defines => CurrentJob.FileCfg.Defines;
        OneTimeScriptState ISpitGlobals.SpitEval(string code, string src, int ln) =>
            new OneTimeScriptState(Eval(code), src, ln);
        FileCfg ISpitGlobals.FileConfig => CurrentJob.SharedFileCfg.Clone();
        StackCfg ISpitGlobals.StackConfig => CurrentJob.SharedStackCfg.Clone();
        
        OneTimeScriptState ISpitGlobals.SpitLoad(
            string inputFile, string outputFile, FileOp operations,
            string src, int ln)
        {
            var fileCfg = Globals.FileConfig.SetFile(inputFile, outputFile).SetOperations(operations);
            return Globals.SpitLoad(fileCfg, Globals.StackConfig);
        }

        OneTimeScriptState ISpitGlobals.SpitLoad(FileCfg fileCfg, StackCfg stackCfg,
            string src, int ln)
        {
            RunJob(fileCfg, stackCfg ?? CurrentJob.StackCfg.Clone());
            return new OneTimeScriptState(ScriptState, src, ln);
        }
        #endregion

        public static void Execute(FileCfg fileCfg, StackCfg stackCfg)
        {
            var runner = new SpitGlobalJob();
            runner.RunJob(fileCfg, stackCfg);
            runner.WriteFiles();
        }

        public void RunJob(FileCfg fileCfg, StackCfg stackCfg)
        {
            var job = new SpitJob
            {
                SharedStackCfg = stackCfg,
                SharedFileCfg = fileCfg,
            };

            {
                job.StackCfg = StackCfg.Default().MergeOverWith(job.SharedStackCfg);
                job.FileCfg = job.StackCfg.GetProtoFileCfg(job.SharedFileCfg.InputFileExt)
                    .MergeOverWith(job.SharedFileCfg);
                job.FileCfg.Validate();
                jobStack.Push(job);
                jobWriteList.Add(job);

                if (ScriptState == null)
                {
                    ScriptState = CSharpScript.RunAsync(
                        "",
                        ScriptOptions.Default
                            .WithMetadataResolver(ScriptMetadataResolver.Default
                                .WithBaseDirectory(Environment.CurrentDirectory)
                                .WithSearchPaths(RuntimeEnvironment.GetRuntimeDirectory())
                                .WithSearchPaths(job.StackCfg.Includes)
                            ).WithEmitDebugInformation(true),
                        this, typeof(ISpitGlobals)).Result;
                }
                else
                {
                    foreach (var incl in job.StackCfg.Includes.Where(i => Includes.Add(i)))
                    {
                        Util.AssertWarning(false,
                            $"Could not add include directory {incl} midway through Spit execution");
                    }
                }

                StringBuilder setupCode = new StringBuilder();

                foreach (var assm in job.StackCfg.Assemblies.Where(a => Assemblies.Add(a)))
                {
                    setupCode.AppendLine($"#r\"{assm}\"");
                }

                foreach (var using1 in job.StackCfg.Usings.Where(u => Usings.Add(u)))
                {
                    setupCode.AppendLine($"using {using1};");
                }

                Util.AssertWarning(
                    InputFiles.Add(job.FileCfg.InputFilePath),
                    $"{job.FileCfg.InputFilePath} is getting preprocessed twice.");

                Util.AssertWarning(
                    OutputFiles.Add(job.FileCfg.OutputFilePath),
                    $"{job.FileCfg.OutputFilePath} is getting written twice.");

                ScriptState = ScriptState.ContinueWithAsync(setupCode.ToString()).Result;
            }

            var fmt = job.FileCfg.FormatCfg;

            {
                var text = File.ReadAllText(job.FileCfg.InputFilePath, fmt.Encoding);
                var linesIn = text.EzSplit(fmt.NewL);

                Action<int, int, LineGroupType> addLineGroup = (s, e, t) =>
                    job.LineGroups.Add(new LineGroup
                    {
                        InclStart = s,
                        ExclEnd = e,
                        Type = t,
                    });

                addLineGroup(0, -1, LineGroupType.Content);

                for (int index = 0; index < linesIn.Length; index++)
                {
                    var line = linesIn[index];

                    switch (job.LineGroups.Last().Type)
                    {
                        case LineGroupType.Content:
                            if (line.Contains(fmt.StartSpitToken))
                            {
                                job.LineGroups.Last().ExclEnd = index;
                                addLineGroup(index, index + 1, LineGroupType.StartSpitCode);
                                addLineGroup(index + 1, -1, LineGroupType.SpitCode);
                            }
                            break;
                        case LineGroupType.SpitCode:
                            if (line.Contains(fmt.StartOutputToken))
                            {
                                job.LineGroups.Last().ExclEnd = index;
                                addLineGroup(index, index + 1, LineGroupType.StartOutput);
                                addLineGroup(index + 1, -1, LineGroupType.SpitOutput);
                            }
                            break;
                        case LineGroupType.SpitOutput:
                            if (line.Contains(fmt.EndOutputToken))
                            {
                                job.LineGroups.Last().ExclEnd = index;
                                addLineGroup(index, index + 1, LineGroupType.EndOutput);
                                addLineGroup(index + 1, -1, LineGroupType.Content);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                job.LineGroups.Last().ExclEnd = linesIn.Length;

                Util.Assert(
                    job.LineGroups.Last().Type == LineGroupType.Content,
                    "Doucment ends with unclosed " + job.LineGroups.Last().Type,
                    job.FileCfg.InputFileName, job.LineGroups.Last().InclStart + 1);

                foreach (var lg in job.LineGroups)
                {
                    lg.InLines = linesIn.Slice(lg.InclStart, lg.ExclEnd).ToList();
                }
            }

            {
                string indent = "";
                string indentWhiteSpace = "";
                string oldHash = null;
                List<string> prevOutputIndented = null;
                int prevCodeStartLine = -1;

                foreach (var lineGroup in job.LineGroups)
                {
                    switch (lineGroup.Type)
                    {
                        case LineGroupType.Content:
                        {
                            job.PrevContentLines = lineGroup.InLines;
                            lineGroup.OutLines = lineGroup.InLines;
                        } break;
                        case LineGroupType.SpitCode:
                        {
                            job.PrevCodeLines = lineGroup.InLines.Select(l =>
                            {
                                Util.Assert(l.Contains(indent), "Line not indented",
                                    job.FileCfg.InputFileName, lineGroup.StartingLineNumber);
                                return indent.Length == 0 ? "" : l.Replace(indent, "");
                            }).ToList();
                            lineGroup.OutLines = lineGroup.InLines;
                            prevCodeStartLine = lineGroup.StartingLineNumber;
                        } break;
                        case LineGroupType.SpitOutput:
                        {
                            oldHash = Util.CalculateMD5Hash(lineGroup.InLines.AggLines());
                            job.SpitOut = new StringWriter();
                            StringBuilder codeSb = new StringBuilder();
                            int lineNumber = prevCodeStartLine;
                            codeSb.AppendLine($"#line {lineNumber} \"{job.FileCfg.InputFileName}\"");
                            
                            void RunCollectedCode()
                            {
                                OneTimeScriptState.CheckInstancesDisposed();
                                ScriptState = Eval(codeSb.ToString());
                                codeSb.Clear();
                                var returnValue = ScriptState.ReturnValue;
                                var returnValueScriptState = returnValue as OneTimeScriptState;

                                // SpitLoad or SpitEval
                                if (returnValueScriptState != null)
                                {
                                    ScriptState = returnValueScriptState.ScriptState;
                                }
                                else if (returnValue != null)
                                {
                                    job.SpitOut.WriteLine(returnValue);
                                }

                                codeSb.AppendLine($"#line {lineNumber + 1} \"{job.FileCfg.InputFileName}\"");
                            }

                            string latestReturnValue = null;

                            foreach (var line in job.PrevCodeLines)
                            {
                                bool forced = false, yield = false;
                                string trimmedLine = line.Trim();

                                if (trimmedLine.StartsWith(fmt.ForceDirective))
                                {
                                    forced = true;
                                    trimmedLine = trimmedLine.Replace(fmt.ForceDirective, "");
                                }

                                if (job.FileCfg.ShallSkipCompute && !forced) { continue; }

                                if (trimmedLine.StartsWith(fmt.YieldDirective))
                                {
                                    yield = true;
                                    trimmedLine = trimmedLine.Replace(fmt.YieldDirective, "");
                                }

                                codeSb.AppendLine(trimmedLine);

                                if (yield) { RunCollectedCode(); }
                                lineNumber++;
                            }

                            RunCollectedCode();
                            var outputUnindented = job.SpitOut.GetStringBuilder().ToString();

                            if (outputUnindented.EndsWith(Environment.NewLine))
                            {
                                outputUnindented =
                                    outputUnindented.Substring(0, outputUnindented.Length - Environment.NewLine.Length);
                            }

                            job.SpitOut.Dispose();
                            job.SpitOut = null;
                            job.PrevOutputLines = outputUnindented.EzSplit(Environment.NewLine).ToList();
                            prevOutputIndented = job.PrevOutputLines.Select(line => indentWhiteSpace + line).ToList();
                            lineGroup.OutLines = prevOutputIndented;
                        } break;
                        case LineGroupType.EndOutput:
                        {
                            var markerInd = lineGroup.InLines.First().IndexOf(fmt.EndOutputToken);
                            var indent1 = lineGroup.InLines.First().Substring(0, markerInd);
                            Util.Assert(indent == indent1, "indent mismatch",
                                job.FileCfg.InputFileName, lineGroup.StartingLineNumber);

                            string lastOutputHash = Util.CalculateMD5Hash(prevOutputIndented.AggLines());

                            var line = lineGroup.InLines.Last();
                            var match = Regex.Match(line, fmt.HashRegex);
                            if (match.Success)
                            {
                                line = line.Replace(match.Value, "");
                                Util.Assert(match.Groups[1].Value == oldHash,
                                    "Code preceding hash does not match hash, which means that generated code has been edited.\n" +
                                    "Undo changes or remove Hash",
                                    job.FileCfg.InputFileName, lineGroup.StartingLineNumber);
                            }

                            lineGroup.OutLines = new List<string>
                            {
                                $"{line}{fmt.FmtHash(lastOutputHash)}"
                            };
                        } break;
                        case LineGroupType.StartSpitCode:
                        {
                            var markerInd = lineGroup.InLines.First().IndexOf(fmt.StartSpitToken);
                            indent = lineGroup.InLines.First().Substring(0, markerInd);
                            indentWhiteSpace = "".PadRight(indent.Length);
                            lineGroup.OutLines = lineGroup.InLines;
                        } break;
                        case LineGroupType.StartOutput:
                        {
                            var markerInd = lineGroup.InLines.First().IndexOf(fmt.StartOutputToken);
                            var indent1 = lineGroup.InLines.First().Substring(0, markerInd);
                            Util.Assert(indent == indent1, "indent mismatch", job.FileCfg.InputFileName,
                                lineGroup.StartingLineNumber);
                            lineGroup.OutLines = lineGroup.InLines;
                        } break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            // Invalidating stuff which I know I don't want to be used again
            job.SpitOut = null;
            job.PrevCodeLines = null;
            job.PrevContentLines = null;
            job.PrevOutputLines = null;
            jobStack.Pop();
        }

        public void WriteFiles()
        {
            foreach (var job in jobWriteList)
            {
                if (!job.FileCfg.ShallDeleteOutput
                    && !job.FileCfg.ShallDeleteSpitCode
                    && !job.FileCfg.ShallGenerate)
                {
                    return;
                }

                var fmt = job.FileCfg.FormatCfg;

                using (var writer = new StreamWriter(job.FileCfg.OutputFilePath, false, fmt.Encoding))
                {
                    foreach (var lineGroup in job.LineGroups)
                    {
                        IEnumerable<string> writtenLines = lineGroup.OutLines;

                        switch (lineGroup.Type)
                        {
                            case LineGroupType.SpitCode:
                            {
                                if (job.FileCfg.ShallDeleteSpitCode) { writtenLines = null; }
                            } break;
                            case LineGroupType.SpitOutput:
                            {
                                if (!job.FileCfg.ShallGenerate) { writtenLines = lineGroup.InLines; }
                                if (job.FileCfg.ShallDeleteOutput) { writtenLines = null; }
                            } break;
                            case LineGroupType.EndOutput:
                            {
                                // If user has elected not to generate, leave prev line (possibly including hash) alone
                                if (!job.FileCfg.ShallGenerate) { writtenLines = lineGroup.InLines; }

                                // If user wants output deleted, strip hash value from line (no matter the source)
                                // This is an unusual case :/
                                if (job.FileCfg.ShallDeleteOutput)
                                {
                                    writtenLines = writtenLines.Select(l => Regex.Replace(l, fmt.HashRegex, ""));
                                }

                                // or if spit code is deleted, replace with cold output token
                                if (job.FileCfg.ShallDeleteSpitCode)
                                {
                                    writtenLines = writtenLines
                                        .Select(l => l.Replace(fmt.EndOutputToken, fmt.ColdEndOutputToken))
                                        .Select(l => Regex.Replace(l, fmt.HashRegex, ""));
                                }
                            } break;
                            case LineGroupType.StartSpitCode:
                            {
                                if (job.FileCfg.ShallDeleteSpitCode) { writtenLines = null; }
                            } break;
                            case LineGroupType.StartOutput:
                            {
                                if (job.FileCfg.ShallDeleteSpitCode)
                                {
                                    writtenLines = writtenLines
                                        .Select(l => l.Replace(fmt.StartOutputToken, fmt.ColdStartOutputToken));
                                }
                            } break;
                        }

                        if (writtenLines != null)
                        {
                            writer.WriteLine(writtenLines.AggLines());
                        }
                    }
                }
            }
        }

        public ScriptState Eval(string code)
        {
            try
            {
                return ScriptState.ContinueWithAsync(code).Result;
            }
            catch (AggregateException aggregateEx)
            {
                throw aggregateEx.InnerException ?? aggregateEx;
            }
        }
    }

    public enum LineGroupType
    {
        Content,
        StartSpitCode,
        SpitCode,
        StartOutput,
        SpitOutput,
        EndOutput,
    }

    public interface ISpitGlobals
    {
        string PrevContent { get; }
        string PrevOutput { get; }
        string PrevCode { get; }
        IReadOnlyList<string> PrevContentLines { get; }
        IReadOnlyList<string> PrevOutputLines { get; }
        IReadOnlyList<string> PrevCodeLines { get; }
        IReadOnlyDictionary<string, object> Defines { get; }
        TextWriter SpitOut { get; }
        string AtString(string s);
        OneTimeScriptState SpitEval(
            string code,
            [CallerFilePath] string sourceFile = "UNKNOWN",
            [CallerLineNumber] int lineNumber = -1);
        FileCfg FileConfig { get; }
        StackCfg StackConfig { get; }
        OneTimeScriptState SpitLoad(
            string inputFile, string outputFile = null, FileOp operations = FileOp.None,
            [CallerFilePath] string sourceFile = "UNKNOWN",
            [CallerLineNumber] int lineNumber = -1);
        OneTimeScriptState SpitLoad(
            FileCfg fileCfg, StackCfg stackCfg = null,
            [CallerFilePath] string sourceFile = "UNKNOWN",
            [CallerLineNumber] int lineNumber = -1);
    }

    public class SpitJob
    {
        // Long term state
        public readonly List<LineGroup> LineGroups = new List<LineGroup>();

        // Changes between blocks
        public StringWriter SpitOut;
        public List<string> PrevCodeLines = new List<string>();
        public List<string> PrevContentLines = new List<string>();
        public List<string> PrevOutputLines = new List<string>();

        // Actually used in execution
        public FileCfg FileCfg;
        public StackCfg StackCfg;

        // Holds only non-default values
        public FileCfg SharedFileCfg;
        public StackCfg SharedStackCfg;
    }

    public class LineGroup
    {
        public List<string> InLines;
        public List<string> OutLines;
        public LineGroupType Type;
        public int InclStart;
        public int ExclEnd = -1;

        public int StartingLineNumber => InclStart + 1;

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, " +
                   $"{nameof(InLines)}: [{InLines.Count}]({InLines.FirstOrDefault()}...), " +
                   $"{nameof(OutLines)}: [{OutLines.Count}]({OutLines.FirstOrDefault()}...)";
        }
    }

    public class OneTimeScriptState : IDisposable
    {
        private static readonly List<OneTimeScriptState> Instances = new List<OneTimeScriptState>();

        public string FileCreated { get; }
        public int LineNumberCreated { get; }
        private readonly ScriptState _scriptState;
        private bool _disposed = false;

        public ScriptState ScriptState
        {
            get
            {
                Dispose();
                return _scriptState;
            }
        }

        public OneTimeScriptState(ScriptState scriptState, string fileCreated, int lineNumberCreated = -1)
        {
            _scriptState = scriptState;
            FileCreated = fileCreated;
            LineNumberCreated = lineNumberCreated;
            Instances.Add(this);
        }

        public void Dispose()
        {
            Util.AssertWarning(
                !_disposed,
                $"{nameof(OneTimeScriptState)} has been disposed twice and likely represents mishandled execution state",
                FileCreated, LineNumberCreated);
            _disposed = true;
        }

        public static void CheckInstancesDisposed()
        {
            foreach (var instance in Instances)
            {
                Util.AssertWarning(instance._disposed,
                    $"{nameof(OneTimeScriptState)} is not disposed and likely represents lost execution state",
                    instance.FileCreated, instance.LineNumberCreated);
            }
        }

        public override string ToString()
        {
            return $"{nameof(FileCreated)}: {FileCreated}, {nameof(LineNumberCreated)}: {LineNumberCreated}";
        }
    }
}
