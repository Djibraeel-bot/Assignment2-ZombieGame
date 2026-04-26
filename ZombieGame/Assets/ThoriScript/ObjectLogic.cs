using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

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

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        HandleSelection();

        if (Input.GetKeyDown(KeyCode.L) && readyToThrow)
        {
            Throw();
        }
    }

    void HandleSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectThrowable(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectThrowable(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectThrowable(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectThrowable(3);
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
        ThrowableItem current = throwables[selectedIndex];

        if (current.currentAmount <= 0) return;

        readyToThrow = false;

        GameObject projectile = Instantiate(current.throwablePrefab, attackPoint.position, cam.rotation);

        ThrowableState state = projectile.GetComponent<ThrowableState>();
        if (state != null)
        {
            state.SetAsThrown();
        }
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        
        Vector3 forceDirection = cam.forward;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;

        rb.AddForce(forceToAdd, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);

        current.currentAmount--;
        throwables[selectedIndex] = current;

        UpdateUI();
        Invoke(nameof(ResetThrow), throwCooldown);
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