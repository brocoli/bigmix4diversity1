using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class EnemyPieceSpawner : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject PiecePrefab;

    private Random _random;

    public void Awake()
    {
        _random = new Random();
    }

    public void Update()
    {

    }
}
