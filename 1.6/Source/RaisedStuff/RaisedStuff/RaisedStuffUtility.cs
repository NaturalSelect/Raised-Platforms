using System;
using UnityEngine;
using Verse;

namespace RaisedStuff;

public static class RaisedStuffUtility
{
    public static float losHeightOffset => RaisedStuff.Settings.lineOfSightHeightOffset;
    //public static float RaisedGridLevel(this IntVec3 c, Map map) => RaisedGridLevelBase(c, map) + ((map.roofGrid.RoofAt(c) is RoofDef roof) ? (roof.isThickRoof ? RaisedStuff.Settings.thickRoofHeight : RaisedStuff.Settings.defaultRoofHeight) : (c.GetEdifice(map)?.def?.fillPercent ?? 0));
    //public static int RaisedGridLevelBase(this IntVec3 c, Map map) => map.GetComponent<RaisedStuffManager>().raisedGrid[c];

    public static bool LineGoesAbove(IntVec3 start, IntVec3 end, IntVec3 cell, Map map, RaisedStuffManager cachedLevelManager = null)
    {
        if (cachedLevelManager == null)
        {
            cachedLevelManager = map.GetComponent<RaisedStuffManager>();
        }
        return CanPassThrough(cell, map, cachedLevelManager.raisedGrid[start], cachedLevelManager.raisedGrid[end], cell.DistanceTo(start) / start.DistanceTo(end));
    }

    public static bool CanPassThrough(IntVec3 c, Map map, float startHeight, float endHeight, float travelFraction, RaisedStuffManager cachedLevelManager = null)
    {
        if (cachedLevelManager == null)
        {
            cachedLevelManager = map.GetComponent<RaisedStuffManager>();
        }
        float lineHeight = travelFraction * (endHeight - startHeight) + startHeight + losHeightOffset;// - cachedLevelManager.raisedGrid[c];
        return !cachedLevelManager.FillAtHeight(c, lineHeight);
    }

    public static bool LineOfSight(IntVec3 start, IntVec3 end, Map map, bool skipFirstCell = false, Func<IntVec3, bool> validator = null, int halfXOffset = 0, int halfZOffset = 0, RaisedStuffManager cachedLevelManager = null)
    {
        if (!start.InBounds(map) || !end.InBounds(map))
        {
            return false;
        }
        bool flag = ((start.x != end.x) ? (start.x < end.x) : (start.z < end.z));
        int num = Mathf.Abs(end.x - start.x);
        int num2 = Mathf.Abs(end.z - start.z);
        int num3 = start.x;
        int num4 = start.z;
        int num5 = 1 + num + num2;
        int num6 = ((end.x > start.x) ? 1 : (-1));
        int num7 = ((end.z > start.z) ? 1 : (-1));
        num *= 4;
        num2 *= 4;
        num += halfXOffset * 2;
        num2 += halfZOffset * 2;
        int num8 = num / 2 - num2 / 2;
        IntVec3 intVec = default(IntVec3);

        float tdist = start.DistanceTo(end);

        if (cachedLevelManager == null)
        {
            cachedLevelManager = map.GetComponent<RaisedStuffManager>();
        }
        float startH = cachedLevelManager.raisedGrid[start];
        float endH = cachedLevelManager.raisedGrid[end];

        while (num5 > 1)
        {
            intVec.x = num3;
            intVec.z = num4;
            if (!skipFirstCell || intVec != start)
            {
                if (!CanPassThrough(intVec, map, startH, endH, intVec.DistanceTo(start) / tdist, cachedLevelManager:cachedLevelManager))
                {
                    return false;
                }
                if (validator != null && !validator(intVec))
                {
                    return false;
                }
            }

            if (num8 > 0 || (num8 == 0 && flag))
            {
                num3 += num6;
                num8 -= num2;
            }
            else
            {
                num4 += num7;
                num8 += num;
            }

            num5--;
        }

        return true;
    }

    public static bool LineOfSightToEdges(IntVec3 start, IntVec3 end, Map map, bool skipFirstCell = false, Func<IntVec3, bool> validator = null, RaisedStuffManager cachedLevelManager = null)
    {
        if (LineOfSight(start, end, map, skipFirstCell, validator, cachedLevelManager : cachedLevelManager))
        {
            return true;
        }

        int num = (start * 2).DistanceToSquared(end * 2);
        for (int i = 0; i < 4; i++)
        {
            if ((start * 2).DistanceToSquared(end * 2 + GenAdj.CardinalDirections[i]) <= num && LineOfSight(start, end, map, skipFirstCell, validator, GenAdj.CardinalDirections[i].x, GenAdj.CardinalDirections[i].z, cachedLevelManager : cachedLevelManager))
            {
                return true;
            }
        }

        return false;
    }

    public static bool LineOfSight(IntVec3 start, IntVec3 end, Map map, CellRect startRect, CellRect endRect, Func<IntVec3, bool> validator = null, bool forLeaning = false, RaisedStuffManager cachedLevelManager = null)
    {
        if (!start.InBounds(map) || !end.InBounds(map))
        {
            return false;
        }

        bool flag = ((start.x != end.x) ? (start.x < end.x) : (start.z < end.z));
        int num = Mathf.Abs(end.x - start.x);
        int num2 = Mathf.Abs(end.z - start.z);
        int num3 = start.x;
        int num4 = start.z;
        int num5 = 1 + num + num2;
        int num6 = ((end.x > start.x) ? 1 : (-1));
        int num7 = ((end.z > start.z) ? 1 : (-1));
        int num8 = num - num2;
        num *= 2;
        num2 *= 2;
        IntVec3 intVec = default(IntVec3);

        float tdist = start.DistanceTo(end);

        if (cachedLevelManager == null)
        {
            cachedLevelManager = map.GetComponent<RaisedStuffManager>();
        }
        float startH = cachedLevelManager.raisedGrid[start];
        float endH = cachedLevelManager.raisedGrid[end];

        while (num5 > 1)
        {
            intVec.x = num3;
            intVec.z = num4;
            if (endRect.Contains(intVec))
            {
                return true;
            }

            if (!startRect.Contains(intVec))
            {
                if (!CanPassThrough(intVec, map, startH, endH, intVec.DistanceTo(start) / tdist, cachedLevelManager: cachedLevelManager))
                {
                    return false;
                }

                if (validator != null && !validator(intVec))
                {
                    return false;
                }
            }

            if (num8 > 0 || (num8 == 0 && flag))
            {
                num3 += num6;
                num8 -= num2;
            }
            else
            {
                num4 += num7;
                num8 += num;
            }

            num5--;
        }

        return true;
    }

}
