using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Health")]
    public Image[] hearts;           
    public Sprite fullHeart;         
    public Sprite emptyHeart;        

    [Header("Lives")]
    public TMP_Text livesText;
    [Header("Coins")]
    public TMP_Text coinText;

    [Header("Initial UI States")]
    [SerializeField] private int startingHealth = 3;   
    [SerializeField] private int startingLives = 3;    
    [SerializeField] private int startingCoins = 0;    


    private void Start()
    {
        UpdateHearts(startingHealth);
        UpdateLives(startingLives);
        UpdateCoins(startingCoins);
    }


    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
        }
    }


    public void UpdateLives(int currentLives)
    {
        if (livesText != null)
            livesText.text = "x " + currentLives;
    }

    public void UpdateCoins(int coinCount)
    {
        if (coinText != null)
            coinText.text = coinCount.ToString("D2");
    }
}
