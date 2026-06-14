using UnityEngine;
using UnityEngine.UI;
using TMPro; // Wajib untuk TextMeshPro

public class VirusManager : MonoBehaviour
{
    public int hpMaksimal = 100;
    public int hpSekarang;
    
    [Header("UI HP Monster")]
    public TMP_Text hpText; 
    public RectTransform hpFill;
    public float maxFillWidth = 806f; // Panjang bar HP ketika 100%

    private float originalXPos;

    void Start()
    {
        hpSekarang = hpMaksimal;
        if (hpFill != null)
        {
            originalXPos = hpFill.anchoredPosition.x; // Menyimpan posisi X asli
        }
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

    public void UpdateUI()
    {
        if (hpText != null)
        {
            hpText.text = hpSekarang.ToString(); // Hanya menampilkan angka sisa (contoh: "23")
        }

        if (hpFill != null)
        {
            // Menyesuaikan nilai Width (panjang) berdasarkan persentase (maksimal 806)
            float rasio = (float)hpSekarang / hpMaksimal;
            float newWidth = maxFillWidth * rasio;
            hpFill.sizeDelta = new Vector2(newWidth, hpFill.sizeDelta.y);

            // Menggeser titik pusat (X) agar UI selalu terlihat menyusut hanya dari sebelah kanan
            float lebarYangHilang = maxFillWidth - newWidth;
            float geserKeKiri = lebarYangHilang * hpFill.pivot.x;

            hpFill.anchoredPosition = new Vector2(originalXPos - geserKeKiri, hpFill.anchoredPosition.y);
        }
    }
}