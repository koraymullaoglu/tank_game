using UnityEngine;

public class PlayerTank : MonoBehaviour {
    public string playerId;
    private SpriteRenderer spriteRenderer;

[SerializeField] private Sprite greenTankSprite; // greentank.png için sprite
[SerializeField] private Sprite redTankSprite;   // redtank.png için sprite

void Awake()
{
    spriteRenderer = GetComponent<SpriteRenderer>();
    if (spriteRenderer == null)
    {
        Debug.LogError("SpriteRenderer bileşeni eksik!");
    }
}

public void SetPlayerId(string id)
{
    playerId = id;
    UpdateTankSprite();
}

public string GetPlayerId()
{
    return playerId;
}

    private void UpdateTankSprite()
    {
        if (playerId == "Player1")
        {
            spriteRenderer.sprite = greenTankSprite;
        }
        else
        {
            spriteRenderer.sprite = redTankSprite;
        }
    }

}