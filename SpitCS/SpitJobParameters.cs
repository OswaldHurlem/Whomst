using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using FmtDict = System.Collections.Generic.Dictionary<string, SpitCS.FormatCfg>;
using DefineDict = System.Collections.Generic.Dictionary<string, object>;
using Newtonsoft.Json.Linq;

// TODO ApplyDefaults/FillUnset semantics probably are stupid and should be replaced with an overwriting merge always
namespace SpitCS
{
    public abstract class SpitCfg<T> where T:SpitCfg<T>
    {
        public T MergeOverWith(T overwriter)
        {
            var me = JObject.FromObject(this);
            var o = JObject.FromObject(overwriter);
            me.Merge(o, new JsonMergeSettings
            {
                MergeNullValueHandling = MergeNullValueHandling.Ignore,
                MergeArrayHandling = MergeArrayHandling.Union,
            });

            return me.ToObject<T>();
        }

        public string ToJson() => JsonConvert.SerializeObject(this);

        public static T FromJson(string s) => JsonConvert.DeserializeObject<T>(s);

        public T Clone() => FromJson(ToJson());
    }

    public struct Nothing
    {
        public static Nothing YesNothing => new Nothing();
    }

    // TODO maybe combine with FileCfg?? (maybe)
    [DataContract] public class StackCfg : SpitCfg<StackCfg>
    {
        [DataMember] public HashSet<string> Includes { get; set; } = new HashSet<string>();
        [DataMember] public HashSet<string> Assemblies { get; set; } = new HashSet<string>();
        [DataMember] public HashSet<string> Usings { get; set; } = new HashSet<string>();
        [DataMember] public FormatCfg DefaultFormatCfg { get; set; } = new FormatCfg();
        [DataMember] public FmtDict FormatDict { get; set; } = new FmtDict();
        [DataMember] public DefineDict InitialDefines { get; set; } = new DefineDict();
        
        public static StackCfg Default() => new StackCfg
        {
            Includes = new HashSet<string>(),
            Assemblies = new HashSet<string>()
            {
                "System.Core",
                "Microsoft.CSharp",
                typeof(StackCfg).Assembly.Location,
            },
            Usings = new HashSet<string>()
            {
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Text",
                "System.Dynamic",
                "System.Reflection",
                "System.Text",
                "System.Text.RegularExpressions",
                "System.IO",
                "System.Runtime.CompilerServices",
                nameof(SpitCS)
            },
            DefaultFormatCfg = FormatCfg.Default(),
            FormatDict = new FmtDict
            {
                [".html"] = FormatCfg.Html(),
            },
            InitialDefines = new DefineDict
            {
                ["Credit"] = "/* Made with help from SpitCS: http://github.com/OswaldHurlem/SpitCS */",
            }
        };

        public StackCfg WithDefaultFormatCfg(FormatCfg fmtCfg)
        {
            DefaultFormatCfg = DefaultFormatCfg.MergeOverWith(fmtCfg);
            return this;
        }

        public StackCfg AddFormatCfgs(FmtDict fmtCfgs)
        {
            foreach (var kvp in fmtCfgs)
            {
                FormatCfg f = null;
                if (!FormatDict.TryGetValue(kvp.Key, out f))
                {
                    f = new FormatCfg();
                }
                FormatDict[kvp.Key] = f.MergeOverWith(kvp.Value);
            }

            return this;
        }

        public static StackCfg LoadJson(string filename)
        {
            return FromJson(File.ReadAllText(filename));
        }

        public FileCfg GetProtoFileCfg(string fileExtension)
        {
            FormatCfg fmtCfg = FormatCfg.Default();
            fmtCfg = fmtCfg.MergeOverWith(DefaultFormatCfg);

            FormatDict.TryGetValue(fileExtension, out var fromFileExt);
            if (fromFileExt != null) { fmtCfg = fmtCfg.MergeOverWith(fromFileExt); }

            var fileCfg = new FileCfg() { FormatCfg = fmtCfg };
            fileCfg.AddDefines(InitialDefines);
            return fileCfg;
        }
    }

    [DataContract] public class FormatCfg : SpitCfg<FormatCfg>
    {
        [DataMember] public string StartSpitToken { get; set; } = null;
        [DataMember] public string StartOutputToken { get; set; } = null;
        [DataMember] public string EndOutputToken { get; set; } = null;
        [DataMember] public string ColdStartOutputToken { get; set; } = null;
        [DataMember] public string ColdEndOutputToken { get; set; } = null;
        [DataMember] public string OutputSuffix { get; set; } = null;
        [DataMember] public string ForceDirective { get; set; } = null;
        [DataMember] public string YieldDirective { get; set; } = null;
        [DataMember] public string HashInfix { get; set; } = null;
        [DataMember] public string NewL { get; set; } = null;

        public Encoding Encoding { get; set; } = null;

        [DataMember]
        public string EncodingName
        {
            get => Encoding?.WebName;
            set => Encoding = value == null ? null : Encoding.GetEncoding(value);
        }

        public static FormatCfg Default() => new FormatCfg
        {
            StartSpitToken = "#if SPIT",
            StartOutputToken = "#else //SPIT",
            EndOutputToken = "#endif //SPIT",
            ColdStartOutputToken = "#if !SPIT //Generated code",
            ColdEndOutputToken = "#endif //!SPIT",
            OutputSuffix = "",
            ForceDirective = "`force",
            YieldDirective = "`yield",
            HashInfix = " HASH:",
            NewL = "\r\n",
            EncodingName = Encoding.UTF8.WebName,
        };

        public static FormatCfg Html() => new FormatCfg
        {
            StartSpitToken = "<!-- SPIT",
            StartOutputToken = "-- --SPIT",
            EndOutputToken = "SPIT-->",
            ColdStartOutputToken = "<!--SPITGeneratedCode-->",
            ColdEndOutputToken = "<!--/SPITGeneratedCode-->",
            HashInfix = " HASH:",
            NewL = "\n",
            EncodingName = Encoding.UTF8.WebName,
        };

        public string HashRegex => $@"{HashInfix}\(([^)]*)\)";
        public string FmtHash(string hashCode) => $"{HashInfix}({hashCode})";
    }



