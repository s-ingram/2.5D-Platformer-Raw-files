using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	public Slider slider;

	public void SetHealth(object sender, PlayerCharacter.OnHPandShieldChangeArgs args)
    {
		if (args.currentHp >= 0) slider.value = args.currentHp;
		if (args.maxHp >= 0) slider.maxValue = args.maxHp;

		/*
		if (slider.value / slider.maxValue > 0.75f)
			transform.Find("Fill").GetComponent<Image>().color = Color.green;
		else if (slider.value / slider.maxValue > 0.25f )
			transform.Find("Fill").GetComponent<Image>().color = Color.yellow;
		else
			transform.Find("Fill").GetComponent<Image>().color = new Color32(215, 44, 46, 255);*/
	}
}
