using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCounter : MonoBehaviour
{
    public TextMeshProUGUI textCurrentAmmo, textSpareAmmo;

    public void SetAmmo(object sender, Weapon.OnAmmoUpdateArgs args)
    {
		if(args.currentAmmo >= 0)
		textCurrentAmmo.text = args.currentAmmo.ToString();

		if (args.currentSpareAmmo >= 0)
			textSpareAmmo.text = args.currentSpareAmmo.ToString();
    }
}
