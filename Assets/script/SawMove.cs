using UnityEngine;

public class SawMove : MonoBehaviour
{
    public Transform Pos1;
    public Transform Pos2;
    [SerializeField] private float moveSpeed = 2f;

    private Transform moveTarget;

    private void Start()
    {
        moveTarget = Pos1;

        if (Pos1 == null || Pos2 == null)
        {
            Debug.LogWarning($"[SawMove] '{name}' Pos1 or Pos2 is not assigned.");
        }
    }

    private void Update()
    {
        if (Pos1 == null || Pos2 == null || moveTarget == null)
            return;

        if (Vector3.Distance(transform.position, Pos1.position) < 0.1f)
        {
            moveTarget = Pos2;
        }
        else if (Vector3.Distance(transform.position, Pos2.position) < 0.1f)
        {
            moveTarget = Pos1;
        }

        transform.position = Vector3.MoveTowards(transform.position, moveTarget.position, moveSpeed * Time.deltaTime);
    }
}
