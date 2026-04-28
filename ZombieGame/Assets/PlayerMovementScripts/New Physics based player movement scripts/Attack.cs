using UnityEngine;
using Unity.Netcode;

public class PlayerAttack : NetworkBehaviour
{
    public GameObject hitboxPrefab;
    public Transform attackPoint;
    public float attackCooldown = 0.5f;

    private float lastAttackTime;

    private NewPlayerMovement playerMoveScript;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        playerMoveScript = GetComponent<NewPlayerMovement>();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        //controls.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (playerMoveScript.AttackTriggered &&
            Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            // ONLY play animation now
            playerMoveScript.locAnimator.SetTrigger("Attack");
            playerMoveScript.netAnimator.SetTrigger("Attack");
        }
    }

    [ServerRpc]
    void AttackServerRpc(Vector3 pos, Quaternion rot)
    {
        Debug.Log("ServerRpc called on: " + OwnerClientId);

        GameObject hitbox = Instantiate(hitboxPrefab, pos, rot);
        hitbox.GetComponent<NetworkObject>().Spawn();
    }

    public void SpawnAttackHitbox()
    {
        if (!IsOwner) return;

        AttackServerRpc(attackPoint.position, attackPoint.rotation);
    }
}