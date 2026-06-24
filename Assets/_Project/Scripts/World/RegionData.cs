using UnityEngine;
using System.Collections.Generic;

public struct RegionData
{
    public int id;

    public int factionId;

    public bool isConstested;

    public List<Vector2Int> tiles;
}