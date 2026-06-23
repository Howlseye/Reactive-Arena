using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public int hpMaksimal = 100; // Skala maksimal 100
    public int hpSekarang;
    
    [Header("UI HP Player")]
    public TMP_Text hpText; 
    public RectTransform hpFill;
    public float maxFillWidth = 806f; // Panjang bar HP ketika 100%
    public AudioClip sfxDefeat; // TAMBAHAN: Suara saat pemain kalah

    private float originalXPos;

    void Start()
    {
        hpSekarang = hpMaksimal; // Set nyawa awal ke 100
        if (hpFill != null)
        {
            originalXPos = hpFill.anchoredPosition.x; // Menyimpan posisi X asli
        }
        UpdateUI();
    }

    // Dipanggil saat virus menyerang balik
    public void KenaSerang(int damageVirus)
    {
        hpSekarang -= damageVirus;
        Debug.Log("Diserang Virus! Sisa HP Pemain: " + hpSekarang);

        if (hpSekarang <= 0)
        {
            hpSekarang = 0;
            UpdateUI();
            
            // Hentikan semua suara latar/musik yang sedang berjalan
            AudioSource[] semuaAudio = FindObjectsOfType<AudioSource>();
            foreach (AudioSource audio in semuaAudio)
            {
                audio.Stop();
            }
            
            if (sfxDefeat != null)
            {
                // Mainkan suara kekalahan
                AudioSource.PlayClipAtPoint(sfxDefeat, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            }

            GameOver();
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
            hpText.text = hpSekarang.ToString(); // Menampilkan angka (misal "92")
        }

        if (hpFill != null)
        {
            // Menyesuaikan nilai Width berdasarkan persentase (maksimal 806)
            float rasio = (float)hpSekarang / hpMaksimal;
            float newWidth = maxFillWidth * rasio;
            hpFill.sizeDelta = new Vector2(newWidth, hpFill.sizeDelta.y);

            // Menggeser titik pusat (X) agar UI selalu terlihat menyusut hanya dari sebelah kanan
            float lebarYangHilang = maxFillWidth - newWidth;
            float geserKeKiri = lebarYangHilang * hpFill.pivot.x;

            hpFill.anchoredPosition = new Vector2(originalXPos - geserKeKiri, hpFill.anchoredPosition.y);
        }
    }

    void GameOver()
    {
        Debug.Log("HP HABIS! GAME OVER!");
        // Di sini kamu bisa memunculkan Panel UI "Game Over" atau mengulang Scene
    }
}