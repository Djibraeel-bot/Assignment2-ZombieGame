using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GateInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float holdTime = 2f;

    [Header("UI")]
    public GameObject interactUI;
    public Image holdFillImage;

    private bool playerInRange = false;
    private float holdTimer = 0f;

    void Start()
    {
        interactUI.SetActive(false);
        holdFillImage.fillAmount = 0f;
    }

    void Update()
    {
        if (!playerInRange) return;

        interactUI.SetActive(true);

        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;
            holdFillImage.fillAmount = holdTimer / holdTime;

            if (holdTimer >= holdTime)
            {
                OpenGate();
            }
        }
        else
        {
            ResetHold();
        }
    }

    void ResetHold()
    {
        holdTimer = 0f;
        holdFillImage.fillAmount = 0f;
    }

    void OpenGate()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            interactUI.SetActive(false);
            ResetHold();
        }
    }
}
