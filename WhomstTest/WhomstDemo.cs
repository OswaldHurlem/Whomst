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
            Console.WriteLine("Foo");
            //{} HASH: D2EEF2AC76A50F4FCC0C390150A022E6

            //!! You can put the code in a single-line comment. You can also have output
            //!! contain multiple lines. Notice that variables persist between whomst blocks.
            /*{{ return $"{foo}\r\n{foo}\r\n{foo}"; }}*/
            Console.WriteLine("Foo");
            Console.WriteLine("Foo");
            Console.WriteLine("Foo");
            //{} HASH: A7CE3F3A42C41939F5E87CE638049931

            //!! Another form of whomst block is called a "one-liner." This type of block 
            //!! will generate output on the same as the code. With all whomst blocks, 
            //!! you're allowed to omit the "return" and semicolon on the last line.
            /*{foo}*/Console.WriteLine("Foo");/**/

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
            //{} HASH: D41D8CD98F00B204E9800998ECF8427E

            int x = 0;
            //!! What was that WhomstOut we just used? It's another way to output
            //!! values. Any text written to WhomstOut within the execution of a whomst
            //!! block gets added to your output, before any returned string.
            /*{{ UseWhomstOut(3); @"Console.WriteLine(x);" }}*/
            x++;
            x++;
            x++;
            Console.WriteLine(x);
            //{} HASH: 760E681129199BEEC4D865ABA2EB26C3

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
            Console.WriteLine(@"
            x++;
            x++;
            ");
            //{} HASH: 31A4268DCDC6B696DE18CB78E56BDE3D

            //!! C#Script is almost entirely the same thing as C# with a
            //!! few differences you can have someone else tell you about.
            //!! But I'll show you how to add an assembly and use it.
            /*{{
            #r"System.Numerics"
                using System.Numerics;
                
                Complex c = new System.Numerics.Complex(0, -1);
                $"var iSquared = {Complex.Multiply(c, c).Real};"
            }}*/
            var iSquared = -1;
            //{} HASH: A10819ACF90A42D4653FB81A644B5E3B

            //!! By default, whomst includes the assemblies needed for dynamically-typed
            //!! objects and expandos, which are very useful for scripting.
            /*{{
                dynamic d1 = new { Bar = 5 }; 
                dynamic d2 = new ExpandoObject();
                d2.Bar = 7;
                $"var BarSum = {d1.Bar + d2.Bar};"
            }}*/
            var BarSum = 12;
            //{} HASH: 2100CED1821800BC9B248D9CA9080807

            //!! You can also load C#Script files (.csx) with #load.

            //!! Whomst makes available the global variable PrevContentLines,
            //!! which is a list of all the lines. content between this and the previous whomst block.
            /*{{
                $"var prevContentLinesCount = {PrevContentLines.Count};"
            }}*/
            var prevContentLinesCount = 5;
            //{} HASH: E449E7D7DC14E0BA3CCF807619B56F55

            //!! There are some other properties and methods you have avaiable to you.
            /*{{
                var typeStrs = typeof(IWhomstGlobals).GetMembers().Select(t => t.ToString());
                var typeListStr = string.Join(Environment.NewLine, typeStrs);
                $"/*\r\n{typeListStr}\r\n*" + "/"
            }}*/
            /*
            Whomst.IWhomstGlobals get_Globals()
            System.String get_PrevContent()
            System.String get_PrevOutput()
            System.String get_PrevCode()
            System.Collections.Generic.IList`1[System.String] get_PrevContentLines()
            System.Collections.Generic.IList`1[System.String] get_PrevOutputLines()
            System.Collections.Generic.IList`1[System.String] get_PrevCodeLines()
            System.Collections.Generic.IDictionary`2[System.String,System.Object] get_Defines()
            System.IO.TextWriter get_WhomstOut()
            System.String AtString(System.String)
            Whomst.OneTimeScriptState WhomstEval(System.String, System.String, Int32)
            Whomst.FileCfg get_FileConfig()
            Whomst.StackCfg get_StackConfig()
            Whomst.OneTimeScriptState WhomstLoad(System.String, System.String, Whomst.FileOp, System.String, Int32)
            Whomst.OneTimeScriptState WhomstLoad(Whomst.FileCfg, Whomst.StackCfg, System.String, Int32)
            Whomst.IWhomstGlobals Globals
            System.String PrevContent
            System.String PrevOutput
            System.String PrevCode
            System.Collections.Generic.IList`1[System.String] PrevContentLines
            System.Collections.Generic.IList`1[System.String] PrevOutputLines
            System.Collections.Generic.IList`1[System.String] PrevCodeLines
            System.Collections.Generic.IDictionary`2[System.String,System.Object] Defines
            System.IO.TextWriter WhomstOut
            Whomst.FileCfg FileConfig
            Whomst.StackCfg StackConfig
            */
            //{} HASH: 14D917D5D3940BEE4755EE851FA9B7FF

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
            const float piSquared = 9.869589f;
            //{} HASH: BEEF8AA61ED91C4D3322D655A0449B36

            //!! Finally, from within a file, you can tell Whomst to preprocess another file
            //!! using WhomstLoad. This preprocesses the file and adds anything it defined
            //!! into the execution state for Whomst
            /*{{
                `yield WhomstLoad("DefineVariable.cs")
                stringDefinedInOtherFile
            }}*/
            Console.WriteLine("From other file!");
            //{} HASH: 24FA8DD77E8FD7C4DD086501E4079984

            //!! With these basic features understood, let's look at some use cases.
            /*{WhomstLoad("Interop.cs")}*//**/
            /*{WhomstLoad("DependencyInjection1.cs")}*//**/
            /*{WhomstLoad("Serialization.cs")}*//**/
            /*{WhomstLoad("Enums.cs")}*//**/
            /*{WhomstLoad("Limitations.cs")}*//**/
            /*{WhomstLoad("CardGame.cs")}*//**/
            /*{WhomstLoad("Optimization.cpp")}*//**/
            /*{WhomstLoad("Composition.cs")}*//**/
        }
    }
}
