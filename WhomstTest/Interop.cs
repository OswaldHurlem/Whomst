using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace WhomstTest
{
    //!! Making it so that C++ code is callable by C# is often necessary and always a pain.
    //!! You have to spend a lot of time making sure that your types and function signatures
    //!! are the same between files of different languages. Could Whomst make this easier??
    /*{{ using System.Runtime.InteropServices; }}*/
    //{} HASH: D41D8CD98F00B204E9800998ECF8427E
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RTSEnemy
    {
        public float Evilness;
        public uint Health;
        public float PosX;
        public float PosY;
    }
    /*{{
        `yield WhomstEval(PrevContent)
        WhomstLoad("FakeCppFile.cpp")
    }}*/
    //{} HASH: D41D8CD98F00B204E9800998ECF8427E

    public static class NativeFunctions
    {
        /*{{
            foreach (var fs in funcSigs)
            {
                WhomstOut.WriteLine("[DllImport(\"SomeDll.dll\")]");
                WhomstOut.WriteLine($"public static extern unsafe {fs};");
            }
        }}*/
        [DllImport("SomeDll.dll")]
        public static extern unsafe void MoveEnemies(RTSEnemy* enemies, UInt32 enemies_count);
        [DllImport("SomeDll.dll")]
        public static extern unsafe void ClearDeadEnemies(
        	RTSEnemy* enemies, UInt32 enemies_count, UInt32* enemies_count_after);
        //{} HASH: 0457EEF6EEA1FC284BAC22FDC2029BB3
        
        //!! I also use something like this for shaders! :D
    }
}

