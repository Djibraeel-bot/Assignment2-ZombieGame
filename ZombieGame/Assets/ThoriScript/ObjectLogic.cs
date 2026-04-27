using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Netcode;
using TMPro;
using UnityEngine;

public class ObjectLogic : NetworkBehaviour
{
    [System.Serializable]
    public class ThrowableItem
    {
        public string itemName;
        public GameObject throwablePrefab;
        public GameObject uiSlot;
        public int currentAmount;
        public int maxAmount;
    }

    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public TextMeshProUGUI ammoText;

    [Header("Throwables")]
    public ThrowableItem[] throwables;
    public int selectedIndex = 0;

    [Header("Throwing")]
    public float throwCooldown = 0.5f;
    public float throwForce = 15f;
    public float throwUpwardForce = 5f;

    private bool readyToThrow = true;
    
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        UpdateUI();
    }
    
    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Throw.performed += OnThrow;

        inputActions.Player.Select1.performed += ctx => SelectThrowable(0);
        inputActions.Player.Select2.performed += ctx => SelectThrowable(1);
        inputActions.Player.Select3.performed += ctx => SelectThrowable(2);
        inputActions.Player.Select4.performed += ctx => SelectThrowable(3);
    }

    private void OnDisable()
    {
        inputActions.Player.Throw.performed -= OnThrow;

        inputActions.Player.Select1.performed -= ctx => SelectThrowable(0);
        inputActions.Player.Select2.performed -= ctx => SelectThrowable(1);
        inputActions.Player.Select3.performed -= ctx => SelectThrowable(2);
        inputActions.Player.Select4.performed -= ctx => SelectThrowable(3);

        inputActions.Player.Disable();
    }
    
    private void OnThrow(InputAction.CallbackContext context)
    {
        if (!readyToThrow) return;
        Throw();
    }

    void SelectThrowable(int index)
    {
        if (index >= 0 && index < throwables.Length)
        {
            selectedIndex = index;
            UpdateUI();
        }
    }

    void Throw()
    {
        if (!IsOwner) return; // Only the owner throws

        ThrowServerRpc(selectedIndex);
    }

    [ServerRpc]
    private void ThrowServerRpc(int index)
    {
        ThrowableItem current = throwables[index];
        if (current.currentAmount <= 0) return;

        GameObject projectile = Instantiate(
            current.throwablePrefab,
            attackPoint.position,
            cam.rotation
        );

        NetworkObject netObj = projectile.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(OwnerClientId); // Server spawns, owned by thrower

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 force = cam.forward * throwForce + transform.up * throwUpwardForce;
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);

        // Update state on server
        throwables[index].currentAmount--;
        UpdateAmmoClientRpc(index, throwables[index].currentAmount);
    }
    
    [ClientRpc]
    private void UpdateAmmoClientRpc(int index, int newAmount)
    {
        throwables[index].currentAmount = newAmount;
        UpdateUI();
    }

    void ResetThrow()
    {
        readyToThrow = true;
    }

    public void AddThrowable(int index, int amount = 1)
    {
        Debug.Log("AddThrowable called");

        if (throwables == null)
        {
            Debug.LogError("Throwables array is NULL");
            return;
        }

        Debug.Log("Throwables Length: " + throwables.Length);

        if (index < 0 || index >= throwables.Length)
        {
            Debug.LogError("Invalid throwable index: " + index);
            return;
        }

        Debug.Log("Before Add -> " + throwables[index].itemName + ": " + throwables[index].currentAmount);

        throwables[index].currentAmount += amount;
        throwables[index].currentAmount = Mathf.Clamp(
            throwables[index].currentAmount,
            0,
            throwables[index].maxAmount
        );

        Debug.Log("After Add -> " + throwables[index].itemName + ": " + throwables[index].currentAmount);

        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < throwables.Length; i++)
        {
            if (throwables[i].uiSlot != null)
            {
                throwables[i].uiSlot.SetActive(throwables[i].currentAmount > 0);
            }
        }

        if (ammoText != null)
        {
            ThrowableItem current = throwables[selectedIndex];
            ammoText.text = current.itemName + ": " + current.currentAmount;
        }
    }
}
