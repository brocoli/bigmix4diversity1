using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUp : MonoBehaviour
{
    private Camera _main;

    void Awake()
    {
        _main = Camera.main;
    }

	void Update ()
	{
	    _main.transform.Translate(Vector3.up);
	}
}
