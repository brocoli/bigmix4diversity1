using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BackgroundDown : MonoBehaviour
{
    private RectTransform _background;
    public int VelDivider;
    public int ZoomPeriod;

	void Start ()
	{
	    _background = GetComponent<RectTransform>();
	}
	
	void Update () {
        //_background.transform.Translate(Vector3.down / VelDivider);

	    if (Input.GetKeyDown(KeyCode.P))
	    {
	        _background.DOScale(new Vector3(1.5f, 1.5f, 1), ZoomPeriod).SetEase(Ease.InCubic);
	    }
	}
}
