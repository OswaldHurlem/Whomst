using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    //!! A lot of Serialization systems in C# rely on run-time reflection
    //!! which can be quite slow!! What if all the reflection we need to perform
    //!! serialization was performed ahead of time?
    public static class Serializer
    {
        public static void FastSerializeTag(string name)
        {
            throw new NotImplementedException();
        }

        //!! This is to pretend like we have a bunch of overloads for
        //!! basic types a la LBP Method. ex:
        //!! FastSerialize(string, ref int)
        //!! FastSerialize(string, ref string)
        public static void FastSerialize<T>(string name, ref T value)
        {
            throw new NotImplementedException();
        }

        /*{{
            void OutputSerializer<T>()
            {
                var type = typeof(T);
                var tNameLower = type.Name.ToLower();
                WhomstOut.WriteLine($"public static void FastSerialize(string name, ref {type.Name} {tNameLower})");
                WhomstOut.WriteLine("{");
                WhomstOut.WriteLine("    FastSerializeTag(name);");
                foreach (var f in type.GetFields())
                {
                    WhomstOut.WriteLine($"    FastSerialize(\"{f.Name}\", ref {tNameLower}.{f.Name});");
                }
                WhomstOut.WriteLine("}");
            }
        
            OutputSerializer<RTSEnemy>();
        }}*/
        //{}

        //!! Other uses: Cloning, ToString, GetHashCode, == operator, privacy-controlling interfaces, etc.
    }
}
