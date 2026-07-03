using UnityEngine;

public enum RulerCharacter
{
    Honorable,
    Treacherous,
    Generous,
    Aggressive,
    Diplomatic,
}

[CreateAssetMenu(fileName = "NewFaction", menuName = "Game/Faction")]
public class FactionData : ScriptableObject
{
    public int factionId;

    public string factionName;
    public string rulerName;

    public UnitSpriteData unitSpriteData;

    public Vector2Int startingHex;

    public Color factionColor;

    public float startingFood = 250f;

    public int startingUnits = 5;

    public RulerCharacter rulerCharacter;

    public bool isPlayer;

    public Sprite unitSprite;
    
    public Color selectionColor = Color.green;
}