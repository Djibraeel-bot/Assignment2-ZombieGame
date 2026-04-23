using UnityEngine;
using TMPro;

public class PlayerPoints : MonoBehaviour
{
     public static PlayerPoints instance;

    [Header("Points")]
    public int points = 500; // starting points

    [Header("UI")]
    public TMP_Text pointsText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        points += amount;
        UpdateUI();
    }

    public bool SpendPoints(int amount)
    {
        if (points >= amount)
        {
            points -= amount;
            UpdateUI();
            return true;
        }

        return false;
    }

    void UpdateUI()
    {
        if (pointsText != null)
            pointsText.text = points.ToString();
    }
}
