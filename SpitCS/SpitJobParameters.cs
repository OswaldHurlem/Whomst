using Mono.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FmtDict = System.Collections.Generic.Dictionary<string, SpitCS.FormatCfg>;
using DefineDict = System.Collections.Generic.Dictionary<string, object>;
using ISpitJob = SpitCS.SpitJob.ISpitJob;

// TODO ApplyDefaults/FillUnset semantics probably are stupid and should be replaced with an overwriting merge always
namespace SpitCS
{
    [DataContract] public abstract class SpitCfg<T> where T:SpitCfg<T>
    {
        protected abstract T GetProto();
        protected abstract T MergeOverWith(T overwriter);

        // public abstract T FillUnset(T proto);
        // protected abstract T CloneImpl();
        // protected abstract T GetDefault();
        // 
        // public virtual T ApplyDefaults()
        // {
        //     This.DefaultsAppled = true;
        //     var d = GetDefault();
        //     return This.FillUnset(GetDefault());
        // }
        // 
        // public T This => (T)this;
        // public bool DefaultsAppled { get; private set; }
        // 
        // public T Clone
        // {
        //     get
        //     {
        //         Util.Assert(!DefaultsAppled, $"Cloning a cfg with {nameof(DefaultsAppled)}");
        //         var t = CloneImpl();
        //         // Just as an added precaution
        //         t.DefaultsAppled = false;
        //         return t;
        //     }
        // }
        // 
        // public T WithSuperCfg(T superCfg)
        // {
        //     Util.Assert(superCfg != null, $"{nameof(superCfg)} is null");
        //     var superClone = superCfg.Clone;
        //     superClone.FillUnset(This);
        //     return superClone;
        // }
    }

    public static class SpitCfgUtil
    {
        public static T JsonClone<T>(this T obj)
        {
            var s = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(s);
        }

        public static FmtDict FillUnset(this FmtDict dict, FmtDict proto)
        {
            foreach (var kvp in proto)
            {
                FormatCfg valueAtKey = null;

                if (dict.TryGetValue(kvp.Key, out valueAtKey))
                {
                    valueAtKey.FillUnset(kvp.Value);
                }
                else
                {
                    dict[kvp.Key] = kvp.Value?.Clone;
                }
            }

            return dict;
        }

        public static DefineDict FillUnset(this DefineDict dict, DefineDict proto)
        {
            foreach (var kvp in dict)
            {
                if (!dict.ContainsKey(kvp.Key))
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return dict;
        }

        public static HashSet<string> FillUnset(this HashSet<string> set, HashSet<string> proto)
        {
            foreach (var s in proto)
            {
                if (!set.Contains(s))
                {
                    set.Add(s);
                }
            }

            return set;
        }
    }

    public struct Nothing
    {
        public static Nothing YesNothing => new Nothing();
    }

    // TODO maybe combine with FileCfg?? (maybe)
    [DataContract] public class StackCfg : SpitCfg<StackCfg>
    {
        [DataMember] public HashSet<string> Includes { get; set; } = null;
        [DataMember] public HashSet<string> Assemblies { get; set; } = null;
        [DataMember] public HashSet<string> Imports { get; set; } = null;
        [DataMember] public FormatCfg DefaultFormatCfg { get; set; } = new FormatCfg();
        [DataMember] public FmtDict FormatDict { get; set; } = new FmtDict();
        [DataMember] public DefineDict InitialDefines { get; set; } = new DefineDict();

        public static readonly StackCfg Default = new StackCfg
        {
            Includes = new HashSet<string>(),
            Assemblies = new HashSet<string>()
            {
                "System.Core",
                "Microsoft.CSharp",
            },
            Imports = new HashSet<string>()
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
            },
            DefaultFormatCfg = FormatCfg.Default,
            FormatDict = new FmtDict
            {
                [".html"] = FormatCfg.Html,
            },
            InitialDefines = new DefineDict
            {
                ["Credit"] = "/* Made with help from SpitCS: http://github.com/OswaldHurlem/SpitCS */",
            }
        };

        protected override StackCfg GetDefault() => Default;

