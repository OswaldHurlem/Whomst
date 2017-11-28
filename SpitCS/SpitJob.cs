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
    public class SpitFileSettings
    {
        public class JsonSettings
        {
            public string[] Includes = {};
            public Dictionary<string, object> Defines = new Dictionary<string, object>();
            public Dictionary<string, FiletypeConfig> ConfigByFiletype = new Dictionary<string, FiletypeConfig>();
        }

        public class FiletypeConfig
        {
            public string OutputSuffix = null;
            public string NewLine = null;
            public string StartSpitToken = null;
            public string StartOutputToken = null;
            public string EndOutputToken = null;
            public string EncodingWebName = null;
            public string ColdStartOutputToken = null;
            public string ColdEndOutputToken = null;

            public void MergeInferior(FiletypeConfig inferior)
            {
                OutputSuffix     = OutputSuffix     ?? inferior.OutputSuffix;
                NewLine          = NewLine          ?? inferior.NewLine;
                StartSpitToken   = StartSpitToken   ?? inferior.StartSpitToken;
                StartOutputToken = StartOutputToken ?? inferior.StartOutputToken;
                EndOutputToken   = EndOutputToken   ?? inferior.EndOutputToken;
                EncodingWebName  = EncodingWebName  ?? inferior.EncodingWebName;
                ColdStartOutputToken = ColdStartOutputToken ?? inferior.ColdStartOutputToken;
                ColdEndOutputToken   = ColdEndOutputToken   ?? inferior.ColdEndOutputToken;
            }
        }

        public string OutputFilePath;
        public bool? Generate;
        public bool? DeleteOutput;
        public bool? DeleteSpitCode;
        public bool? SkipCompute;
        public string InputFilePath;
        public Dictionary<string, FiletypeConfig> ConfigsByFiletype = new Dictionary<string, FiletypeConfig>();
        public Dictionary<string, object> Defines = new Dictionary<string, object>();

        public FiletypeConfig Ftc
        {
            get
            {
                FiletypeConfig cfg = null;
                ConfigsByFiletype.TryGetValue(FileType, out cfg);
                return cfg ?? ConfigsByFiletype["*"];
            }
        }
        public string FileType => Path.GetExtension(InputFilePath)?.Substring(1) ?? "*";
        public string InputFileName => Path.GetFileName(InputFilePath);
        public Encoding Encoding => Encoding.GetEncoding(Ftc.EncodingWebName);
        public bool MustGenerate       => Generate       ?? false;
        public bool MustDeleteOutput   => DeleteOutput   ?? false;
        public bool MustDeleteSpitCode => DeleteSpitCode ?? false;
        public bool MustSkipCompute    => SkipCompute    ?? false;

        public void MergeInferior(SpitFileSettings inferior)
        {
            OutputFilePath  = OutputFilePath  ?? inferior.OutputFilePath;
            Generate        = Generate        ?? inferior.Generate;
            DeleteOutput    = DeleteOutput    ?? inferior.DeleteOutput;
            DeleteSpitCode  = DeleteSpitCode  ?? inferior.DeleteSpitCode;
            InputFilePath   = InputFilePath   ?? inferior.InputFilePath;
            SkipCompute     = SkipCompute     ?? inferior.SkipCompute;

            foreach (var kvp in inferior.ConfigsByFiletype.Where(kvp => !ConfigsByFiletype.ContainsKey(kvp.Key)))
            {
                ConfigsByFiletype.Add(kvp.Key, kvp.Value);
            }

            foreach (var kvp in inferior.Defines.Where(kvp => !Defines.ContainsKey(kvp.Key)))
            {
                Defines.Add(kvp.Key, kvp.Value);
            }
        }

        // TODO: SpitGlobalWork parameter is crufty
        public void Configure(string[] args, TextWriter helpMsgWriter, ref string[] includes)
        {
            bool showHelp = false;

            JsonSettings jsonSettings = new JsonSettings();
            string jsonFileName = "";

            var options = new OptionSet
            {
                {
                    "i|input=",
                    "the file to preprocess",
                    i => InputFilePath = Path.GetFullPath(i)
                },
                {
                    "o|output=",
                    "where the file with generated code added gets written to. Leave blank to replace existing file",
                    o => OutputFilePath = Path.GetFullPath(o)
                },
                {
                    "g|generate",
                    "generate code",
                    g => Generate = (g != null)
                },
                {
                    "c|clean",
                    "deletes generated code",
                    c => DeleteOutput = (c != null)
                },
                {
                    "p|private",
                    "deletes spit generator code",
                    p => DeleteSpitCode = (p != null)
                },
                {
                    "x|skip",
                    "doesn't evaluate spit code (and therefore does not generate it). " +
                    $"Still tries to eval lines beginning with \"//{SpitJob.ForceDirective}\"",
                    x => SkipCompute = (x != null)
                },
                {
                    "cfg|config=",
                    "json file holding configuration",
                    fileName =>
                    {
                        if (fileName == null) return;
                        try
                        {
                            jsonFileName = fileName;
                            var cfgText = File.ReadAllText(fileName);
                            jsonSettings = JsonConvert.DeserializeObject<JsonSettings>(cfgText);
                        }
                        catch (Exception e)
                        {
                            var defaultJsonSettings = new JsonSettings
                            {
                                Defines = new Dictionary<string, object>
                                {
                                    ["Credit"] = "/* Generated with SpitCS: https://github.com/OswaldHurlem/SpitCS :) */"
                                },
                            };
                            EnsureDefaultFiletypeConfig(defaultJsonSettings.ConfigByFiletype);

                            var defaultJsonSettingsFile = "spit-default.json";

                            var m = $"Could not open config file {fileName}. Generating {defaultJsonSettingsFile}. " +
                                    $"Use it with -cfg {defaultJsonSettingsFile})";

                            File.WriteAllText(defaultJsonSettingsFile,
                                JsonConvert.SerializeObject(defaultJsonSettings, Formatting.Indented));

                            throw new AggregateException(m, e);
                        }
                    }
                },
                { "h|help",  "show this message and exit",
                    v => showHelp = v != null },
            };

            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                options.WriteOptionDescriptions(helpMsgWriter);
                throw e;
            }

            if (showHelp)
            {
                options.WriteOptionDescriptions(helpMsgWriter);
                return;
            }

            if (InputFilePath == null)
            {
                options.WriteOptionDescriptions(helpMsgWriter);
                throw new InvalidOperationException("Input file (/i) must be specified");
            }

            if (OutputFilePath == null)
            {
                OutputFilePath = InputFilePath;
            }

            var includesCopy = includes;

            // Can't add includes once a ScriptState has been made :(
            if (includesCopy != null 
                && jsonSettings.Includes.Any(incl => !includesCopy.Contains(incl)))
            {
                throw new NotSupportedException($"Referenced json config {jsonFileName} adds new include paths, " +
                                                $"which SpitCS can't do in the middle of execution.");
            }

            if (MustGenerate && MustDeleteOutput)
            {
                throw new ArgumentException("Specified instructions both to generate (-g) and clean (-c) output. " +
                                            "If you want to visit and eval a files code without generating output, use /g-");
            }

            if (MustGenerate && MustSkipCompute)
            {
                throw new ArgumentException("Specified instructions both to generate output (-g) and skip computation (-x).");
            }

            if (jsonSettings != null)
            {
                includes = jsonSettings.Includes ?? includes;
                Defines = jsonSettings.Defines ?? Defines;
                ConfigsByFiletype = jsonSettings.ConfigByFiletype ?? ConfigsByFiletype;
            }
        }

        public static void EnsureDefaultFiletypeConfig(Dictionary<string, FiletypeConfig> d)
        {
            if (d.ContainsKey("*")) return;

            d["*"] = new FiletypeConfig
            {
                StartSpitToken = "#if SPIT",
                StartOutputToken = "#else //SPIT",
                EndOutputToken = "#endif //SPIT",
                OutputSuffix = "",
                NewLine = "\r\n",
                EncodingWebName = Encoding.UTF8.WebName,
                ColdStartOutputToken = "#if !SPIT //Generated code",
                ColdEndOutputToken = "#endif //!SPIT"
            };
        }

        public void EnsureDefaultFiletypeConfig()
        {
            EnsureDefaultFiletypeConfig(ConfigsByFiletype);
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

    public class LineGroup
    {
        public List<string> InLines;
        public List<string> OutLines;
        public LineGroupType Type;
        public int StartingLineNumber;

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, " +
                   $"{nameof(InLines)}: [{InLines.Count}]({InLines.FirstOrDefault()}...), " +
                   $"{nameof(OutLines)}: [{OutLines.Count}]({OutLines.FirstOrDefault()}...)";
        }
    }

    public class SpitGlobalJob
    {
        public ScriptState ScriptState;
        public readonly string[] PrevIncludes;
        public readonly List<SpitJob> Jobs = new List<SpitJob>();

        public SpitGlobalJob(string[] includes)
        {
            PrevIncludes = includes;
        }

        public void AddJob(SpitJob newJob, string warningSrcFile = "", int warningLineNum = -1)
        {
            var jobExists = Jobs.Any(j =>
                j.Settings.InputFilePath == newJob.Settings.InputFilePath
                && j.Settings.OutputFilePath == newJob.Settings.OutputFilePath);

            var msg = $"Issued redundant preprocessor job, input file {newJob.Settings.InputFilePath}"
                      + $" and output file {newJob.Settings.OutputFilePath}";

            Util.AssertWarning(!jobExists, msg, warningSrcFile, warningLineNum);

            Jobs.Add(newJob);
        }
    }

    public interface ISpitJob
    {
        string PrevContent { get; }
        string PrevOutput { get; }
        string PrevCode { get; }
        IReadOnlyList<string> PrevContentLines { get; }
        IReadOnlyList<string> PrevOutputLines { get; }
        IReadOnlyList<string> PrevCodeLines { get; }
        IReadOnlyDictionary<string, object> Defines { get; }
        TextWriter SpitOut { get; }
        OneTimeScriptState SpitEval(
            string code,
            [CallerFilePath] string sourceFile = "UNKNOWN",
            [CallerLineNumber] int lineNumber = -1);
    }

    public class OneTimeScriptState : IDisposable
    {
        private static readonly List<OneTimeScriptState> Instances = new List<OneTimeScriptState>();

        private readonly ScriptState _scriptState;

        public ScriptState ScriptState
        {
            get
            {
                Dispose();
                return _scriptState;
            }
        }

        public string FileCreated { get; }
        public int LineNumberCreated { get; }

        private bool _disposed = false;

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

    // Global state across multiple files
    public class SpitJob : ISpitJob
    {
        public readonly SpitGlobalJob GW;

        public readonly SpitFileSettings Settings;

        // Long term state
        public readonly List<LineGroup> LineGroups = new List<LineGroup>();

        // Changes between blocks
        public StringWriter SpitOut;
        public List<string> PrevCodeLines = new List<string>();
        public List<string> PrevContentLines = new List<string>();
        public List<string> PrevOutputLines = new List<string>();

        #region ISpitWork
        IReadOnlyList<string> ISpitJob.PrevCodeLines => PrevCodeLines;
        IReadOnlyList<string> ISpitJob.PrevContentLines => PrevContentLines;
        IReadOnlyList<string> ISpitJob.PrevOutputLines => PrevOutputLines;
        TextWriter ISpitJob.SpitOut => SpitOut;
        IReadOnlyDictionary<string, object> ISpitJob.Defines => Settings.Defines;
        string ISpitJob.PrevCode => PrevCodeLines.AggLines();
        string ISpitJob.PrevContent => PrevContentLines.AggLines();
        string ISpitJob.PrevOutput => PrevOutputLines.AggLines();

        public OneTimeScriptState SpitEval(
            string code,
            [CallerFilePath] string sourceFile = "UNKNOWN",
            [CallerLineNumber] int lineNumber = -1)
        {
            return new OneTimeScriptState(Eval(code), sourceFile, lineNumber);
        }
        #endregion

        public SpitJob(string[] args, TextWriter helpMsgWriter)
        {
            string[] includes = null;
            Settings = new SpitFileSettings();
            Settings.Configure(args, helpMsgWriter, ref includes);
            Settings.EnsureDefaultFiletypeConfig();
            GW = new SpitGlobalJob(includes);
            GW.AddJob(this, null, 0);
        }

        public SpitJob(SpitJob existing, string[] args, TextWriter helpMsgWriter)
        {
            Settings = new SpitFileSettings();
            string[] prevIncludes = existing.GW.PrevIncludes;
            Settings.Configure(args, helpMsgWriter, ref prevIncludes);
            Settings.MergeInferior(existing.Settings);
            Settings.EnsureDefaultFiletypeConfig();
            GW = existing.GW;
        }

        public SpitFileSettings.FiletypeConfig Ftc => Settings.Ftc;

        public class LineGroupBound
        {
            public int InclStart;
            public int ExclEnd = -1;
            public LineGroupType Type;

            public override string ToString()
            {
                return $"{nameof(InclStart)}: {InclStart}, {nameof(ExclEnd)}: {ExclEnd}, {nameof(Type)}: {Type}";
            }
        }

        private string HashInfix => " HASH = ";
        private string HashRegex => $@"{HashInfix}\(([^)]*)\)";
        private string FmtHash(string hashCode) => $"{HashInfix}({hashCode})";

        public void Run()
        {
            if (GW.ScriptState == null)
            {
                GW.ScriptState = CSharpScript.RunAsync(
                    "",
                    ScriptOptions.Default
                        .AddReferences(
                            Assembly.GetAssembly(typeof(DynamicObject)), // System.Code
                            Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)), // Microsoft.CSharp
                            Assembly.GetAssembly(typeof(ExpandoObject))) // System.Dynamic
                        .AddImports(
                            "System",
                            "System.Collections.Generic",
                            "System.Linq",
                            "System.Text",
                            "System.Threading.Tasks",
                            "System.Dynamic",
                            "System.Reflection",
                            "System.Text",
                            "System.Text.RegularExpressions",
                            "System.IO",
                            "System.Runtime.CompilerServices") // TODO remove
                        .WithMetadataResolver(ScriptMetadataResolver.Default
                            .WithBaseDirectory(Environment.CurrentDirectory)
                            .WithSearchPaths(RuntimeEnvironment.GetRuntimeDirectory())
                            .WithSearchPaths(GW.PrevIncludes)
                        )
                        .WithEmitDebugInformation(true),
                        this, typeof(ISpitJob)).Result;
            }

            // Split line groups
            {
                var text = File.ReadAllText(Settings.InputFilePath, Encoding.GetEncoding(Ftc.EncodingWebName));
                var linesIn = text.EzSplit(Ftc.NewLine);

                var lineGroupBounds = new List<LineGroupBound>();
                lineGroupBounds.Add(new LineGroupBound
                {
                    InclStart = 0,
                    Type = LineGroupType.Content,
                });

                for (int index = 0; index < linesIn.Length; index++)
                {
                    var line = linesIn[index];

                    // TODO compress
                    switch (lineGroupBounds.Last().Type)
                    {
                        case LineGroupType.Content:
                            if (line.Contains(Ftc.StartSpitToken))
                            {
                                lineGroupBounds.Last().ExclEnd = index;
                                lineGroupBounds.Add(new LineGroupBound
                                {
                                    InclStart = index,
                                    ExclEnd = index + 1,
                                    Type = LineGroupType.StartSpitCode,
                                });
                                lineGroupBounds.Add(new LineGroupBound
                                {
                                    InclStart = index + 1,
                                    Type = LineGroupType.SpitCode,
                                });
                            }
                            break;
                        case LineGroupType.SpitCode:
                            if (line.Contains(Ftc.StartOutputToken))
                            {
                                lineGroupBounds.Last().ExclEnd = index;
                                lineGroupBounds.Add(new LineGroupBound
                                {
                                    InclStart = index,
                                    ExclEnd = index + 1,
                                    Type = LineGroupType.StartOutput,
                                });
                                lineGroupBounds.Add(new LineGroupBound
                                {
                                    InclStart = index + 1,
                                    Type = LineGroupType.SpitOutput,
                                });
                            }
                            break;
                        case LineGroupType.SpitOutput:
                            if (line.Contains(Ftc.EndOutputToken))
                            {
                                lineGroupBounds.Last().ExclEnd = index;
                                lineGroupBounds.Add(new LineGroupBound
                                {
                                    InclStart = index,
                                    ExclEnd = index + 1,
                                    Type = LineGroupType.EndOutput,
                                });
                                lineGroupBounds.Add(new LineGroupBound
                                {
                                    InclStart = index + 1,
                                    Type = LineGroupType.Content,
                                });
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                lineGroupBounds.Last().ExclEnd = linesIn.Length;

                Util.Assert(
                    lineGroupBounds.Last().Type == LineGroupType.Content,
                    "Doucment ends with unclosed " + lineGroupBounds.Last().Type,
                    Settings.InputFileName, lineGroupBounds.Last().InclStart + 1);

                LineGroups.AddRange(lineGroupBounds.Select(lgb => new LineGroup
                {
                    InLines = linesIn.Slice(lgb.InclStart, lgb.ExclEnd).ToList(),
                    Type = lgb.Type,
                    StartingLineNumber = lgb.InclStart + 1,
                }));
            }

            // Use line groups
            // TODO check/account for indents outside this function
            Evaluate();

            // Invalidating stuff which I know I don't want to be used again
            SpitOut = null;
            PrevCodeLines = null;
            PrevContentLines = null;
            PrevOutputLines = null;
        }

        private void Evaluate()
        {
            string Indent = "";
            string IndentWhiteSpace = "";
            string OldHash = null;
            List<string> PrevOutputIndented = null;
            int PrevCodeStartingLine = -1;

            foreach (var lineGroup in LineGroups)
            {
                switch (lineGroup.Type)
                {
                    case LineGroupType.Content:
                        {
                            PrevContentLines = lineGroup.InLines;
                            lineGroup.OutLines = lineGroup.InLines;
                        } break;
                    case LineGroupType.SpitCode:
                        {
                            PrevCodeLines = lineGroup.InLines.Select(l =>
                            {
                                Util.Assert(l.Contains(Indent), "Line not indented",
                                    Settings.InputFileName, lineGroup.StartingLineNumber);
                                return Indent.Length == 0 ? "" : l.Replace(Indent, "");
                            }).ToList();
                            lineGroup.OutLines = lineGroup.InLines;
                            PrevCodeStartingLine = lineGroup.StartingLineNumber;
                        } break;
                    case LineGroupType.SpitOutput:
                        {
                            OldHash = Util.CalculateMD5Hash(lineGroup.InLines.AggLines());

                            string[] outputLines;

                            try
                            {
                                outputLines = EvaluateCode(PrevCodeStartingLine);
                            }
                            catch (Exception e)
                            {
                                // TODO maybe handle here
                                throw;
                            }

                            PrevOutputLines = outputLines.ToList();
                            PrevOutputIndented = PrevOutputLines.Select(line => IndentWhiteSpace + line).ToList();
                            lineGroup.OutLines = PrevOutputIndented;
                        } break;
                    case LineGroupType.EndOutput:
                        {
                            var markerInd = lineGroup.InLines.First().IndexOf(Ftc.EndOutputToken);
                            var indent1 = lineGroup.InLines.First().Substring(0, markerInd);
                            Util.Assert(Indent == indent1, "indent mismatch",
                                Settings.InputFileName, lineGroup.StartingLineNumber);

                            string lastOutputHash = Util.CalculateMD5Hash(PrevOutputIndented.AggLines());

                            var line = lineGroup.InLines.Last();
                            var match = Regex.Match(line, HashRegex);
                            if (match.Success)
                            {
                                line = line.Replace(match.Value, "");
                                Util.Assert(match.Groups[1].Value == OldHash,
                                    "Code preceding hash does not match hash, which means that generated code has been edited.\n" +
                                    "Undo changes or remove Hash",
                                    Settings.InputFileName, lineGroup.StartingLineNumber);
                            }

                            lineGroup.OutLines = new List<string>
                            {
                                $"{line}{FmtHash(lastOutputHash)}"
                            };
                        } break;
                    case LineGroupType.StartSpitCode:
                        {
                            var markerInd = lineGroup.InLines.First().IndexOf(Ftc.StartSpitToken);
                            Indent = lineGroup.InLines.First().Substring(0, markerInd);
                            IndentWhiteSpace = "".PadRight(Indent.Length);
                            lineGroup.OutLines = lineGroup.InLines;
                        } break;
                    case LineGroupType.StartOutput:
                        {
                            var markerInd = lineGroup.InLines.First().IndexOf(Ftc.StartOutputToken);
                            var indent1 = lineGroup.InLines.First().Substring(0, markerInd);
                            Util.Assert(Indent == indent1, "indent mismatch", Settings.InputFileName, lineGroup.StartingLineNumber);
                            lineGroup.OutLines = lineGroup.InLines;
                        } break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void WriteAll()
        {
            foreach (var job in GW.Jobs)
            {
                job.WriteFile();
            }
        }

        private void WriteFile()
        {
            if (!Settings.MustDeleteOutput
                && !Settings.MustDeleteSpitCode
                && !Settings.MustGenerate)
            {
                return;
            }

            using (var writer = new StreamWriter(Settings.OutputFilePath, false, Settings.Encoding))
            {
                foreach (var lineGroup in LineGroups)
                {
                    IEnumerable<string> writtenLines = lineGroup.OutLines;

                    switch (lineGroup.Type)
                    {
                        case LineGroupType.SpitCode:
                        {
                            if (Settings.MustDeleteSpitCode) { writtenLines = null; }
                        } break;
                        case LineGroupType.SpitOutput:
                        {
                            if (!Settings.MustGenerate) { writtenLines = lineGroup.InLines;  }
                            if (Settings.MustDeleteOutput) { writtenLines = null; }
                        } break;
                        case LineGroupType.EndOutput:
                        {
                            // If user has elected not to generate, leave prev line (possibly including hash) alone
                            if (!Settings.MustGenerate) { writtenLines = lineGroup.InLines; }
                            
                            // If user wants output deleted, strip hash value from line (no matter the source)
                            // This is an unusual case :/
                            if (Settings.MustDeleteOutput)
                            {
                                writtenLines = writtenLines.Select(l => Regex.Replace(l, HashRegex, ""));
                            }

                            // or if spit code is deleted, replace with cold output token
                            if (Settings.MustDeleteSpitCode)
                            {
                                writtenLines = writtenLines
                                    .Select(l => l.Replace(Ftc.EndOutputToken, Ftc.ColdEndOutputToken))
                                    .Select(l => Regex.Replace(l, HashRegex, ""));
                            }
                        } break;
                        case LineGroupType.StartSpitCode:
                        {
                            if (Settings.MustDeleteSpitCode) { writtenLines = null; }
                        } break;
                        case LineGroupType.StartOutput:
                        {
                            if (Settings.MustDeleteSpitCode)
                            {
                                writtenLines = writtenLines
                                    .Select(l => l.Replace(Ftc.StartOutputToken, Ftc.ColdStartOutputToken));
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

        // public static string LoadToken => "@spitLoad";
        // public static string EvalToken => "@spitEval";
        public static string ForceDirective => "`force ";
        public static string YieldDirective => "`yield ";

        private string[] EvaluateCode(int startingLine)
        {
            SpitOut = new StringWriter();

            StringBuilder codeSb = new StringBuilder();
            string latestReturnValue = null;
            int lineNumber = startingLine - 1;

            void RunCollectedCode()
            {
                OneTimeScriptState.CheckInstancesDisposed();
                GW.ScriptState = Eval(codeSb.ToString());
                codeSb.Clear();
                var returnValue = GW.ScriptState.ReturnValue;
                var returnValueScriptState = returnValue as OneTimeScriptState;

                if (returnValueScriptState != null)
                {
                    GW.ScriptState = returnValueScriptState.ScriptState;
                }
                else if (returnValue != null)
                {
                    SpitOut.WriteLine(returnValue);
                }
            }

            foreach (var line in PrevCodeLines)
            {
                lineNumber++;
                bool forced = false;
                bool yield = false;
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith(ForceDirective))
                {
                    forced = true;
                    trimmedLine = trimmedLine.Replace(ForceDirective, "");
                }

                if (Settings.MustSkipCompute && !forced)
                {
                    continue;
                }

                if (trimmedLine.StartsWith(YieldDirective))
                {
                    yield = true;
                    trimmedLine = trimmedLine.Replace(YieldDirective, "");
                }

                codeSb.AppendLine($"#line {lineNumber} \"{Settings.InputFileName}\"");
                codeSb.AppendLine(trimmedLine);

                if (yield)
                {
                    RunCollectedCode();
                }
            }

            RunCollectedCode();

            var outputUnindented = SpitOut.GetStringBuilder().ToString();

            if (outputUnindented.EndsWith(Environment.NewLine))
            {
                outputUnindented = outputUnindented.Substring(0, outputUnindented.Length - Environment.NewLine.Length);
            }

            var outputLines = outputUnindented.EzSplit(Environment.NewLine);

            SpitOut.Dispose();
            SpitOut = null;

            return outputLines;
        }

        public ScriptState Eval(string code)
        {
            try
            {
                return GW.ScriptState.ContinueWithAsync(code).Result;
            }
            catch (AggregateException aggregateEx)
            {
                throw aggregateEx.InnerException ?? aggregateEx;
            }
        }
    }
}
