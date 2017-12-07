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
    //{} HASH: D41D8CD98F00B204E9800998ECF8427E
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
    //{} HASH: D41D8CD98F00B204E9800998ECF8427E

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
            Tortoise.Name = "Tortoise";
            Snake.Name = "Snake";
            Lion.Name = "Lion";
            Squirrel.Name = "Squirrel";
            Rat.Name = "Rat";
            //{} HASH: 453059FEC98452BB44B9B79C8BBC3BE9

            /*{{ string.Join("\r\n", fNames.Select(fn => $"{fn}.Id = CardId.{fn};")) }}*/
            Tortoise.Id = CardId.Tortoise;
            Snake.Id = CardId.Snake;
            Lion.Id = CardId.Lion;
            Squirrel.Id = CardId.Squirrel;
            Rat.Id = CardId.Rat;
            //{} HASH: 6F3B2E61D9C6C137DA1CEE07236241B0

            AllCards = new List<Card>
            {
                /*{{ string.Join("\r\n", fNames.Select(fn => fn + ",")) }}*/
                Tortoise,
                Snake,
                Lion,
                Squirrel,
                Rat,
                //{} HASH: 6C4C2C598DE7D3225DD2D160E5AD7125
            };
        }
    }

    public enum CardId
    {
        /*{{ string.Join("\r\n", fNames.Select(fn => fn + ",")) }}*/
        Tortoise,
        Snake,
        Lion,
        Squirrel,
        Rat,
        //{} HASH: 6C4C2C598DE7D3225DD2D160E5AD7125
    }
}
