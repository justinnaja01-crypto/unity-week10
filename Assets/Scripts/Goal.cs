using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Goal : MonoBehaviour
{
    private bool hasTriggered = false;

    public bool HasTriggered => hasTriggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // First time only
        if (hasTriggered) return;

        if (collision.CompareTag("Player") || collision.GetComponent<PlayerContorler>() != null)
        {
            PlayerContorler player = collision.GetComponent<PlayerContorler>();
            if (player == null)
            {
                player = collision.GetComponentInParent<PlayerContorler>();
            }

            if (player != null)
            {
                hasTriggered = true;
                player.CompleteLevel();
            }
        }
    }
}

