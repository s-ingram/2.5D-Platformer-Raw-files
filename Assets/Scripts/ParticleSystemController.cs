using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
	public ParticleSystem ps;

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKey(KeyCode.Space))
			ps.Play();
    }
}
