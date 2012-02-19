using UnityEngine;
using System.Collections;

public class NetTag : MonoBehaviour
{

    private string id = "";
    public string Id
    {
        get
        {
            return id;
        }
        set
        {
            if (id == "")
            {
                id = value;
            }
        }
    }
}
