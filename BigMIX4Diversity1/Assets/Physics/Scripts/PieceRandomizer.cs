using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class PieceRandomizer : MonoBehaviour
{
    public GameObject[] Pieces;
    public Transform SpawnPoint;

    public int Min;
    public int Max;

    public float Limit;
    private float _timer;

    private Random _random;

    public void Awake()
    {
        _random = new Random();
    }

    public void Update()
    {
        _timer = _timer + Time.deltaTime;
        if (!(_timer >= Limit)) return;

        var posX = _random.Next(Min, Max);
        var rotZ = _random.Next(0, 360);
        var obj = _random.Next(1, Pieces.Length);

        SpawnPoint.position = new Vector3(posX, SpawnPoint.position.y, SpawnPoint.position.z);
        SpawnPoint.rotation = new Quaternion(SpawnPoint.rotation.x, SpawnPoint.rotation.y, rotZ, SpawnPoint.rotation.w);
        Instantiate(Pieces[obj], SpawnPoint.position, SpawnPoint.rotation);

        _timer = 0;
    }
}
