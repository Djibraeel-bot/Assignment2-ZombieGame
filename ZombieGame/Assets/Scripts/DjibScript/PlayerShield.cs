using UnityEngine;
using System.Collections;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerShield : MonoBehaviour
{
  

    [Header("Shield Settings")]
    [Header("Shield Objects")]
    public GameObject shieldObject;    // Assign full Shield GameObject (child)
    public GameObject parryObject;     // Assign full Parry GameObject (child)
    public float shieldDuration = 3f;
    public float shieldCooldown = 6f;  // Changed to 6s countdown
    public float parryWindow = 0.3f;
    public float pushbackForce = 10f;


    [Header("Collision Event")]
    public UnityEvent<Collision> onShieldHit = new UnityEvent<Collision>();  // Assign ShieldCollider's OnCollisionEnter


    private bool isShieldActive = false;
    private bool shieldReady = true;  // New: Master ready flag
    private bool isOnCooldown = false;
    public bool inParryWindow = false;
    private float cooldownEndTime = 0f;
    private int lastCountdown = -1;  // Prevent spam


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && shieldReady)  // Only shieldReady allows!
        {
            ActivateShield();
        }


        // Countdown logic
        if (isOnCooldown)
        {
            float remaining = cooldownEndTime - Time.time;
            int countdown = Mathf.Max(0, Mathf.CeilToInt(remaining));


            if (countdown != lastCountdown)
            {
                if (countdown > 0)
                {
                    shieldReady = false;  // Explicitly not ready
                    Debug.Log($"Shield cooldown: {countdown}s");
                }
                else
                {
                    isOnCooldown = false;
                    shieldReady = true;  // NOW ready
                    Debug.Log("🛡️ Shield READY! (Press Q)");
                }
                lastCountdown = countdown;
            }
        }
        else
        {
            shieldReady = true;  // Default ready when no cooldown
        }
    }


   void ActivateShield()
{
    if (isShieldActive || !shieldReady || shieldObject == null) return;


    isShieldActive = true;
    isOnCooldown = true;
    shieldReady = false;
    cooldownEndTime = Time.time + shieldCooldown;
    lastCountdown = Mathf.CeilToInt(shieldCooldown) + 1;
    shieldObject.SetActive(true);  // Full object active!
    Debug.Log($"Shield activated! Cooldown: {shieldCooldown}s");


    StartCoroutine(ShieldTimer());
}


IEnumerator ShieldTimer()
{
    yield return new WaitForSeconds(shieldDuration);


    shieldObject.SetActive(false);  // Deactivates object + collider
    isShieldActive = false;
    Debug.Log("Shield duration ended. Cooldown starts.");
}


public void StartParryWindow()
{
    if (!isShieldActive || parryObject == null) return;


    inParryWindow = true;
    parryObject.SetActive(true);  // Full parry object!
    Debug.Log("Parry window OPEN (0.3s)!");


    StartCoroutine(CloseParryWindow());
}


IEnumerator CloseParryWindow()
{
    yield return new WaitForSeconds(parryWindow);
    inParryWindow = false;
    parryObject.SetActive(false);  // Deactivates
    Debug.Log("Parry window CLOSED.");
}


void OnDrawGizmosSelected()
{
    if (!shieldObject) return;


    // Parry pushback preview (blue ray from player forward)
    Gizmos.color = Color.blue;
    Vector3 previewDir = transform.forward;  // Or your shield face direction
    float previewLength = pushbackForce * 0.5f;  // Scale to visualize force (tune)
    Gizmos.DrawRay(transform.position + Vector3.up * 1f, previewDir * previewLength);  // From player height
    Gizmos.DrawSphere(transform.position + Vector3.up * 1f + previewDir * previewLength, 0.2f);  // End sphere


    // Labels
    #if UNITY_EDITOR
    UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"Pushback: {pushbackForce}");
    #endif
}


// Always-visible version (faint)
void OnDrawGizmos()
{
    if (!shieldObject) return;


    Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);  // Faint blue
    Vector3 previewDir = transform.forward;
    float previewLength = pushbackForce * 0.3f;
    Gizmos.DrawRay(transform.position + Vector3.up * 1f, previewDir * previewLength);
}

}
