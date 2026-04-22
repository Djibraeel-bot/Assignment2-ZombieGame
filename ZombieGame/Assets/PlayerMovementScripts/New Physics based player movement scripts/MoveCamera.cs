using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class MoveCamera : NetworkBehaviour
{
    [SerializeField] private Transform cameraPosition;

    private InputSystem_Actions controls;

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
        transform.position = cameraPosition.position;
    }
}
