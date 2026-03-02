using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SerialInput serialInput;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform monsterSpawnPointBehindPlayer;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform spawnedParent;

    [Header("Spawn")]
    [SerializeField] private bool spawnOnMonsterReadyFallingEdge = true;
    [SerializeField] private float spawnYOffset = 0f;
    [SerializeField] private bool requireArmedReadyState = true;
    [SerializeField] private float minSpawnIntervalSeconds = 0.2f;
    [SerializeField] private bool allowOnlyOneSpawnPerNotReadyWindow = true;
    [SerializeField] private float readyStateDebounceSeconds = 0.15f;

    private bool hasReadyState;
    private bool previousMonsterReady = true;
    private bool isArmed;
    private float lastSpawnTime = -999f;
    private bool hasSpawnedInCurrentNotReadyWindow;
    private bool rawMonsterReady = true;
    private bool stableMonsterReady = true;
    private float lastRawReadyChangeTime;
    private GameObject currentMonster;

    private void Start()
    {
        if (serialInput == null)
        {
            serialInput = FindObjectOfType<SerialInput>();
        }
    }

    private void Update()
    {
        if (serialInput == null || monsterPrefab == null)
        {
            return;
        }

        bool currentReady = serialInput.monsterReady;
        if (!hasReadyState)
        {
            hasReadyState = true;
            rawMonsterReady = currentReady;
            stableMonsterReady = currentReady;
            previousMonsterReady = stableMonsterReady;
            lastRawReadyChangeTime = Time.time;
            isArmed = !requireArmedReadyState || stableMonsterReady;
            return;
        }

        if (currentReady != rawMonsterReady)
        {
            rawMonsterReady = currentReady;
            lastRawReadyChangeTime = Time.time;
        }

        if (stableMonsterReady != rawMonsterReady &&
            Time.time - lastRawReadyChangeTime >= readyStateDebounceSeconds)
        {
            stableMonsterReady = rawMonsterReady;
        }

        if (!isArmed)
        {
            // Wait until ready goes true at least once before allowing spawn edges.
            if (stableMonsterReady)
            {
                isArmed = true;
            }

            previousMonsterReady = stableMonsterReady;
            return;
        }

        if (stableMonsterReady)
        {
            // Unlock next spawn window when Arduino cooldown returns to ready state.
            hasSpawnedInCurrentNotReadyWindow = false;
        }

        bool fallingEdge = previousMonsterReady && !stableMonsterReady;
        previousMonsterReady = stableMonsterReady;

        if (spawnOnMonsterReadyFallingEdge && fallingEdge)
        {
            if (allowOnlyOneSpawnPerNotReadyWindow && hasSpawnedInCurrentNotReadyWindow)
            {
                return;
            }

            if (Time.time - lastSpawnTime < minSpawnIntervalSeconds)
            {
                return;
            }

            SpawnMonster();
            lastSpawnTime = Time.time;
            hasSpawnedInCurrentNotReadyWindow = true;
        }
    }

    public void SpawnMonster()
    {
        if (monsterPrefab == null)
        {
            return;
        }

        if (currentMonster != null)
        {
            currentMonster.SetActive(false);
            Destroy(currentMonster);
            currentMonster = null;
        }

        Vector3 spawnPosition = ResolveSpawnPosition();
        GameObject spawned = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity, spawnedParent);
        currentMonster = spawned;

        MonsterMover mover = spawned.GetComponent<MonsterMover>();
        if (mover != null)
        {
            mover.SetSerialInput(serialInput);
        }
    }

    private Vector3 ResolveSpawnPosition()
    {
        if (monsterSpawnPointBehindPlayer != null)
        {
            Vector3 p = monsterSpawnPointBehindPlayer.position;
            p.y += spawnYOffset;
            return p;
        }

        if (playerTransform != null)
        {
            Vector3 p = playerTransform.position + new Vector3(-2f, 0f, 0f);
            p.y += spawnYOffset;
            return p;
        }

        return transform.position + new Vector3(0f, spawnYOffset, 0f);
    }
}
