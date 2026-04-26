using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class UpdateCamRotation : NetworkBehaviour
{
    public Transform cameraTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        //controls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        //transform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);

        transform.rotation = cameraTransform.rotation;
    }
}
