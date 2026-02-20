using UnityEngine;

public class PlayerPlatformAttachment : MonoBehaviour
{
    [Tooltip("Downward ray distance used to detect attachable platform")]
    public float attachRayDistance = 1.1f;

    public void DetachFromPlatform()
    {
        if (transform.parent != null)
        {
            var pm = transform.parent.GetComponentInParent<Platform1move>();
            if (pm != null)
            {
                transform.parent = null;
            }
        }
    }

    public void TryAttachToPlatform()
    {
        // Raycast down to find a moving platform underneath and parent to it.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, attachRayDistance);
        if (hit.collider != null)
        {
            var pm = hit.collider.GetComponentInParent<Platform1move>();
            if (pm != null)
            {
                transform.parent = pm.transform;
            }
        }
    }
}