    [DataContract] public class FileCfg : SpitCfg<FileCfg>
    {
        [DataMember] public FormatCfg FormatCfg { get; set; } = new FormatCfg();

        [DataMember] public string InputFilePath { get; set; } = null;
        [DataMember] public string OutputFilePath { get; set; } = null;

        public string InputFileName
        {
            get => Path.GetFileName(InputFilePath);
            set => InputFilePath = Path.GetFullPath(value);
        }

        public string InputFileExt => Path.GetExtension(InputFilePath);

        // TODO multiple output files each with an associated action
        public string OutputFileName
        {
            get => Path.GetFileName(OutputFilePath);
            set => OutputFilePath = Path.GetFullPath(value);
        }

        [DataMember] public bool? Generate { get; set; } = null;
        [DataMember] public bool? DeleteOutput { get; set; } = null;
        [DataMember] public bool? DeleteSpitCode { get; set; } = null;
        [DataMember] public bool? SkipCompute { get; set; } = null;

        public bool ShallGenerate       { get => Generate       ?? false; set => Generate       = value; }
        public bool ShallDeleteOutput   { get => DeleteOutput   ?? false; set => DeleteOutput   = value; }
        public bool ShallDeleteSpitCode { get => DeleteSpitCode ?? false; set => DeleteSpitCode = value; }
        public bool ShallSkipCompute    { get => SkipCompute    ?? false; set => SkipCompute    = value; }

        [DataMember] public DefineDict Defines { get; set; } = new DefineDict();

        public void Validate()
        {
            if (InputFilePath == null)
            {
                throw new ArgumentException("Input file (/i) must be specified");
            }

            if (Generate == null
                && SkipCompute == null
                && !ShallDeleteOutput
                && !ShallDeleteSpitCode)
            {
                Generate = true;
            }

            if (ShallGenerate && ShallDeleteOutput)
            {
                throw new ArgumentException("Specified instructions both to generate (/g) and clean (/c) output. " +
                                            "If you want to visit and eval a files code without generating output, use /g-");
            }

            if (ShallGenerate && ShallSkipCompute)
            {
                throw new ArgumentException("Specified instructions both to generate output (/g) and skip computation (/x).");
            }
        }

        public FileCfg SetFile(string inFile, string outFile = null)
        {
            Util.Assert(inFile != null, "Input and output files cannot be null");
            InputFileName = inFile;
            OutputFileName = outFile ?? inFile;
            return this;
        }

        public FileCfg SetOperations(FileOp flags)
        {   
            if (flags.HasFlag(FileOp.Generate)       && !flags.HasFlag(FileOp.DontGenerate))       { Generate       = true; }
            if (flags.HasFlag(FileOp.DeleteOutput)   && !flags.HasFlag(FileOp.DontDeleteOutput))   { DeleteOutput   = true; }
            if (flags.HasFlag(FileOp.DeleteSpitCode) && !flags.HasFlag(FileOp.DontDeleteSpitCode)) { DeleteSpitCode = true; }
            if (flags.HasFlag(FileOp.SkipCompute)    && !flags.HasFlag(FileOp.DontSkipCompute))    { SkipCompute    = true; }

            if (!flags.HasFlag(FileOp.Generate)       && flags.HasFlag(FileOp.DontGenerate))       { Generate       = false; }
            if (!flags.HasFlag(FileOp.DeleteOutput)   && flags.HasFlag(FileOp.DontDeleteOutput))   { DeleteOutput   = false; }
            if (!flags.HasFlag(FileOp.DeleteSpitCode) && flags.HasFlag(FileOp.DontDeleteSpitCode)) { DeleteSpitCode = false; }
            if (!flags.HasFlag(FileOp.SkipCompute)    && flags.HasFlag(FileOp.DontSkipCompute))    { SkipCompute    = false; }

            return this;
        }

        public FileCfg AddDefines(DefineDict dict)
        {
            foreach (var kvp in dict)
            {
                Defines[kvp.Key] = kvp.Value;
            }

            return this;
        }
    }

    [Flags]
    public enum FileOp
    {
        None = 0,
        Generate = 1 << 0,
        DeleteOutput = 1 << 1,
        DeleteSpitCode = 1 << 2,
        SkipCompute = 1 << 3,
        DontGenerate = 1 << 4,
        DontDeleteOutput = 1 << 5,
        DontDeleteSpitCode = 1 << 6,
        DontSkipCompute = 1 << 7,
    }
}

namespace SpitCS.User
{
    public static class Test
    {
    }
}