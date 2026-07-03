using UnityEngine;

[CreateAssetMenu(fileName = "UnitSpriteData", menuName = "Game/Unit Sprite Data")]
public class UnitSpriteData : ScriptableObject
{
    [Header("Eyes Open")]
    public Sprite upOpen;
    public Sprite downOpen;
    public Sprite leftOpen;
    public Sprite rightOpen;

    [Header("Eyes Closed")]
    public Sprite upClosed;
    public Sprite downClosed;
    public Sprite leftClosed;
    public Sprite rightClosed;
    
    public Sprite GetSprite(Vector2 velocity, bool eyesClosed)
    {
        string direction = GetDirection(velocity);

        return eyesClosed ? GetClosedSprite(direction) : GetOpenSprite(direction);
    }

    private string GetDirection(Vector2 velocity)
    {
        if (velocity == Vector2.zero) return "down";

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        if (angle > -45f && angle <= 45f) return "right";
        if (angle > 45f && angle <= 135f) return "up";
        if (angle > 135f || angle <= -135f) return "left";

        return "down";
    }

    private Sprite GetOpenSprite(string dir) => dir switch
    {
        "up" => upOpen, "down" => downOpen, "left" => leftOpen, "right" => rightOpen, _ => downOpen
    };

    private Sprite GetClosedSprite(string dir) => dir switch
    {
        "up" => upClosed, "down" => downClosed, "left" => leftClosed, "right" => rightClosed, _ => downClosed
    };
}