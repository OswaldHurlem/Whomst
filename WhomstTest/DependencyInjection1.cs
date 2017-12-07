using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    public class NurseRatchedInjectAttribute : Attribute
    {

    }

    public interface IDatabaseWriter
    {
        T ReadFromDb<T>([CallerMemberName]string name = null);
        void WriteToDb<T>(T value, [CallerMemberName]string name = null);
    }

    public class CoconuttoDbWriter : IDatabaseWriter
    {
        public T ReadFromDb<T>([CallerMemberName] string name = null)
        {
            throw new NotImplementedException();
        }

        public void WriteToDb<T>(T value, [CallerMemberName] string name = null)
        {
            throw new NotImplementedException();
        }
    }

    public static class HydraMox
    {
        public static T CreateMock<T>()
        {
            return default(T);
        }
    }

    /*{{
        var implementations = new Dictionary<string, string>
        {
            ["IDatabaseWriter"] = "new CoconuttoDbWriter()",
            // a bajillion other implementation assignments
        };
        `yield null
        WhomstLoad("DependencyInjection2.cs")
    }}*/
    //{} HASH: D41D8CD98F00B204E9800998ECF8427E
}
