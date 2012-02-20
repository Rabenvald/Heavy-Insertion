using UnityEngine;
using System.Collections;

public class NetInputController : InputController 
{
    

    void Start()
    {
        turret = GetComponentInChildren<TurretScript>();
        hull = GetComponentInChildren<Hovercraft>();
    }

	
}
