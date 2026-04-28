using System;
using UnityEngine;

public class LimitCamera : MonoBehaviour
{
    public GameObject Player;

    void LateUpdate()
    {
        transform.position = new Vector3(Player.transform.position.x, 103, Player.transform.position.z);
    }
}
