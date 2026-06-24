public struct TileData
{
    public TileType tileType;
    public ResourceType resourceType;
    public TileModifiers[] modifiers;

    public int[] unitsByFaction;

    public int factionId;

    public int regionId;

    public float foodAmount;

    public int elevation;

    public float moveCost;
}