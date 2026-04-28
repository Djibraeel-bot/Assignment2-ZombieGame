using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerPoints : NetworkBehaviour
{
    public static PlayerPoints LocalInstance { get; private set; }

    [Header("Points")]
    private NetworkVariable<int> netPoints = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("UI")]
    public TMP_Text pointsText;

    public override void OnNetworkSpawn()
    {
        netPoints.OnValueChanged += OnPointsChanged;

        if (IsLocalPlayer)
        {
            LocalInstance = this;
            UpdateUI(netPoints.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        netPoints.OnValueChanged -= OnPointsChanged;

        if (IsLocalPlayer && LocalInstance == this)
            LocalInstance = null;
    }

    private void OnPointsChanged(int previous, int current)
    {
        // Only update UI for the local player
        if (IsLocalPlayer)
            UpdateUI(current);
    }

    public void AddPoints(int amount)
    {
        AddPointsServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = true)]
    private void AddPointsServerRpc(int amount)
    {
        netPoints.Value += amount;
    }

    public bool SpendPoints(int amount)
    {
        if (netPoints.Value >= amount)
        {
            SpendPointsServerRpc(amount);
            return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = true)]
    private void SpendPointsServerRpc(int amount)
    {
        if (netPoints.Value >= amount)
            netPoints.Value -= amount;
    }

    private void UpdateUI(int value)
    {
        if (pointsText != null)
            pointsText.text = "Points: " + value.ToString();
    }
}
