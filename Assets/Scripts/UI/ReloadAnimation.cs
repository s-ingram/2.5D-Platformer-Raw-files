using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadAnimation : MonoBehaviour
{
	Image circle;
	bool active = false;
	float seconds;

	void Awake()
	{
		circle = gameObject.GetComponentInChildren<Image>();
		circle.fillAmount = 0f;
	}

	void Update()
	{
		transform.position = Input.mousePosition;

		if (circle.fillAmount == 1f)
		{
			circle.fillAmount = 0f;
			active = false;
		}

		if(active)
			circle.fillAmount += 1f / seconds * Time.deltaTime;
	}

	public void PlayAnimationForSeconds(float s)
	{
		seconds = s;
		active = true;
	}
}
