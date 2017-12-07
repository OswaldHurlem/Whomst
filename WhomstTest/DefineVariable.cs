using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    class DefineVariable
    {
        static void PrintStupidThing()
        {
            /*{{
                var stringDefinedInOtherFile = "Console.WriteLine(\"From other file!\");";
                stringDefinedInOtherFile
            }}*/
            Console.WriteLine("From other file!");
            //{} HASH: 24FA8DD77E8FD7C4DD086501E4079984
        }
    }
}
