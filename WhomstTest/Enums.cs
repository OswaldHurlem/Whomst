﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    /*{{
        void OutputOptionEnum(string enumName, string content)
        {
            var enumVals = Regex.Matches(content, @"(?<!\w)" + enumName + @"\.\w+", RegexOptions.Multiline)
                .OfType<Match>().Select(m => m.Value.Replace($"{enumName}.", "")).Distinct();
            
            WhomstOut.WriteLine($"public enum {enumName}");
            WhomstOut.WriteLine("{");
    
            foreach (var v in enumVals)
            {
                WhomstOut.WriteLine($"    {v},");
            }
    
            WhomstOut.WriteLine("}");
        }
    }}*/
    //{}

    public class ReesesEater
    {
        void EatReeses(ReesesTechnique technique)
        {
            switch (technique)
            {
                case ReesesTechnique.TwoAtATime:
                    // ...
                    break;
                case ReesesTechnique.InASpiral:
                    // ...
                    break;
                case ReesesTechnique.ChoppedIntoSlices:
                    // ...
                    break;
                case ReesesTechnique.InIceCream:
                    //...
                    break;
                case ReesesTechnique.UpTheExhaust:
                    break;
            }
        }
    }

    /*{{ OutputOptionEnum("ReesesTechnique", PrevContent) }}*/
    //{}
}
