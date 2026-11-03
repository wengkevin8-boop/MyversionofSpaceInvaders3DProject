using UnityEngine;
using TMPro;

public class AlienCounter : MonoBehaviour
{
    [Header("Dynamic")]
    public int score = 0;
    TextMeshProUGUI uiText;

    void Start()
    {
        uiText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (uiText != null) uiText.text = "Remaining Aliens: " + score.ToString("#,0");
    }
}
