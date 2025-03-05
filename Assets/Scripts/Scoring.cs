using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Scoring : MonoBehaviour
{
    public int score;
    [SerializeField] private TMPro.TextMeshProUGUI scoreDisplay;

    // Update is called once per frame
    void Update()
    {
        scoreDisplay.text = "Score : " + score.ToString();
    }
}
