using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    class WhomstDemo
    {
        static void Main(string[] args)
        {
            //!! Based on the python tool "cog," Whomst is a tool for generating code 
            //!! in any language using inline code blocks.  This differs from tools
            //!! which either generate assembly code as a build step, or code generators
            //!! which separate the generator and generated code into separate files.

            //!! Basic use case: within a comment block with special markings 
            //!! (/*+{{ and }}+*/ by default), write a C#Script snippet that returns a string.
            //!! Write an end-of-output indicator (//+{}) on the next line. The code
            //!! will execute, and its return value will appear between the whomst block
            //!! and end-of-output mark.
            /*{{
                var foo = @"Console.WriteLine(""Foo"");";
                return foo;
            }}*/
            //{}

            //!! You can put the code in a single-line comment. You can also have output
            //!! contain multiple lines. Notice that variables persist between whomst blocks.
            /*{{ return $"{foo}\r\n{foo}\r\n{foo}"; }}*/
            //{}

            //!! Another form of whomst block is called a "one-liner." This type of block 
            //!! will generate output on the same as the code. With all whomst blocks, 
            //!! you're allowed to omit the "return" and semicolon on the last line.
            /*{foo}*//**/

            //!! You can define functions and use them later. You can also have
            //!! whomst blocks which don't return a value.
            /*{{
                public void UseWhomstOut(int n)
                {
                    foreach (var i in Enumerable.Range(0, n))
                    {
                        WhomstOut.WriteLine("x++;");
                    }
                }
            }}*/
            //{}

            int x = 0;
            //!! What was that WhomstOut we just used? It's another way to output
            //!! values. Any text written to WhomstOut within the execution of a whomst
            //!! block gets added to your output, before any returned string.
            /*{{ UseWhomstOut(3); @"Console.WriteLine(x);" }}*/
            //{}

            //!! Now for something a little weird. The requirement to have each code snippet
            //!! between a special character can get annoying
            //!! (especially under ~certain circumstances~ that will come up later).
            //!! To get around this, You can start a line with `yield.
            //!! `yield will create a new code snippet on the line that comes after it.
            /*{{
                `yield "Console.WriteLine(@\""
                UseWhomstOut(2);
                "\");"
            }}*/
            //{}

            //!! C#Script is almost entirely the same thing as C# with a
            //!! few differences you can have someone else tell you about.
            //!! But I'll show you how to add an assembly and use it.
            /*{{
            #r"System.Numerics"
                using System.Numerics;
                
                Complex c = new System.Numerics.Complex(0, -1);
                $"var iSquared = {Complex.Multiply(c, c).Real};"
            }}*/
            //{}

            //!! By default, whomst includes the assemblies needed for dynamically-typed
            //!! objects and expandos, which are very useful for scripting.
            /*{{
                dynamic d1 = new { Bar = 5 }; 
                dynamic d2 = new ExpandoObject();
                d2.Bar = 7;
                $"var BarSum = {d1.Bar + d2.Bar};"
            }}*/
            //{}





            //!! You can also load C#Script files (.csx) with #load.

            //!! Whomst makes available the global variable PrevContentLines,
            //!! which is a list of all the lines. content between this and the previous whomst block.
            /*{{
                $"var prevContentLinesCount = {PrevContentLines.Count};"
            }}*/
            //{}

            //!! There are some other properties and methods you have avaiable to you.
            /*{{
                var typeStrs = typeof(IWhomstGlobals).GetMembers().Select(t => t.ToString());
                var typeListStr = string.Join(Environment.NewLine, typeStrs);
                return $"/*\r\n{typeListStr}\r\n*" + "/";
            }}*/
            //{}

            const float pi = 3.14159f;

            //!! There are some other properties and methods you have avaiable to you.
            //!! One of these is WhomstEval which will evaluate a string containing the
            //!! the code you pass into it, and make that part of your execution state.
            //!! WhomstEval must be provided as a return value -- this is why `yield and
            //!! globals like PrevContent exist
            /*{{
                `yield WhomstEval(PrevContent)
                $"const float piSquared = {pi*pi}f;"
            }}*/
            //{}

            //!! Finally, from within a file, you can tell Whomst to preprocess another file
            //!! using WhomstLoad. This preprocesses the file and adds anything it defined
            //!! into the execution state for Whomst
            /*{{
                `yield WhomstLoad("DefineVariable.cs")
                stringDefinedInOtherFile
            }}*/
            //{}

            //!! With these basic features understood, let's look at some use cases.
            /*{WhomstLoad("Interop.cs")}*//**/
            /*{WhomstLoad("DependencyInjection1.cs")}*//**/
            /*{WhomstLoad("Serialization.cs")}*//**/
            /*{WhomstLoad("Enums.cs")}*//**/
            /*{WhomstLoad("Limitations.cs")}*//**/
            /*{WhomstLoad("CardGame.cs")}*//**/
            /*{WhomstLoad("Optimization.cpp")}*//**/
            /*{WhomstLoad("Composition.cs")}*//**/

            /*{{
                var fName = "GetInt";
            }}*/
            //{}
            
            int z = /*{fName}*//**/();
        }
    }
}
