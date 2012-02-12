using UnityEngine;
using System.Collections;

public class TestCannonScript : MonoBehaviour
{

    public GameObject target;
    public GameObject Projectile;
    private GameObject projectile;
    //public GameObject DeathExplosion;
    //public GameObject ImpactExplosion;

    private Vector3 targetFinalPosition;
    private Vector3 targetDragVel;
    public Vector3 projectileVel;
    private Vector3 projectileDragVel;

    private Quaternion angleToTarget;
    //private Quaternion testQ;

    private float targetDrag;
    public float projectileDrag;
    public float maxDegreeRotation = 3000.0f;

    private float timeToIntercept;

    private float xRotConstraint = 750.0f;
    private float yRotConstraint = 750.0f;

    private Vector3 gravity = Physics.gravity;

    public int Health = 300;
    public float ArmorFactor = 0.7f;

    public float BulletSpeed = 400.0f;
    public float FireRate = 0.5f;
    private float lastFired = 0.0f;
    private Vector3 randomness = Vector3.zero;
    public bool UseRigidbodyPosition = false;

    //Time Tracking
    float deltaTime;
    float currentTime;

    public Vector3 MyPosition;

    // Use this for initialization
    void Start()
    {
        targetDrag = target.rigidbody.drag;
        projectileDrag = Projectile.rigidbody.drag;

        targetDragVel = new Vector3(targetDrag, targetDrag, targetDrag);
        projectileDragVel = new Vector3(projectileDrag, projectileDrag, projectileDrag);

        MyPosition = transform.position;
        if (UseRigidbodyPosition) MyPosition = GetComponent<CapsuleCollider>().transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime = Time.deltaTime;
        currentTime = Time.time;

        randomness.x = (Random.Range(0, 20) - 10) / 10;
        randomness.y = (Random.Range(0, 20) - 10) / 10;
        randomness.z = (Random.Range(0, 20) - 10) / 10;

        if (target != null)
        {

            /*targetFinalPosition = rigidbody.velocity - target.rigidbody.transform.position + target.rigidbody.velocity;
            /if (target.rigidbody.useGravity)
            {
                targetFinalPosition += Physics.gravity;
            }/
            if (Mathf.Abs(rigidbody.velocity.x) - targetDrag > 0)
            {
                targetFinalPosition -= new Vector3(targetFinalPosition.x - targetDrag * targetFinalPosition.normalized.x, 0.0f, 0.0f);
            }
            if (Mathf.Abs(rigidbody.velocity.y) - targetDrag > 0)
            {
                targetFinalPosition -= new Vector3(0.0f, targetFinalPosition.y - targetDrag * targetFinalPosition.normalized.y, 0.0f);
            }
            if (Mathf.Abs(rigidbody.velocity.z) - targetDrag > 0)
            {
                targetFinalPosition -= new Vector3(0.0f, 0.0f, targetFinalPosition.z - targetDrag * targetFinalPosition.normalized.z);
            }

            //targetFinalPosition.y += (gravity.y * targetFinalPosition.y) / Mathf.Pow(projectileVel.y, 2.0f);
            //timeToIntercept=projectileVel/Mathf.Abs(targetFinalPosition-transform.position)
            angleToTarget = Quaternion.LookRotation(targetFinalPosition + transform.position + randomness, Vector3.up);
            //angleToTarget = new Quaternion(Mathf.Clamp(angleToTarget.x, -xRotConstraint, xRotConstraint), Mathf.Clamp(angleToTarget.y, -yRotConstraint, yRotConstraint), 0.0f, 0.0f);
            //angleToTarget.SetEulerAngles.x = Mathf.Clamp(angleToTarget.x, -xRotConstraint, xRotConstraint);
            //angleToTarget.SetEulerAngles.y = Mathf.Clamp(angleToTarget.y, -yRotConstraint, yRotConstraint);
            //testQ = Quaternion.LookRotation(target.transform.position);


            projectileVel = transform.forward * -1.0f * BulletSpeed;*/
            //angleToTarget = TrackingComputer.GetParabolicFiringSolution(transform.position, target.transform.position, BulletSpeed, Physics.gravity);
            /*MyPosition = transform.position;
            if (UseRigidbodyPosition) MyPosition = GetComponent<CapsuleCollider>().transform.position;*/

            //MyPosition = the pivot of the cannon barrel

            transform.rotation = Quaternion.Slerp(rigidbody.transform.rotation, Quaternion.LookRotation(TrackingComputer.GetParabolicFiringSolution(MyPosition, target.transform.position, BulletSpeed, Physics.gravity, rigidbody.velocity, target.rigidbody.velocity)), Time.deltaTime * 1000f);

        }
        if (currentTime - lastFired > FireRate)
        {
            Fire();
            lastFired = currentTime;
        }
    }
    private void Fire()
    {
        // Create a ball
        GameObject projectile = (GameObject)Instantiate(Projectile, transform.position, Quaternion.identity);

        // Set its velocity
        projectile.rigidbody.velocity = transform.forward * -1.0f * BulletSpeed + randomness;
    }

    void OnCollisionEnter(Collision collision)
    {
        //GameObject impExp = (GameObject)Instantiate(ImpactExplosion, collision.transform.position, Quaternion.identity);
        Health -= (int)(collision.impactForceSum.magnitude * ArmorFactor);
        if (Health < 0) explode();
    }

    void explode()
    {
        //GameObject deathExp = (GameObject)Instantiate(DeathExplosion, transform.position, Quaternion.identity);
        GameObject.Destroy(this.gameObject);
    }
}