using UnityEngine;

public static class MonsterCleanup
{
    public static int ClearAllMonsters()
    {
        int removedCount = 0;

        MonsterMover[] monsters = Object.FindObjectsOfType<MonsterMover>();
        for (int i = 0; i < monsters.Length; i++)
        {
            if (monsters[i] != null)
            {
                Object.Destroy(monsters[i].gameObject);
                removedCount++;
            }
        }

        SpawnedObstacleMarker[] spawnedObstacles = Object.FindObjectsOfType<SpawnedObstacleMarker>();
        for (int i = 0; i < spawnedObstacles.Length; i++)
        {
            if (spawnedObstacles[i] != null)
            {
                Object.Destroy(spawnedObstacles[i].gameObject);
                removedCount++;
            }
        }

        return removedCount;
    }
}
