using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    public enum FieldType
    {
        House,
        Field,
        Dirt,
        Pasture,
        PumpkinPatch,
    }

    class Prologgy
    {
        #if DECLARATIVE_CODEGEN
            var [16] var FieldType Fields
            var int Sheep
            var int Cows
            var int Pumpkins
            var int Grains
            var int Bread
            var bool UsingOven
            var int Goblins

            bool Starving := (Sheep + Cows + Pumpkins + Bread) > 10
            int HouseSize := count(fields, @ == House)
            
            Delta<Bread> := UsingOven ? 1 : 0
            Delta<Grains> := UsingOven ? -1 : 0
            Delta<Goblins> := count(fields, @ == PumpkinPatch)/4

            int Score := (Starving ? -3 : 0) + (Goblins > 2 ? 4 : 0) + HouseSize
        #endif
        struct FrameState
        {
            public FieldType[] Fields;
            public int Sheep;
            public int Cows;
            public int Pumpkins;
            public int Grains;
            public int Bread;
            public bool UsingOven;
            public int Goblins;
            public bool Starving;
            public int HouseSize;
            public int Score;
        }

        FrameState Prev;
        FrameState Curr;

        public void Advance()
        {
            Curr = Prev;
            Curr.Starving = (Prev.Sheep + Prev.Cows + Prev.Pumpkins + Prev.Bread) > 10;
            Curr.HouseSize = Prev.Fields.Count(f => f == FieldType.House);
            Curr.Bread = Prev.Bread + (Prev.UsingOven ? 1 : 0);
            Curr.Grains = Prev.Grains + (Prev.UsingOven ? -1 : 0);
            Curr.Score = (Prev.Starving ? -3 : 0) + (Prev.Goblins > 2 ? 4 : 0) + Prev.HouseSize;
        }
    }
}
