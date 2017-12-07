using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    /*{{ }}*/
    //{} HASH: D41D8CD98F00B204E9800998ECF8427E
    public interface IGun
    {
        int Shoot();
    }

    public interface ILegs
    {
        int Legs();
    }

    public interface ISword
    {
        int Slash();
    }

    public interface IHead
    {
        int EyeLasers();
    }
    /*{{
        `yield WhomstEval(PrevContent)
        
        void OutputComposition(string className, params Type[] interfaceTypes)
        {
            WhomstOut.WriteLine($"public partial class {className} :");
            foreach (var t in interfaceTypes)
            {
                WhomstOut.WriteLine($"    {t.Name}" + (t == interfaceTypes.Last() ? "" : ","));
            }
        
            WhomstOut.WriteLine("{");
            
            foreach(var t in interfaceTypes)
            {
                WhomstOut.WriteLine($"    public {t.Name} {t.Name};");
        
                foreach (var m in t.GetMethods())
                {
                    WhomstOut.WriteLine($"    public {m.ReturnType} {m.Name}() => {t.Name}.{m.Name}();");
                }
            }
        
            WhomstOut.WriteLine("}");
        }
        
        OutputComposition("Voltron", typeof(IGun), typeof(ILegs), typeof(ISword), typeof(IHead));
    }}*/
    public partial class Voltron :
        IGun,
        ILegs,
        ISword,
        IHead
    {
        public IGun IGun;
        public System.Int32 Shoot() => IGun.Shoot();
        public ILegs ILegs;
        public System.Int32 Legs() => ILegs.Legs();
        public ISword ISword;
        public System.Int32 Slash() => ISword.Slash();
        public IHead IHead;
        public System.Int32 EyeLasers() => IHead.EyeLasers();
    }
    //{} HASH: 37D75DB55FF237FBD86255CE5E4A32C6
}
