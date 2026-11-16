using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PlayerContorler : MonoBehaviour
{
    [SerializeField] string playerName = "Alien";
    [Range(1, 100)]
    [SerializeField] float speed = 5.5f;
    [SerializeField] float jump = 5.5f;

    int totalcoin = 5;
    int coin;
    bool isGrounded = true;
    string bar;

    Rigidbody2D rd;
    SpriteRenderer sprites;
    Animator anim;
    Vector2 move;

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");

    [SerializeField] TMP_Text coinText;
    [SerializeField] TMP_Text debugText;

    // week13
    public float fallLimit = -6.0f; // กำหนดความสูงที่ตกได้ในตำแหน่ง
    Vector3 startPosition; // ตำแหน่ง x,y,z เพื่อ respawn หลังตกแมพ
    public AudioClip jumpSound; // คลิปเสียงกระโดด
    public AudioClip coinSound; // คลิปเสียงเก็บเหรียญ
    public AudioClip hurtSound; // คลิปเสียงเจ็บ
    AudioSource audioSource; // ลำโพงเพื่อเล่นคลิปเสียง

    // Feature 3: Lives & UI
    [Header("Lives & UI")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite heartFullSprite;
    [SerializeField] private Sprite heartEmptySprite;

    [Header("Keys & HUD")]
    [SerializeField] private TMP_Text blueKeyText;
    [SerializeField] private TMP_Text greenKeyText;
    [SerializeField] private TMP_Text redKeyText;
    [SerializeField] private TMP_Text yellowKeyText;

    private int blueKeys = 0;
    private int greenKeys = 0;
    private int redKeys = 0;
    private int yellowKeys = 0;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;

    [Header("Level Complete UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private AudioClip levelCompleteSound;

    private bool isInvulnerable = false;
    private bool isGameOver = false;
    private bool isLevelComplete = false;
    private bool isControlStopped = false;
    private Coroutine blinkCoroutine;

    public int CurrentLives => currentLives;
    public bool IsGameOver => isGameOver;
    public bool IsLevelComplete => isLevelComplete;

    void Awake()
    {
        // Ensure time scale is restored on scene load/start
        Time.timeScale = 1f;
    }

    void Start()
    {
        rd = GetComponent<Rigidbody2D>();
        sprites = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (coinText != null)
        {
            coinText.text = $"Coin: {coin} / {totalcoin}";
        }

        startPosition = transform.position; // ตอนเกมเริ่ม จุด respawn = ตำแหน่งปัจจุบันของผู้เล่น

        currentLives = maxLives;
        UpdateHeartsHUD();
        UpdateKeysHUD();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RestartGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    void Update()
    {
        // Feature 4: Pressing R reloads the scene at any time
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
            return;
        }

        // Feature 3 & Feature 4: Stop player's control when game over or level complete
        if (isControlStopped)
        {
            return;
        }

        // Falling off still costs a life and respawns
        if (transform.position.y < fallLimit)
        {
            FallRespawn();
        }

        bar = "";
        for (int i = 0; i < totalcoin; i++)
        {
            if (i < coin)
            {
                bar += "#";
            }
            else
            {
                bar += "-";
            }
        }

        if (debugText != null)
        {
            debugText.text = $"Position: {transform.position.x.ToString("F2")}, {transform.position.y.ToString("F2")}" +
                $"\n Coins: {bar}";
        }

        // เดิน ซ-ข
        move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
        // กระโดด
        if (InputSystem.actions.FindAction("jump").WasCompletedThisFrame() && isGrounded)
        {
            rd.linearVelocity = new Vector2(rd.linearVelocity.x, jump);
            PlaySound(jumpSound);
            isGrounded = false;
        }

        if (move.x > 0.1f)
        {
            sprites.flipX = false;
        }
        else if (move.x < -0.1f)
        {
            sprites.flipX = true;
        }

        if (anim != null)
        {
            anim.SetBool(IsWalkingHash, Mathf.Abs(move.x) > 0.1f);
            anim.SetBool(IsGroundedHash, isGrounded);
        }
    }

    private void FixedUpdate()
    {
        if (isControlStopped)
        {
            rd.linearVelocity = Vector2.zero;
            return;
        }

        rd.linearVelocity = new Vector2(move.x * speed, rd.linearVelocity.y);
    }

    public void TakeDamage()
    {
        if (isInvulnerable || isGameOver) return;

        currentLives--;
        if (currentLives < 0) currentLives = 0;
        UpdateHeartsHUD();

        PlaySound(hurtSound);

        if (anim != null)
        {
            anim.SetTrigger(HurtHash);
        }

        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            StartInvulnerability();
        }
    }

    private void FallRespawn()
    {
        currentLives--;
        if (currentLives < 0) currentLives = 0;
        UpdateHeartsHUD();

        PlaySound(hurtSound);

        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            transform.position = startPosition;
            rd.linearVelocity = Vector2.zero;
            StartInvulnerability();
            Debug.Log($"Fell off the world! Lost a life. Remaining lives: {currentLives}");
        }
    }

    public void Respawn()
    {
        FallRespawn();
    }

    private void StartInvulnerability()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(InvulnerabilityCoroutine());
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        float duration = 1.0f;
        float blinkInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (sprites != null)
            {
                sprites.enabled = !sprites.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (sprites != null)
        {
            sprites.enabled = true;
        }
        isInvulnerable = false;
        blinkCoroutine = null;
    }

    public void UpdateHeartsHUD()
    {
        if (heartImages == null) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].sprite = (i < currentLives) ? heartFullSprite : heartEmptySprite;
            }
        }
    }

    public void AddKey(KeyColor color)
    {
        switch (color)
        {
            case KeyColor.Blue: blueKeys++; break;
            case KeyColor.Green: greenKeys++; break;
            case KeyColor.Red: redKeys++; break;
            case KeyColor.Yellow: yellowKeys++; break;
        }
        UpdateKeysHUD();
        Debug.Log($"Collected {color} key! Total: {GetKeyCount(color)}");
    }

    public int GetKeyCount(KeyColor color)
    {
        switch (color)
        {
            case KeyColor.Blue: return blueKeys;
            case KeyColor.Green: return greenKeys;
            case KeyColor.Red: return redKeys;
            case KeyColor.Yellow: return yellowKeys;
            default: return 0;
        }
    }

    public bool HasKeys(KeyColor color, int amount)
    {
        return GetKeyCount(color) >= amount;
    }

    public bool UseKeys(KeyColor color, int amount)
    {
        if (!HasKeys(color, amount)) return false;

        switch (color)
        {
            case KeyColor.Blue: blueKeys -= amount; break;
            case KeyColor.Green: greenKeys -= amount; break;
            case KeyColor.Red: redKeys -= amount; break;
            case KeyColor.Yellow: yellowKeys -= amount; break;
        }
        UpdateKeysHUD();
        return true;
    }

    public void UpdateKeysHUD()
    {
        if (blueKeyText != null) blueKeyText.text = $"x{blueKeys}";
        if (greenKeyText != null) greenKeyText.text = $"x{greenKeys}";
        if (redKeyText != null) redKeyText.text = $"x{redKeys}";
        if (yellowKeyText != null) yellowKeyText.text = $"x{yellowKeys}";
    }

    public void PlayAudioClip(AudioClip clip)
    {
        PlaySound(clip);
    }

    private void ShowEndPanel(GameObject panel, Button buttonToSelect, string logMessage)
    {
        isControlStopped = true;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (sprites != null)
        {
            sprites.enabled = true;
        }

        if (rd != null)
        {
            rd.linearVelocity = Vector2.zero;
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        // Stop time / pause
        Time.timeScale = 0f;

        // Select the button for keyboard/gamepad navigation
        if (buttonToSelect != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buttonToSelect.gameObject);
        }

        Debug.Log(logMessage);
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        ShowEndPanel(gameOverPanel, retryButton, "Game Over! All lives lost.");
    }

    public void CompleteLevel()
    {
        if (isLevelComplete || isGameOver) return;
        isLevelComplete = true;

        PlaySound(levelCompleteSound);
        ShowEndPanel(levelCompletePanel, restartButton, "Level Complete! Reached the goal.");
    }

    public void RestartGame()
    {
        // Feature 5: If paused with Time.timeScale = 0, set it back to 1 before reloading
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contactCount > 0 && collision.GetContact(0).normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            coin += 1;
            Debug.Log($"Coin collected! Total: {coin}");
            PlaySound(coinSound);
            UpdateCoinText();
            Destroy(other.gameObject);
        }
    }

    void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"Coin: {coin} / {totalcoin}";
            if (coin == totalcoin)
            {
                coinText.text = "All coins collected!";
            }
        }
    }

    void PlaySound(AudioClip audio)
    {
        if (audioSource != null && audio != null)
        {
            audioSource.PlayOneShot(audio);
        }
    }
}
