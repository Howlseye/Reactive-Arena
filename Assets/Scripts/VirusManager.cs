using UnityEngine;
using TMPro; // Wajib untuk TextMeshPro

public class VirusManager : MonoBehaviour
{
    public int hpMaksimal = 100;
    public int hpSekarang;
    
    // Ganti jadi TMP_Text agar bisa mendeteksi TextMeshPro di Inspector
    public TMP_Text hpText; 

    void Start()
    {
        hpSekarang = hpMaksimal;
        UpdateUI();
    }

    public void KenaDamage(int damage)
    {
        hpSekarang -= damage;
        if (hpSekarang <= 0)
        {
            hpSekarang = 0;
            UpdateUI();
            Debug.Log("VIRUS HANCUR! KAMU MENANG!");
            gameObject.SetActive(false); // Menghilangkan monster
        }
        else
        {
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (hpText != null)
        {
            hpText.text = "Virus Hacu\n" + hpSekarang + "/" + hpMaksimal;
        }
    }
}