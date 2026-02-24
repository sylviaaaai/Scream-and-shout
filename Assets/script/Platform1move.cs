using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform1move : MonoBehaviour
{
    public Transform Pos1, Pos2;
    private Transform MovePos;
    [SerializeField] private float MoveSpeed;
    [Header("Movement Control")]
    public bool moveOnlyWhenPlayerOn = true;
    private bool shouldMove = false;
    [Tooltip("If true the platform stops when the player steps off. If false it keeps moving after triggered.")]
    public bool stopOnExit = false;
    [Tooltip("List of tags that will trigger the platform. Add Player, Player1, Player2 etc.")]
    public string[] triggerTags = new string[] { "Player" };
    [Tooltip("If true, platform goes back to Pos1 when reset (e.g. on respawn).")]
    [SerializeField] private bool resetToPos1OnRespawn = true;

    private bool IsTriggerTag(Collider2D col)
    {
        if (triggerTags == null || triggerTags.Length == 0)
            return false;

        // search the collider's transform and its parents for a matching tag
        Transform t = col.transform;
        while (t != null)
        {
            for (int i = 0; i < triggerTags.Length; i++)
            {
                if (t.CompareTag(triggerTags[i]))
                {
                    Debug.Log($"[Platform1move] '{name}' matched tag '{triggerTags[i]}' on '{t.name}' (searching from collider '{col.name}')");
                    return true;
                }
            }
            t = t.parent;
        }
        return false;
    }

    public void ResetPlatformState()
    {
        shouldMove = false;
        MovePos = Pos1;

        if (resetToPos1OnRespawn && Pos1 != null)
        {
            transform.position = Pos1.position;
        }

        Debug.Log($"[Platform1move] '{name}' reset: shouldMove=false, position={(resetToPos1OnRespawn ? "Pos1" : "keep current")}");
    }

    // Start is called before the first frame update
    void Start()
    {
        MovePos = Pos1;
        // Diagnostic logs to help debug trigger/movement issues
        string tags = triggerTags != null ? string.Join(",", triggerTags) : "<none>";
        Debug.Log($"[Platform1move] '{name}' Start: MoveSpeed={MoveSpeed} moveOnlyWhenPlayerOn={moveOnlyWhenPlayerOn} stopOnExit={stopOnExit} triggerTags={tags}");
        if (Pos1 == null || Pos2 == null)
            Debug.LogWarning($"[Platform1move] '{name}' Pos1 or Pos2 not assigned (Pos1={Pos1}, Pos2={Pos2})");

        var col = GetComponent<Collider2D>();
        if (col == null)
            Debug.LogWarning($"[Platform1move] '{name}' has no Collider2D on the same GameObject.");
        else
            Debug.Log($"[Platform1move] '{name}' Collider2D.isTrigger={col.isTrigger}");

        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            // not required, but recommended for kinematic platform
            Debug.Log($"[Platform1move] '{name}' has no Rigidbody2D (recommended: add Kinematic Rigidbody2D)");
        }
        else
        {
            Debug.Log($"[Platform1move] '{name}' Rigidbody2D.bodyType={rb.bodyType}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (moveOnlyWhenPlayerOn && !shouldMove)
            return;

        if(Vector3.Distance(transform.position, Pos1.position) < 0.1f)
        {
            MovePos = Pos2;
        }
        if(Vector3.Distance(transform.position, Pos2.position) < 0.1f)
        {
            MovePos = Pos1;
        }
        transform.position = Vector3.MoveTowards(transform.position, MovePos.position, MoveSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // only parent objects that match trigger tags so unrelated objects aren't reparented
        if (IsTriggerTag(collision))
        {
            collision.transform.parent = this.transform;
            shouldMove = true;
            Debug.Log($"Platform '{name}' triggered by '{collision.name}' tag '{collision.tag}' -> shouldMove=true");
        }
        else
        {
            Debug.Log($"Platform '{name}' OnTriggerEnter by '{collision.name}' tag '{collision.tag}' ignored");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // only restore parent for objects we previously parented
        if (IsTriggerTag(collision))
        {
            if (collision.transform.parent == this.transform)
                collision.transform.parent = null;

            // if configured to stop when an allowed trigger exits, turn movement off
            if (stopOnExit)
            {
                shouldMove = false;
                Debug.Log($"Platform '{name}' trigger '{collision.name}' exited -> shouldMove=false");
            }
        }
    }

    // Support non-trigger colliders: treat collisions as stepping on/off
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var col = collision.collider;
        if (IsTriggerTag(col))
        {
            if (col.transform.parent != this.transform)
                col.transform.parent = this.transform;
            shouldMove = true;
            Debug.Log($"Platform '{name}' collided by '{col.name}' tag '{col.tag}' -> shouldMove=true");
        }
        else
        {
            Debug.Log($"Platform '{name}' OnCollisionEnter by '{col.name}' tag '{col.tag}' ignored");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        var col = collision.collider;
        if (IsTriggerTag(col))
        {
            if (col.transform.parent == this.transform)
                col.transform.parent = null;

            if (stopOnExit)
            {
                shouldMove = false;
                Debug.Log($"Platform '{name}' collision '{col.name}' exited -> shouldMove=false");
            }
        }
    }
}
