using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
	/*
	[SerializeField]
    public GameObject crosshairPrefab;
    private GameObject crosshair;

    private Vector3 target;

    void Start()
    {
        Cursor.visible = false;
        crosshair = Instantiate(crosshairPrefab);
    }

    void Update()
    {
        target = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

        crosshair.transform.position = new Vector2(target.x, target.y);
    }
	*/

	Image crosshair;

	void Start()
	{
		Cursor.visible = false;
	}

	void Update()
	{
		transform.position = Input.mousePosition;
	}
}
