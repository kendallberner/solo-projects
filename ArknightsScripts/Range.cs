using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range
{
    public List<XYPairing> pairings = new List<XYPairing>();
    public float originX;
    public float originY;
    public DIRECTION Direction;

    public struct RangeBuildingBlock
    {
        public float minXMod;
        public float maxXMod;
        public float minYMod;
        public float maxYMod;
    }

    public class XYPairing
    {
        public Tuple<float, float> xRange;
        public Tuple<float, float> yRange;

        public bool AreCoordsWithinRange(float x, float y)
        {
            if (x >= xRange.Item1 && x <= xRange.Item2)
                if (y >= yRange.Item1 && y <= yRange.Item2)
                    return true;
            return false;
        }

        public Vector3 GetCenter()
        {
            return new Vector3((xRange.Item2 + xRange.Item1) / 2f, 0f, (yRange.Item2 + yRange.Item1) / 2f);
        }

        public Vector3 GetScale()
        {
            return new Vector3(xRange.Item2 - xRange.Item1, .01f, yRange.Item2 - yRange.Item1);
        }
    }

    public bool AreCoordsWithinRange(float x, float y)
    {
        foreach (XYPairing pairing in pairings)
        {
            if (pairing.AreCoordsWithinRange(x, y))
                return true;
        }
        return false;
    }

    public void AddPairingToRange(float lowX, float highX, float lowY, float highY)
    {
        XYPairing xyPairing = new XYPairing();

        Tuple<float, float> xTuple = new Tuple<float, float>(lowX, highX);
        Tuple<float, float> yTuple = new Tuple<float, float>(lowY, highY);

        xyPairing.xRange = xTuple;
        xyPairing.yRange = yTuple;

        pairings.Add(xyPairing);
    }

    public List<Vector3> GetPositionCenters()
    {
        List<Vector3> centers = new List<Vector3>();
        foreach (XYPairing pairing in pairings)
        {
            centers.Add(pairing.GetCenter());
        }
        return centers;
    }

    public List<Vector3> GetScales()
    {
        List<Vector3> scales = new List<Vector3>();
        foreach (XYPairing pairing in pairings)
        {
            scales.Add(pairing.GetScale());
        }
        return scales;
    }

    public static Range GetRangeFromBuildingBlocks(List<RangeBuildingBlock> buildingBlocks, float originX, float originY, DIRECTION direction)
    {
        Range range = new Range();
        range.originX = originX;
        range.originY = originY;
        range.Direction = direction;

        foreach (RangeBuildingBlock block in buildingBlocks)
        {
            float minXMod = block.minXMod;
            float maxXMod = block.maxXMod;
            float minYMod = block.minYMod;
            float maxYMod = block.maxYMod;

            switch (direction)
            {
                case DIRECTION.RIGHT:
                    range.AddPairingToRange(originX + minXMod, originX + maxXMod, originY + minYMod, originY + maxYMod);
                    break;
                case DIRECTION.UP:
                    range.AddPairingToRange(originX + minYMod, originX + maxYMod, originY + minXMod, originY + maxXMod);
                    break;
                case DIRECTION.LEFT:
                    range.AddPairingToRange(originX - maxXMod, originX - minXMod, originY - maxYMod, originY - minYMod);
                    break;
                case DIRECTION.DOWN:
                    range.AddPairingToRange(originX - maxYMod, originX - minYMod, originY - maxXMod, originY - minXMod);
                    break;
            }
        }

        return range;
    }

    // O
    public static List<RangeBuildingBlock> Get1x1BuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    // O X
    public static List<RangeBuildingBlock> Get1x2BuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X
    //O X
    //X
    public static List<RangeBuildingBlock> GetCloseRangeBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //  X
    //X O X
    //  X
    public static List<RangeBuildingBlock> GetAdjacentBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = GetCloseRangeBuildingBlocks();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X
    //O X X
    //X X
    public static List<RangeBuildingBlock> GetWideRangeMedicCloseRangeBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //  X X
    //O X X
    //  X X
    public static List<RangeBuildingBlock> GetShotgunBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X
    //O X X X
    //X X
    public static List<RangeBuildingBlock> GetRangedGuardBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X X
    //O X X
    //X X X
    public static List<RangeBuildingBlock> Get3x3BuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X X
    //X O X
    //X X X
    public static List<RangeBuildingBlock> Get3x3CenteredBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //    X
    //  X X X
    //X X O X X
    //  X X X
    //    X
    public static List<RangeBuildingBlock> GetAuraBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X X
    //O X X X
    //X X X
    public static List<RangeBuildingBlock> Get3x3And1BuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = Get3x3BuildingBlocks();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X X X
    //X O X X
    //X X X X
    public static List<RangeBuildingBlock> GetSupporterBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //  X X
    //X X X X
    //X O X X
    //X X X X
    //  X X
    public static List<RangeBuildingBlock> GetWideSupporterBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = GetSupporterBuildingBlocks();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = -2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //  X X X
    //X X X X X
    //X X O X X
    //X X X X X
    //  X X X
    public static List<RangeBuildingBlock> GetWideAuraBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X X X
    //O X X X
    //X X X X
    public static List<RangeBuildingBlock> Get3x4BuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //    X X
    //    X X X
    //O   X X X
    //    X X X
    //    X X
    public static List<RangeBuildingBlock> GetHeavyBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 4.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X X
    //X X X X
    //O X X X
    //X X X X
    //X X X
    public static List<RangeBuildingBlock> GetWideRangeMedicBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X X
    //X X X X
    //O X X X X
    //X X X X
    //X X X
    public static List<RangeBuildingBlock> GetSniperBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = GetWideRangeMedicBuildingBlocks();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 4.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X X X X
    //O X X X X
    //X X X X X
    public static List<RangeBuildingBlock> Get3x5BuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 4.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X X X X X X X
    //O X X X X X X
    //X X X X X X X
    public static List<RangeBuildingBlock> Get3x7BuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 6.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }

    //X
    //X X
    //X X X
    //O X X X
    //X X X
    //X X
    //X
    public static List<RangeBuildingBlock> GetTruesilverSlashBuildingBlocks()
    {
        List<RangeBuildingBlock> rangeBuildingBlocks = new List<RangeBuildingBlock>();

        RangeBuildingBlock rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = 1.5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        rangeBuildingBlock = new RangeBuildingBlock();
        rangeBuildingBlock.minXMod = 2.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxXMod = 3.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.minYMod = -.5f * Constants.NODE_WIDTH;
        rangeBuildingBlock.maxYMod = .5f * Constants.NODE_WIDTH;
        rangeBuildingBlocks.Add(rangeBuildingBlock);

        return rangeBuildingBlocks;
    }
}
