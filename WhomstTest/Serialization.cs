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
        //!! Serialize(string, ref int)
        //!! Serialize(string, ref string)
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
        public static void FastSerialize(string name, ref RTSEnemy rtsenemy)
        {
            FastSerializeTag(name);
            FastSerialize("Evilness", ref rtsenemy.Evilness);
            FastSerialize("Health", ref rtsenemy.Health);
            FastSerialize("PosX", ref rtsenemy.PosX);
            FastSerialize("PosY", ref rtsenemy.PosY);
        }
        //{} HASH: 26EF9AB2CBBD6FB1DD8A9EC82F89238C

        //!! Other uses: Cloning, ToString, GetHashCode, == operator, privacy-controlling interfaces, etc.
    }
}
