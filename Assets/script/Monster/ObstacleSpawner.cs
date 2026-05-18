using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SerialInput serialInput;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform obstacleSpawnPointInFrontPlayer;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform spawnedParent;

    [Header("Spawn")]
    [SerializeField] private bool enableKeyboardSpawn = true;
    [SerializeField] private KeyCode keyboardSpawnKey = KeyCode.K;
    [SerializeField] private bool spawnOnObstacleReadyFallingEdge = true;
    [SerializeField] private float spawnYOffset = 0f;
    [SerializeField] private bool requireArmedReadyState = true;
    [SerializeField] private float minSpawnIntervalSeconds = 0.2f;
    [SerializeField] private bool allowOnlyOneSpawnPerNotReadyWindow = true;
    [SerializeField] private float readyStateDebounceSeconds = 0.15f;

    private bool hasReadyState;
    private bool previousObstacleReady = true;
    private bool isArmed;
    private float lastSpawnTime = -999f;
    private bool hasSpawnedInCurrentNotReadyWindow;
    private bool rawObstacleReady = true;
    private bool stableObstacleReady = true;
    private float lastRawReadyChangeTime;
    private GameObject currentObstacle;

    private void Start()
    {
        if (serialInput == null)
        {
            serialInput = FindObjectOfType<SerialInput>();
        }
    }

    private void Update()
    {
        if (obstaclePrefab == null)
        {
            return;
        }

        if (enableKeyboardSpawn && Input.GetKeyDown(keyboardSpawnKey))
        {
            TrySpawnObstacle();
        }

        if (serialInput == null)
        {
            return;
        }

        bool currentReady = serialInput.obstacleReady;
        if (!hasReadyState)
        {
            hasReadyState = true;
            rawObstacleReady = currentReady;
            stableObstacleReady = currentReady;
            previousObstacleReady = stableObstacleReady;
            lastRawReadyChangeTime = Time.time;
            isArmed = !requireArmedReadyState || stableObstacleReady;
            return;
        }

        if (currentReady != rawObstacleReady)
        {
            rawObstacleReady = currentReady;
            lastRawReadyChangeTime = Time.time;
        }

        if (stableObstacleReady != rawObstacleReady &&
            Time.time - lastRawReadyChangeTime >= readyStateDebounceSeconds)
        {
            stableObstacleReady = rawObstacleReady;
        }

        if (!isArmed)
        {
            if (stableObstacleReady)
            {
                isArmed = true;
            }

            previousObstacleReady = stableObstacleReady;
            return;
        }

        if (stableObstacleReady)
        {
            hasSpawnedInCurrentNotReadyWindow = false;
        }

        bool fallingEdge = previousObstacleReady && !stableObstacleReady;
        previousObstacleReady = stableObstacleReady;

        if (spawnOnObstacleReadyFallingEdge && fallingEdge)
        {
            if (allowOnlyOneSpawnPerNotReadyWindow && hasSpawnedInCurrentNotReadyWindow)
            {
                return;
            }

            if (!TrySpawnObstacle())
            {
                return;
            }

            hasSpawnedInCurrentNotReadyWindow = true;
        }
    }

    public void SpawnObstacle()
    {
        if (obstaclePrefab == null)
        {
            return;
        }

        if (currentObstacle != null)
        {
            currentObstacle.SetActive(false);
            Destroy(currentObstacle);
            currentObstacle = null;
        }

        Vector3 spawnPosition = ResolveSpawnPosition();
        GameObject spawned = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, spawnedParent);
        currentObstacle = spawned;

        if (spawned.GetComponent<SpawnedObstacleMarker>() == null)
        {
            spawned.AddComponent<SpawnedObstacleMarker>();
        }
    }

    private Vector3 ResolveSpawnPosition()
    {
        if (obstacleSpawnPointInFrontPlayer != null)
        {
            Vector3 p = obstacleSpawnPointInFrontPlayer.position;
            p.y += spawnYOffset;
            return p;
        }

        if (playerTransform != null)
        {
            Vector3 p = playerTransform.position + new Vector3(2f, 0f, 0f);
            p.y += spawnYOffset;
            return p;
        }

        return transform.position + new Vector3(0f, spawnYOffset, 0f);
    }

    private bool TrySpawnObstacle()
    {
        if (Time.time - lastSpawnTime < minSpawnIntervalSeconds)
        {
            return false;
        }

        SpawnObstacle();
        lastSpawnTime = Time.time;
        return true;
    }
}
