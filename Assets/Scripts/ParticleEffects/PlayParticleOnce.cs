using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayParticleOnce : MonoBehaviour
{
	ParticleSystem ps;
	
	// Start is called before the first frame update
    void Awake()
    {
		ps = gameObject.GetComponent<ParticleSystem>();

		ps.Play();

		Destroy(gameObject, ps.duration + ps.startLifetime);
    }
}
