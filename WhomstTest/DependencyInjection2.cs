using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NurseRatchedInjectAttribute = System.ObsoleteAttribute; //fake
using IDatabaseWriter = System.Collections.Generic.IDictionary<string, object>; // fake
using CoconuttoDbWriter = System.Collections.Generic.Dictionary<string, object>;

namespace WhomstTest
{   
    //!! People who haven't done enterprise work might not know about this,
    //!! but there's this thing called a "Dependency Injection Framework"
    //!! which people like to make and then add to their codebase in order
    //!! to make it harder to read. Here is an example.
    public class BazingaAnalyticsCustomerImpressionView
    {
        //!! Very good Model-View-Controller right here
        public DateTime? ImpressionTime
        {
            get => DbWriter.ReadFromDb<DateTime?>();
            set => DbWriter.WriteToDb(value);
        }

        //!! Some other properties

        //!! At runtime, the NurseRatchedInject will use one of a million
        //!! little xml files to decide what implementation of IDatabaseWriter
        //!! to assign to DbWriter.
        //!! In theory, this allows you to set up multiple configurations of the 
        //!! application, with different implementations of IDatabaseWriter used.
        //!! That sounds great, until you find out that there's a bug in the
        //!! implementation you're using and it's hard to track down because
        //!! you can't find the xml file that says what the the implementation is.
        [NurseRatchedInject]
        public IDatabaseWriter DbWriter { get; set; }
    }

    public class BazingaAnalyticsCustomerImpressionView2
    {
        public DateTime? ImpressionTime
        {
            get => DbWriter.ReadFromDb<DateTime?>();
            set => DbWriter.WriteToDb<DateTime?>(value);
        }

        public IDatabaseWriter DbWriter { get; set; } = /*{implementations["IDatabaseWriter"]}*//**/;
    }



    //!! Haha those wild enteprise programmers, am I right?
    //!! Well... I've run into quite a few macro-heavy C files as well.
}
