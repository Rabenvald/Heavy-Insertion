using UnityEngine;
using System.Collections;

public class ProjectileScript : ImportantObject
{
    public GameObject ImpactExplosion;
    //public int Health = 300;
    public float Spin = 10;
    // Max age
    public float MaximumAge = 10.0f;

    // Age counter
    private float age;

    // Use this for initialization
    void Start()
    {
        Health = 300;
        age = 0.0f;
        rigidbody.AddRelativeTorque(Vector3.forward * Spin, ForceMode.Force);
    }

    // Update is called once per frame
    void Update()
    {

        // Add to the age
        age += Time.deltaTime;

        // Check the age
        if (age > MaximumAge)
        {
            explode();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Health -= (int)(collision.impactForceSum.magnitude);
        if (Health < 0) explode();
    }

    protected void explode()
    {
        GameObject impExp = (GameObject)Instantiate(ImpactExplosion, transform.position, Quaternion.identity);
        GameObject.Destroy(this.gameObject);
        //Manager.Instance.updatePhysList();
    }
}
