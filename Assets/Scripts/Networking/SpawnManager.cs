using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    Transform[] spawnpoints;

    void Awake()
    {
        Instance = this;
        spawnpoints = GetComponentsInChildren<Transform>();
    }

    public Transform getSpawnPoint()
    {
        return spawnpoints[Random.Range(0, spawnpoints.Length)];
    }
}
