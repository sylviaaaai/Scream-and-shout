using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterMover : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float baseSpeed = 7f;
    [SerializeField] private bool useSerialMonsterSpeed = true;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private SerialInput serialInput;

    [Header("Direction")]
    [SerializeField] private Vector2 moveDirection = Vector2.right;

    [Header("Ground Follow (Optional)")]
    [SerializeField] private bool stickToGround = false;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundRayDistance = 2f;
    [SerializeField] private float groundOffset = 0f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (serialInput == null)
        {
            serialInput = FindObjectOfType<SerialInput>();
        }
    }

    private void FixedUpdate()
    {
        Vector2 dir = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector2.right;
        float speed = ResolveCurrentSpeed();

        if (rb != null)
        {
            // Keep vertical physics (gravity/fall), only drive horizontal movement.
            Vector2 velocity = rb.velocity;
            velocity.x = dir.x * speed;
            rb.velocity = velocity;

            if (stickToGround)
            {
                Vector2 projected = ProjectToGround(rb.position);
                rb.MovePosition(new Vector2(rb.position.x, projected.y));
            }

            return;
        }

        Vector2 delta = dir * (speed * Time.fixedDeltaTime);
        Vector2 target = (Vector2)transform.position + delta;
        if (stickToGround)
        {
            target = ProjectToGround(target);
        }

        transform.position = target;
    }

    public void SetSerialInput(SerialInput input)
    {
        serialInput = input;
    }

    private float ResolveCurrentSpeed()
    {
        if (useSerialMonsterSpeed && serialInput != null)
        {
            return Mathf.Max(0f, serialInput.monsterSpeed * speedMultiplier);
        }

        return Mathf.Max(0f, baseSpeed * speedMultiplier);
    }

    private Vector2 ProjectToGround(Vector2 target)
    {
        if (groundLayer.value == 0)
        {
            return target;
        }

        Vector2 rayOrigin = target + Vector2.up * groundRayDistance;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundRayDistance * 2f, groundLayer);
        if (hit.collider == null)
        {
            return target;
        }

        target.y = hit.point.y + groundOffset;
        return target;
    }
}