        public override StackCfg FillUnset(StackCfg proto)
        {
            Util.Assert(proto != null, $"{nameof(proto)} is null");
            // TODO decide if lists should be appended this way
            Includes = Includes ?? proto.Includes;
            Assemblies = Assemblies ?? proto.Assemblies;
            Imports = Imports ?? proto.Imports;

            DefaultFormatCfg.FillUnset(proto.DefaultFormatCfg);
            FormatDict.FillUnset(proto.FormatDict);
            InitialDefines.FillUnset(proto.InitialDefines);
            return this;
        }

        protected override StackCfg CloneImpl() => this.JsonClone();

        public StackCfg WithDefaultFormatCfg(FormatCfg fmtCfg)
        {
            DefaultFormatCfg = fmtCfg.FillUnset(DefaultFormatCfg);
            return this;
        }

        public StackCfg AddFormatCfgs(FmtDict dict)
        {
            FormatDict = dict.FillUnset(FormatDict);
            return this;
        }

        public static StackCfg LoadJson(string filename)
        {
            return JsonConvert.DeserializeObject<StackCfg>(File.ReadAllText(filename));
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
        [DataMember] public string NewLine { get; set; } = null;
        [DataMember] public string EncodingWebName { get; set; } = null;

        public Encoding Encoding { get; set; } = null;

        [DataMember] public string EncodingName { get => Encoding.WebName; set => Encoding.GetEncoding(value); }

        public override FormatCfg FillUnset(FormatCfg proto)
        {
            Util.Assert(proto != null, $"{nameof(proto)} is null");
            var protoJson = JsonConvert.SerializeObject(proto);
            // NOTE JSON population is "fine" for this
            JsonConvert.PopulateObject(protoJson, this);
            return this;
        }

        public static readonly FormatCfg Default = new FormatCfg
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
            NewLine = "\r\n",
            EncodingWebName = Encoding.UTF8.WebName,
        };

        protected override FormatCfg GetDefault() => Default;

        protected override FormatCfg CloneImpl() => this.JsonClone();

        public static readonly FormatCfg Html = new FormatCfg
        {
            StartSpitToken = "<!-- SPIT",
            StartOutputToken = "-- --SPIT",
            EndOutputToken = "SPIT-->",
            ColdStartOutputToken = "<!--SPITGeneratedCode-->",
            ColdEndOutputToken = "<!--/SPITGeneratedCode-->",
            OutputSuffix = "",
            ForceDirective = "`force",
            YieldDirective = "`yield",
            HashInfix = " HASH:",
            NewLine = "\n",
            EncodingWebName = Encoding.UTF8.WebName,
        };
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

        private DefineDict _defines = new DefineDict();

        [DataMember] DefineDict Defines { get; set; } = new DefineDict();

        protected override FileCfg CloneImpl() => this.JsonClone();

        public override FileCfg FillUnset(FileCfg proto)
        {
            Util.Assert(proto != null, $"{nameof(proto)} is null");
            // NOTE JSON population is "fine" for this
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(proto), this);
            return this;
        }

        public StackCfg StackCfgForGettingDefault { private get; set; }

        protected override FileCfg GetDefault()
        {
            Util.Assert(StackCfgForGettingDefault != null, $"{nameof(StackCfgForGettingDefault)} is null");
            FormatCfg fmtCfg = new FormatCfg();
            StackCfgForGettingDefault.FormatDict.TryGetValue(InputFileExt, out fmtCfg);
            fmtCfg.FillUnset(StackCfgForGettingDefault.DefaultFormatCfg);
            fmtCfg.FillUnset(FormatCfg.Default);
            var cfg = new FileCfg
            {
                FormatCfg = fmtCfg,
            };
            cfg.Defines.FillUnset(StackCfgForGettingDefault.InitialDefines);
            return cfg;
        }

        public override FileCfg ApplyDefaults()
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

            return base.ApplyDefaults();
        }

        public FileCfg SetFile(string inFile, string outFile = null)
        {
            Util.Assert(inFile != null, "Input file cannot be null");
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
            dict.FillUnset(Defines);
            Defines = dict;
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
        public static void Test()
        {
            ISpitJob g;

            g.SpitLoad("blah.cs");

            FileCfg fileCfg = g.FileCfg;

            var f = fileCfg.SetFile("cum.cs").WithSuperCfg(new FileCfg
            {
                Generate = true,
            })
            .SetOperations(FileOp.DontGenerate | FileOp.SkipCompute)
            .AddDefines(new DefineDict());

            g.SpitLoad(f);
            g.SpitLoad(f, g.LoadCfg("ass.json"));

        }
    }
}