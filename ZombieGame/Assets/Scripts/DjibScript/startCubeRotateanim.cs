using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class startCubeRotateanim : NetworkBehaviour
{
   private NetworkAnimator netAnim;
        public override void OnNetworkSpawn()
        {
            netAnim = GetComponent<NetworkAnimator>();
            if (IsServer)
            {
                netAnim.SetTrigger("StartRotate");
            }
        }
   
}
