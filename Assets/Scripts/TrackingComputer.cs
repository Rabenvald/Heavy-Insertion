using UnityEngine;
using System.Collections;

public class TrackingComputer : MonoBehaviour
{

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public static Vector3 GetParabolicFiringSolution(Vector3 origin, Vector3 target, float muzzle_v, Vector3 gravity, Vector3 myVelocity, Vector3 targetVelocity)
    {
        float ottt = GetParabolicFiringSolutionTime(origin + myVelocity, target, muzzle_v, gravity);
        float ttt; // Time to target
        gravity *= -1;
        Vector3 VectorToTarget = (origin + myVelocity) - (target + targetVelocity * ottt);
        float a = gravity.sqrMagnitude;
        float b = -4 * (gravity.magnitude * VectorToTarget.magnitude + muzzle_v * muzzle_v);
        float c = 4 * VectorToTarget.sqrMagnitude;

        if (4*a*c > b*b)
            return Vector3.down;

        float time0 = Mathf.Sqrt((-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a));
        float time1 = Mathf.Sqrt((-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a));

        if (time0 < 0)
        {
            if (time1 < 0)
                //# We have no valid times
                return Vector3.down;
            else
                ttt = time1;
        }

        else
        {
            if (time1 < 0)
                ttt = time0;
            else
                ttt = Mathf.Min(time0, time1);
        }

            //# Return the firing vector
        return (2 * VectorToTarget - gravity * ttt * ttt) / (2 * muzzle_v * ttt);
    }

    public static Vector3 GetParabolicFiringSolution(Vector3 origin, Vector3 target, float muzzle_v, Vector3 gravity, Vector3 myVelocity)
    {
        return GetParabolicFiringSolution(origin, target, muzzle_v, gravity, myVelocity, Vector3.zero);
    }

    public static Vector3 GetParabolicFiringSolution(Vector3 origin, Vector3 target, float muzzle_v, Vector3 gravity)
    {
        return GetParabolicFiringSolution(origin, target, muzzle_v, gravity, Vector3.zero, Vector3.zero);
    }

    private static float GetParabolicFiringSolutionTime(Vector3 origin, Vector3 target, float muzzle_v, Vector3 gravity)
    {
        float ttt; // Time to target
        gravity *= -1;
        Vector3 VectorToTarget = origin - target;
        float a = gravity.sqrMagnitude;
        float b = -4 * (gravity.magnitude * VectorToTarget.magnitude + muzzle_v * muzzle_v);
        float c = 4 * VectorToTarget.sqrMagnitude;

        if (4 * a * c > b * b)
            return 0;

        float time0 = Mathf.Sqrt((-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a));
        float time1 = Mathf.Sqrt((-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a));

        if (time0 < 0)
        {
            if (time1 < 0)
                //# We have no valid times
                return 0;
            else
                ttt = time1;
        }

        else
        {
            if (time1 < 0)
                ttt = time0;
            else
                ttt = Mathf.Min(time0, time1);
        }

        return ttt;
    }

    public static Vector3 GetFiringSolution(Vector3 origin, Vector3 target, float muzzle_v, Vector3 gravity, Vector3 myVelocity, Vector3 targetVelocity, float margin)
    {
        Vector3 FiringSoultion = Vector3.zero;
        Vector3 VectortoTarget = target - origin;
        Vector3 direction = GetParabolicFiringSolution(origin, target, muzzle_v, gravity, myVelocity, targetVelocity);
        float minBounds = Mathf.Asin(direction.y / direction.magnitude);
        /*
        float distance = distanceToTarget(direction, origin, target, muzzle_v);
        if (distance * distance < margin * margin)
            return direction;
         
        else if (minBoundDistance > 0)
            maxBound = minBound;
         * 
         * 
         # Use the shortest possible shot as the minimum bound
minBound = -90

 # Otherwise we need to find a maximum bound, we use
 # 45 degrees
 else:
 maxBound = 45

 # Calculate the distance for the maximum bound
 direction = convertToDirection(deltaPosition, angle)
 distance = distanceToTarget(direction, source,
 target, muzzleVelocity)

# See if we’ve made it
 if distance*distance < margin*margin:
 return direction

 # Otherwise make sure it overshoots
 else if distance < 0:

 # Our best shot can’t make it
 return None

 # Now we have a minimum and maximum bound, use a binary
 # search from here on.
 distance = margin
 while distance*distance < margin*margin:

 # Divide the two bounds
 angle = (maxBound - minBound) * 0.5
# Calculate the distance
rection = convertToDirection(deltaPosition, angle)
 distance = distanceToTarget(direction, source,
 target, muzzleVelocity)

 # Change the appropriate bound
 if distance < 0: minBound = angle
 else: maxBound = angle
         * 
         */
        return FiringSoultion;
    }
}
