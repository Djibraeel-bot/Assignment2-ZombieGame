using UnityEngine;
using Unity.Netcode;

public class AttackRelay : NetworkBehaviour
{
    private PlayerAttack playerAttack;

    private void Awake()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        //controls.Disable();
    }

    public void SpawnAttackHitbox()
    {
        playerAttack.SpawnAttackHitbox();
    }
}
