using System.Collections;
using UnityEngine;
using TMPro;

[ExecuteAlways]
public class Door : MonoBehaviour
{
    [Header("Key Requirement")]
    [Tooltip("Drag a Key from Hierarchy. Its color will be automatically copied to this door.")]
    [SerializeField] private Key requiredKey;
    [SerializeField] private int keysNeeded = 1;
    [SerializeField] private KeyColor doorColor = KeyColor.Blue;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;

    [Header("Door Sprites")]
    [SerializeField] private Sprite doorClosedBottomSprite;
    [SerializeField] private Sprite doorClosedTopSprite;
    [SerializeField] private Sprite doorOpenBottomSprite;
    [SerializeField] private Sprite doorOpenTopSprite;

    [Header("HUD Key Sprites for Indicator")]
    [SerializeField] private Sprite hudKeyBlue;
    [SerializeField] private Sprite hudKeyGreen;
    [SerializeField] private Sprite hudKeyRed;
    [SerializeField] private Sprite hudKeyYellow;

    [Header("Component References")]
    [SerializeField] private SpriteRenderer bottomRenderer;
    [SerializeField] private SpriteRenderer topRenderer;
    [SerializeField] private Collider2D solidCollider;
    [SerializeField] private SpriteRenderer indicatorIcon;
    [SerializeField] private TextMeshPro indicatorText;

    private bool isOpen = false;
    private Coroutine feedbackCoroutine;

    public Key RequiredKey
    {
        get => requiredKey;
        set
        {
            requiredKey = value;
            if (requiredKey != null) doorColor = requiredKey.Color;
            UpdateVisuals();
        }
    }

    public int KeysNeeded
    {
        get => keysNeeded;
        set
        {
            keysNeeded = Mathf.Max(1, value);
            UpdateVisuals();
        }
    }

    public KeyColor Color
    {
        get => doorColor;
        set
        {
            doorColor = value;
            UpdateVisuals();
        }
    }

    public bool IsOpen => isOpen;

    private void Awake()
    {
        EnsureSpritesLoaded();
        UpdateVisuals();
    }

    private void OnValidate()
    {
        if (requiredKey != null)
        {
            doorColor = requiredKey.Color;
        }

        if (keysNeeded < 1) keysNeeded = 1;

        EnsureSpritesLoaded();
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (isOpen)
        {
            if (bottomRenderer != null && doorOpenBottomSprite != null)
                bottomRenderer.sprite = doorOpenBottomSprite;
            if (topRenderer != null && doorOpenTopSprite != null)
                topRenderer.sprite = doorOpenTopSprite;

            if (indicatorIcon != null) indicatorIcon.gameObject.SetActive(false);
            if (indicatorText != null) indicatorText.gameObject.SetActive(false);
        }
        else
        {
            if (bottomRenderer != null && doorClosedBottomSprite != null)
                bottomRenderer.sprite = doorClosedBottomSprite;
            if (topRenderer != null && doorClosedTopSprite != null)
                topRenderer.sprite = doorClosedTopSprite;

            if (indicatorIcon != null)
            {
                indicatorIcon.gameObject.SetActive(true);
                indicatorIcon.sprite = GetHudSpriteForColor(doorColor);
            }
            if (indicatorText != null)
            {
                indicatorText.gameObject.SetActive(true);
                indicatorText.text = $"x{keysNeeded}";
                indicatorText.color = UnityEngine.Color.white;
            }
        }
    }

    private Sprite GetHudSpriteForColor(KeyColor col)
    {
        switch (col)
        {
            case KeyColor.Blue: return hudKeyBlue;
            case KeyColor.Green: return hudKeyGreen;
            case KeyColor.Red: return hudKeyRed;
            case KeyColor.Yellow: return hudKeyYellow;
            default: return hudKeyBlue;
        }
    }

    public void EnsureSpritesLoaded()
    {
#if UNITY_EDITOR
        string dir = "Assets/Art/Kenney/kenney_new-platformer-pack-1.1/Sprites/Tiles/Default/";
        if (doorClosedBottomSprite == null) doorClosedBottomSprite = LoadSubSprite(dir + "door_closed.png");
        if (doorClosedTopSprite == null) doorClosedTopSprite = LoadSubSprite(dir + "door_closed_top.png");
        if (doorOpenBottomSprite == null) doorOpenBottomSprite = LoadSubSprite(dir + "door_open.png");
        if (doorOpenTopSprite == null) doorOpenTopSprite = LoadSubSprite(dir + "door_open_top.png");

        if (hudKeyBlue == null) hudKeyBlue = LoadSubSprite(dir + "hud_key_blue.png");
        if (hudKeyGreen == null) hudKeyGreen = LoadSubSprite(dir + "hud_key_green.png");
        if (hudKeyRed == null) hudKeyRed = LoadSubSprite(dir + "hud_key_red.png");
        if (hudKeyYellow == null) hudKeyYellow = LoadSubSprite(dir + "hud_key_yellow.png");

        if (openSound == null) openSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Kenney/kenney_new-platformer-pack-1.1/Sounds/sfx_disappear.ogg");
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

    private void OnDrawGizmos()
    {
        if (!isOpen)
        {
            Vector3 pos = transform.position + Vector3.up * 1.6f;
            UnityEditor.Handles.Label(pos + Vector3.right * 0.25f, $"{doorColor} x{keysNeeded}");
        }
    }
#endif

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!Application.isPlaying || isOpen) return;
        TryOpen(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!Application.isPlaying || isOpen) return;
        TryOpen(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Application.isPlaying || isOpen) return;
        TryOpen(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!Application.isPlaying || isOpen) return;
        TryOpen(other);
    }

    private void TryOpen(Collider2D col)
    {
        if (col.CompareTag("Player") || col.GetComponent<PlayerContorler>() != null)
        {
            PlayerContorler player = col.GetComponent<PlayerContorler>();
            if (player == null) player = col.GetComponentInParent<PlayerContorler>();

            if (player != null)
            {
                int playerKeys = player.GetKeyCount(doorColor);

                if (playerKeys >= keysNeeded)
                {
                    player.UseKeys(doorColor, keysNeeded);
                    OpenDoor(player);
                }
                else
                {
                    int missing = keysNeeded - playerKeys;
                    ShowMissingFeedback(missing, playerKeys);
                }
            }
        }
    }

    private void OpenDoor(PlayerContorler player)
    {
        isOpen = true;

        if (solidCollider != null)
        {
            solidCollider.enabled = false;
        }

        UpdateVisuals();

        if (openSound != null && player != null)
        {
            player.PlayAudioClip(openSound);
        }

        Debug.Log($"Door opened! Used {keysNeeded} {doorColor} key(s).");
    }

    private void ShowMissingFeedback(int missing, int have)
    {
        Debug.Log($"Door locked! Needs {missing} more {doorColor} key(s) (Have: {have}, Need: {keysNeeded}).");

        if (indicatorText != null && Application.isPlaying)
        {
            if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = StartCoroutine(ShowMissingCoroutine(missing));
        }
    }

    private IEnumerator ShowMissingCoroutine(int missing)
    {
        indicatorText.text = $"Need {missing}!";
        indicatorText.color = UnityEngine.Color.red;

        yield return new WaitForSeconds(2.0f);

        indicatorText.text = $"x{keysNeeded}";
        indicatorText.color = UnityEngine.Color.white;
        feedbackCoroutine = null;
    }
}
