using UnityEngine;
using System.Collections;

public class ExampleGUI : MonoBehaviour {

	EffekseerEmitter laserEffect;
	EffekseerEmitter particleEffect;

	// Use this for initialization
	void Start () {
		laserEffect = GameObject.Find("LaserEffect").GetComponent<EffekseerEmitter>();
		particleEffect = GameObject.Find("ParticleEffect").GetComponent<EffekseerEmitter>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI() {
		GUILayout.BeginArea(new Rect(20, 20, 200, 100));
		if (laserEffect.exists) {
			if (GUILayout.Button("Emitter \"LaserEffect\" Stop")) {
				laserEffect.Stop();
			}
		} else {
			if (GUILayout.Button("Emitter \"LaserEffect\" Play")) {
				laserEffect.Play();
			}
		}
		if (particleEffect.exists) {
			if (GUILayout.Button("Emitter \"ParticleEffect\" Stop")) {
				particleEffect.Stop();
			}
		} else {
			if (GUILayout.Button("Emitter \"ParticleEffect\" Play")) {
				particleEffect.Play();
			}
		}
		if (GUILayout.Button("Particle01 (0, 0, 0) Play")) {
			EffekseerSystem.PlayEffect("Particle01", new Vector3(0, 0, 0));
		}
		if (GUILayout.Button("All Stop")) {
			EffekseerSystem.StopAllEffects();
		}
		GUILayout.EndArea();
	}
}
