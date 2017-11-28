/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpitCS.User
{
    [DataContract]
    public class SpitJobParameters
    {
        [DataMember] public string OutputSuffix { get; set; }
        [DataMember] public string NewLine { get; set; }
        [DataMember] public string StartSpitToken { get; set; }
        [DataMember] public string StartOutputToken { get; set; }
        
        public string EndOutputToken = null;
        public string EncodingWebName = null;
        public string ColdStartOutputToken = null;
        public string ColdEndOutputToken = null;


        public string InputFilePath;
        public string OutputFilePath;
        public bool? Generate;
        public bool? DeleteOutput;
        public bool? DeleteSpitCode;
        public bool? SkipCompute;
    }

    public class SpitGlobalParameters
    {
        // public string[] Includes = { };
        // public Dictionary<string, object> Defines = new Dictionary<string, object>();
        // public Dictionary<string, FiletypeConfig> ConfigByFiletype = new Dictionary<string, FiletypeConfig>();
    }
}

namespace SpitCS.Test
{
    public static class Test
    {
        public static void Test()
        {
            ISpitJob g;

            g.Load()
        }
    }
}
*/