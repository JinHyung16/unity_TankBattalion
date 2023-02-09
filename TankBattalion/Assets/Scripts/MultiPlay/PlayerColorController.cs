using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayerColor
{
    None,
    Green,
}

public class PlayerColorController : MonoBehaviour
{
    public PlayerColor Color = PlayerColor.None;
    public SpriteRenderer tankSprite;

    private void Start()
    {
        tankSprite.color = new Color(1, 1, 1);
    }
    public void SetColor(PlayerColor color)
    {
        switch (color)
        {
            case PlayerColor.None:
                tankSprite.color = new Color(1, 1, 1);
                break;
            case PlayerColor.Green:
                tankSprite.color = new Color(0, 1, 0);
                break;
        }
    }
    public void SetColor(int colorIndex)
    {
        var colorType = typeof(PlayerColor);
        var colors = colorType.GetEnumValues();
        var playerColor = (PlayerColor)colors.GetValue(colorIndex % colors.Length);
        SetColor(playerColor);
    }
}
