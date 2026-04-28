using Unity.Netcode;
using UnityEngine;

public class FireEffect : NetworkBehaviour
{
    public ParticleSystem fireParticles;

    public override void OnNetworkSpawn()
    {
        // Plays on all clients when spawned
        if (fireParticles != null)
            fireParticles.Play();
    }
}
