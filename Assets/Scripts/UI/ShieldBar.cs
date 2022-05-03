using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldBar : MonoBehaviour
{
	public Slider slider;

    public void SetShield(object sender, PlayerCharacter.OnHPandShieldChangeArgs args)
	{
        if (args.currentShield >= 0) slider.value = args.currentShield;
		if (args.maxShield >= 0) slider.maxValue = args.maxShield;
    }
}
