using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RaisedStuff;

public class RaisedStuffManager : MapComponent
{
    public IntGrid raisedGrid;
    public RaisedStuffManager(Map map) : base(map)
    {
        raisedGrid = new IntGrid(map);
    }
}
