using UnityEngine;
using System.Collections;

public class CreateCraterOnImpact : MonoBehaviour {

	public float Radius=15f;
	public float Depth=10f;
	public float Noise=0.5f;
	public GameObject Explosion;

	void OnCollisionEnter(Collision collision) {
		if (Explosion) {
			Instantiate(Explosion, collision.contacts[0].point, Quaternion.identity);
		}

		CraterMaker CM = collision.gameObject.GetComponent<CraterMaker>();
		if (CM) {
			CM.Create(collision.contacts[0].point, Radius, Depth, Noise);
		}

		Destroy(gameObject);
	}
	
}
