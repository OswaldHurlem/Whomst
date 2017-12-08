using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    /*{{
        List<string> GetStaticFieldNames(Type staticType, Type fieldType)
        {
            return staticType.GetFields().Where(fi => fi.IsStatic)
                .Where(fi => fi.FieldType == fieldType).Select(f => f.Name).ToList();
        }
    }}*/
    //{}
    public class Card
    {
        public string Name;
        public int Power;
        public int Toughness;
        public CardId Id;
        // ...
    }

    public static partial class Cards
    {
        public static Card MakeCard(int pow, int tough) => new Card { Power = pow, Toughness = tough };
        public static List<Card> AllCards;

        public static Card Tortoise = MakeCard(0, 3);
        public static Card Snake = MakeCard(3, 1);
        public static Card Lion = MakeCard(5, 3);
        public static Card Squirrel = MakeCard(0, 1);
        public static Card Rat = MakeCard(1, 1);
    }
    /*{{
        `yield WhomstEval("public enum CardId {}")
        WhomstEval(PrevContent)
    }}*/
    //{}

    partial class Cards
    {
        static Cards()
        {
            /*{{
                var fNames = GetStaticFieldNames(typeof(Cards), typeof(Card));
                 
                foreach (var fn in fNames)
                {
                    WhomstOut.WriteLine($"{fn}.Name = \"{fn}\";");
                }
            }}*/
            //{}

            /*{{ string.Join("\r\n", fNames.Select(fn => $"{fn}.Id = CardId.{fn};")) }}*/
            //{}

            AllCards = new List<Card>
            {
                /*{{ string.Join("\r\n", fNames.Select(fn => fn + ",")) }}*/
                //{}
            };
        }
    }

    public enum CardId
    {
        /*{{ string.Join("\r\n", fNames.Select(fn => fn + ",")) }}*/
        //{}
    }
}
