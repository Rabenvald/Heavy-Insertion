using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Exceptions;

public class NetTransform : MonoBehaviour 
{
    //string FullId = typeID.ToString() + ownerID.ToString() + iD.ToString(); 

    public int ownerID;
    public int typeID;
    public int iD;

    //=====================

    public float TimeBetweenUpdates = 0.1f;

    private float LastUpdateTime = 0;
    private float massUpdateTimer = 0;
    private const float massUpdateInterval = 1.5f;
    private const uint NumVarTypes = 2;
    private uint sendVarType = 1;
    private bool newPlayer = false;
    /*7 public int OwnerId
     {
         get
         {
             return ownerId;
         }
         set
         {
             if (ownerId < 0)
                 ownerId = value;
         }
     }*/
    private int ownerId = -1;
    private Vector3 lastPosition;
    private Vector3 lastVelocity;
    private Quaternion lastRotation;

    private SmartFox smartFox;


    // Use this for initialization
    void Start()
    {
        if (SmartFoxConnection.IsInitialized)
        {
            smartFox = SmartFoxConnection.Connection;
        }
        else
        {
            smartFox = new SmartFox(Application.isEditor);
        }

        ownerId = smartFox.MySelf.Id;
        smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);

        UpdateServer();

    }

    public void UpdateServer()
    {
        List<UserVariable> userVars = new List<UserVariable>();

        userVars.Add(new SFSUserVariable("px", (int)transform.position.x));
        userVars.Add(new SFSUserVariable("py", (int)transform.position.y));
        userVars.Add(new SFSUserVariable("pz", (int)transform.position.z));

        userVars.Add(new SFSUserVariable("rx", (int)transform.rotation.eulerAngles.x));
        userVars.Add(new SFSUserVariable("ry", (int)transform.rotation.eulerAngles.y));
        userVars.Add(new SFSUserVariable("rz", (int)transform.rotation.eulerAngles.z));

        userVars.Add(new SFSUserVariable("vx", (int)rigidbody.velocity.x));
        userVars.Add(new SFSUserVariable("vy", (int)rigidbody.velocity.y));
        userVars.Add(new SFSUserVariable("vz", (int)rigidbody.velocity.z));

        smartFox.Send(new SetUserVariablesRequest(userVars));
    }

    public void OnUserEnterRoom(BaseEvent evt)
    {
        newPlayer = true;
        UpdateServer();
    }

    void FixedUpdate()
    {
        float dt = Time.deltaTime;

        if (massUpdateTimer >= massUpdateInterval)
        {
            sendPosition();
            sendRotation();
            massUpdateTimer = 0;
            newPlayer = false;
        }

        if (LastUpdateTime >= TimeBetweenUpdates)
        {
            switch (sendVarType)
            {
                case 1:
                    if (lastPosition == transform.position)
                        break;
                    sendPosition();
                    break;
                case 2:
                    if (lastRotation == transform.rotation)
                        break;
                    sendRotation();
                    break;
                default:
                    sendPosition();
                    sendRotation();
                    Debug.Log("Invalid case choice");
                    break;
            }

            sendVarType++;

            if (sendVarType > NumVarTypes)
                sendVarType = 1;

            LastUpdateTime = 0;
        }
        LastUpdateTime += dt;

        if (newPlayer)
        {
            massUpdateTimer += dt;
        }
    }

    void sendRotation()
    {
        SFSObject myData = new SFSObject();

        myData.PutInt("PID", ownerId);
        myData.PutFloat("rx", transform.rotation.eulerAngles.x);
        myData.PutFloat("ry", transform.rotation.eulerAngles.y);
        myData.PutFloat("rz", transform.rotation.eulerAngles.z);

        smartFox.Send(new ObjectMessageRequest(myData));
        lastRotation = transform.rotation;
    }

    void sendPosition()
    {
        SFSObject myData = new SFSObject();

        myData.PutInt("PID", ownerId);
        myData.PutFloat("px", transform.position.x);
        myData.PutFloat("py", transform.position.y);
        myData.PutFloat("pz", transform.position.z);

        smartFox.Send(new ObjectMessageRequest(myData));
        lastPosition = transform.position;
    }

    void sendVelocity()
    {
        SFSObject myData = new SFSObject();

        myData.PutInt("PID", ownerId);
        myData.PutFloat("vx", rigidbody.velocity.x);
        myData.PutFloat("vy", rigidbody.velocity.y);
        myData.PutFloat("vz", rigidbody.velocity.z);

        smartFox.Send(new ObjectMessageRequest(myData));
        lastVelocity = rigidbody.velocity;
    }
}
