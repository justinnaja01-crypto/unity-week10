using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Patrol Limits")]
    [Tooltip("Distance the enemy can patrol to the left of where it was placed")]
    [SerializeField] private float leftDistance = 2f;

    [Tooltip("Distance the enemy can patrol to the right of where it was placed")]
    [SerializeField] private float rightDistance = 2f;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _startPosition;
    private bool _hasStarted = false;
    private int _direction = 1; // 1 = moving right, -1 = moving left

    public float LeftDistance
    {
        get => leftDistance;
        set => leftDistance = value;
    }

    public float RightDistance
    {
        get => rightDistance;
        set => rightDistance = value;
    }

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;
        _hasStarted = true;
    }

    private void Start()
    {
        UpdateFacing();
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        float minX = _startPosition.x - Mathf.Abs(leftDistance);
        float maxX = _startPosition.x + Mathf.Abs(rightDistance);

        Vector2 currentPos = _rb.position;
        float nextX = currentPos.x + _direction * speed * Time.fixedDeltaTime;

        if (nextX >= maxX)
        {
            nextX = maxX;
            _direction = -1;
        }
        else if (nextX <= minX)
        {
            nextX = minX;
            _direction = 1;
        }

        _rb.MovePosition(new Vector2(nextX, currentPos.y));
        UpdateFacing();
    }

    private void UpdateFacing()
    {
        if (_spriteRenderer != null)
        {
            // Sprite naturally faces right; flip when moving left.
            _spriteRenderer.flipX = (_direction < 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When it touches the player (OnTriggerEnter2D only), player loses a life
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerContorler>() != null)
        {
            PlayerContorler player = collision.GetComponent<PlayerContorler>();
            if (player == null)
            {
                player = collision.GetComponentInParent<PlayerContorler>();
            }

            if (player != null)
            {
                player.TakeDamage();
            }
        }
    }

    private void OnDrawGizmos()
    {
        // While editing it follows the enemy when moved; in Play mode it stays where enemy started.
        Vector3 origin = (Application.isPlaying && _hasStarted) ? _startPosition : transform.position;
        float leftX = origin.x - Mathf.Abs(leftDistance);
        float rightX = origin.x + Mathf.Abs(rightDistance);
        float y = origin.y;
        float z = origin.z;

        Vector3 leftLimit = new Vector3(leftX, y, z);
        Vector3 rightLimit = new Vector3(rightX, y, z);

        // Draw line between the two limits at the enemy's height
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(leftLimit, rightLimit);

        // Red wire sphere at the left limit
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(leftLimit, 0.2f);

        // Green wire sphere at the right limit
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(rightLimit, 0.2f);
    }
}

