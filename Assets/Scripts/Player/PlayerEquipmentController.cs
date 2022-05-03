using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerEquipmentController : MonoBehaviour
{
	UnityEngine.Rendering.Universal.Light2D torch;

	void Awake()
	{
		torch = transform.Find("Torch").GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
		torch.enabled = false;

		gameObject.GetComponent<PlayerContols>().OnTorchKeyPressed += ToggleTorch;
	}

	private void ToggleTorch(object sender, EventArgs args)
	{
		torch.enabled = !torch.enabled;
	}
}
