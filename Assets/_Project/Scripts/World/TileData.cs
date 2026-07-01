using System.Collections.Generic;

public struct TileData
{
    public TileType tileType;
    public ResourceType resourceType;
    public TileModifiers[] modifiers;

    public List<BuildingData> buildings;

    public int[] unitsByFaction;

    public int factionId;

    public int regionId;

    public float foodAmount;

    public float elevation;

    public float moveCost;
}

public enum BuildingType
{
    None = 0,

    Farm = 1,
    Market = 2,

    Temple = 3,

    Wall = 4,
}

[System.Serializable]
public struct BuildingData
{
    public BuildingType type;
    public int level;
    public int factionId;
}