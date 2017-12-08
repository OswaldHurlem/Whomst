using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    /*{{ }}*/
    //{}
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

    public interface IWings
    {
        bool CanFly();
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
        
        OutputComposition("Voltron", typeof(IGun), typeof(ILegs), typeof(ISword), typeof(IHead), typeof(IWings));
    }}*/
    //{}
}
