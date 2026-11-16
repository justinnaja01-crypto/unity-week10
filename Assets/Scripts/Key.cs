using UnityEngine;

public enum KeyColor
{
    Blue,
    Green,
    Red,
    Yellow
}

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class Key : MonoBehaviour
{
    [Header("Key Configuration")]
    [SerializeField] private KeyColor keyColor = KeyColor.Blue;
    [SerializeField] private AudioClip pickupSound;

    [Header("Sprites for Each Color")]
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite greenSprite;
    [SerializeField] private Sprite redSprite;
    [SerializeField] private Sprite yellowSprite;

    private SpriteRenderer spriteRenderer;

    public KeyColor Color
    {
        get => keyColor;
        set
        {
            keyColor = value;
            UpdateSprite();
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        EnsureSpritesLoaded();
        UpdateSprite();
    }

    private void OnValidate()
    {
        EnsureSpritesLoaded();
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            Sprite s = GetSpriteForColor(keyColor);
            if (s != null)
            {
                spriteRenderer.sprite = s;
            }
        }
    }

    private Sprite GetSpriteForColor(KeyColor col)
    {
        switch (col)
        {
            case KeyColor.Blue: return blueSprite;
            case KeyColor.Green: return greenSprite;
            case KeyColor.Red: return redSprite;
            case KeyColor.Yellow: return yellowSprite;
            default: return blueSprite;
        }
    }

    public void EnsureSpritesLoaded()
    {
#if UNITY_EDITOR
        string dir = "Assets/Art/Kenney/kenney_new-platformer-pack-1.1/Sprites/Tiles/Default/";
        if (blueSprite == null) blueSprite = LoadSubSprite(dir + "key_blue.png");
        if (greenSprite == null) greenSprite = LoadSubSprite(dir + "key_green.png");
        if (redSprite == null) redSprite = LoadSubSprite(dir + "key_red.png");
        if (yellowSprite == null) yellowSprite = LoadSubSprite(dir + "key_yellow.png");
        if (pickupSound == null) pickupSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Kenney/kenney_new-platformer-pack-1.1/Sounds/sfx_gem.ogg");
#endif
    }

#if UNITY_EDITOR
    private Sprite LoadSubSprite(string path)
    {
        var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var a in assets)
        {
            if (a is Sprite s) return s;
        }
        return null;
    }
#endif

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Application.isPlaying) return;

        if (collision.CompareTag("Player") || collision.GetComponent<PlayerContorler>() != null)
        {
            PlayerContorler player = collision.GetComponent<PlayerContorler>();
            if (player == null)
            {
                player = collision.GetComponentInParent<PlayerContorler>();
            }

            if (player != null)
            {
                player.AddKey(keyColor);

                if (pickupSound != null)
                {
                    player.PlayAudioClip(pickupSound);
                }

                Destroy(gameObject);
            }
        }
    }
}

