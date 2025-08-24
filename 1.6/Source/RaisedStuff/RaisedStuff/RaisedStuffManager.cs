using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RaisedStuff;

public class RaisedStuffManager : MapComponent
{
    public class FillColumn
    {
        public FillColumn((float, float)[] fillsInput)
        {
            this.fills = fillsInput;
        }

        private (float, float)[] fills;
        public bool FillAtHeight(float height)
        {
            foreach ((float bot, float top) fill in fills)
            {
                if (height > fill.bot && height <= fill.top)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public IntGrid raisedGrid;

    public FillColumn[] cachedFillColumnGrid;

    private int mapSizeX = 250;
    private int mapSizeZ = 250;

    private int tickInterval = 60;

    public bool FillAtHeight(IntVec3 c, float height)
    {
        return cachedFillColumnGrid[CellIndicesUtility.CellToIndex(c, mapSizeX)].FillAtHeight(height);
    }

    public RaisedStuffManager(Map map) : base(map)
    {
        if (raisedGrid == null)
        {
            raisedGrid = new IntGrid(map);
        }

        mapSizeX = map.Size.x;
        mapSizeZ = map.Size.z;

        cachedFillColumnGrid = new FillColumn[mapSizeX * mapSizeZ];

        // - empty column fill to prevent errors
        for (int i = 0; i < cachedFillColumnGrid.Length; i++)
        {
            cachedFillColumnGrid[i] = new FillColumn([]);
        }
    }

    public override void FinalizeInit()
    {
        CacheFillColumns();

        tickInterval = RaisedStuff.Settings.levelCachingTickInterval;
    }

    public override void MapComponentTick()
    {
        if (Find.TickManager.TicksGame % tickInterval == 0)
        {
            CacheFillColumns();
        }
    }

    public void CacheFillColumns()
    {
        for (int i = 0; i < raisedGrid.CellsCount; i++)
        {
            cachedFillColumnGrid[i] = MakeColumnFor(i);
        }
    }

    // 
    // FOR OTHER MODDERS : Patches altering height profiles should prefix/postfix this method!
    //
    protected FillColumn MakeColumnFor(int cellInd)
    {
        List<(float bot, float top)> fills = [];

        // : ground level
        fills.Add((0, raisedGrid[cellInd]));

        // : edifices
        if (map.edificeGrid[cellInd] is Building b)
        {
            fills.Add((raisedGrid[cellInd], raisedGrid[cellInd] + b.def.fillPercent));
        }

        // : roofs
        if (map.roofGrid.RoofAt(cellInd) is RoofDef roof)
        {
            fills.Add((raisedGrid[cellInd] + 1f, raisedGrid[cellInd] + (roof.isThickRoof ? RaisedStuff.Settings.thickRoofHeight : RaisedStuff.Settings.defaultRoofHeight)));
        }

        return new FillColumn(fills.ToArray());
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref raisedGrid, "raisedGrid", new IntGrid(base.map));
    }


}
