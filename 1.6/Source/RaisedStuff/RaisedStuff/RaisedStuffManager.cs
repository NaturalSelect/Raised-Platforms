using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace RaisedStuff;

[StaticConstructorOnStartup]
public class RaisedStuffManager : MapComponent
{
    private static int maxFillEntries = 5;

    public enum FillType
    {
        Terrain,
        Edifice,
        Roof
    }
    public class FillColumn
    {
        public FillColumn((float, float)[] fillsInput)
        {
            this.fills = fillsInput;
        }

        public (float, float)[] fills;
        public bool FillAtHeight(float height)
        {
            foreach ((float bot, float top) fill in fills)
            {
                if (fill == (0f, 0f)) continue;
                if (height > fill.bot && height <= fill.top)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public IntGrid raisedGrid;

    public FillColumn[] cachedFillColumnGrid;

    private int mapSizeX = 250;
    private int mapSizeZ = 250;

    private int tickInterval = 60;

    public bool FillAtHeight(IntVec3 c, float height)
    {
        return cachedFillColumnGrid[CellIndicesUtility.CellToIndex(c, mapSizeX)]?.FillAtHeight(height) ?? false;
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
    }

    public override void FinalizeInit()
    {
        if (raisedGrid == null)
        {
            raisedGrid = new IntGrid(map);
        }
        if (cachedFillColumnGrid == null)
        {
            mapSizeX = map.Size.x;
            mapSizeZ = map.Size.z;
            cachedFillColumnGrid = new FillColumn[mapSizeX * mapSizeZ];
        }

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
            if (cachedFillColumnGrid[i] is null)
            {
                cachedFillColumnGrid[i] = new FillColumn(new (float, float)[maxFillEntries]);//new FillColumn(new (float, float)[5]);
            }
            UpdateColumn(cachedFillColumnGrid[i], i);
        }
    }

    // 
    // FOR OTHER MODDERS : Patches altering height profiles should prefix/postfix this method!
    //

    protected void UpdateColumn(FillColumn fc, int cellInd)
    {

        fc.fills[(int)FillType.Terrain] = (-20, raisedGrid[cellInd]);

        fc.fills[(int)FillType.Edifice] = (map.edificeGrid[cellInd] is Building b) ? (raisedGrid[cellInd], raisedGrid[cellInd] + b.def.fillPercent) : (0, 0);

        fc.fills[(int)FillType.Roof] = (map.roofGrid.RoofAt(cellInd) is RoofDef roof) ? (raisedGrid[cellInd] + 1f, raisedGrid[cellInd] + (roof.isThickRoof ? RaisedStuff.Settings.thickRoofHeight : RaisedStuff.Settings.defaultRoofHeight)) : (0, 0);
    }

    public override void ExposeData()
    {
        MapExposeUtility.ExposeInt(
            map,
            (IntVec3 c) => raisedGrid[map.cellIndices.CellToIndex(c)],
            delegate (IntVec3 c, int val) {raisedGrid[map.cellIndices.CellToIndex(c)] = val;},
            "raisedGrid"
        );
    }


}
