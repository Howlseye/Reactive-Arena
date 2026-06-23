using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI komponenTeks;  
    public GameObject backgroundCerita;   // Objek Background abu-abu
    public Button tombolLanjut;           // Tombol Next kamu
    
    [Header("Isi Cerita")]
    [TextArea(3, 5)]
    public string[] daftarCerita;
    
    [Header("Pengaturan Kecepatan")]
    public float kecepatanKetik = 0.05f;

    [Header("Audio Settings")]
    public AudioClip sfxKetik;
    private AudioSource audioSource;

    [Header("Pengaturan BGM")]
    public GameObject bgmTransition;      
    public GameObject bgmMain;            
    
    private int indeksCerita = 0;
    private bool sedangKetik = false;
    private Coroutine efekKetik; 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        // Pastikan Background menyala di awal cerita
        if (backgroundCerita != null) backgroundCerita.SetActive(true);
        
        // Sembunyikan tombol Next di awal biar ga muncul sebelum teks diketik
        if (tombolLanjut != null) tombolLanjut.gameObject.SetActive(false); 

        MulaiCerita();
    }

    void MulaiCerita()
    {
        indeksCerita = 0;
        if (efekKetik != null) StopCoroutine(efekKetik);
        efekKetik = StartCoroutine(KetikKalimat());
    }

    IEnumerator KetikKalimat()
    {
        sedangKetik = true;
        komponenTeks.text = "";

        // ✨ BARU: Sembunyikan tombol Next pas teks mulai berjalan
        if (tombolLanjut != null) tombolLanjut.gameObject.SetActive(false);

        foreach (char huruf in daftarCerita[indeksCerita].ToCharArray())
        {
            komponenTeks.text += huruf;

            if (huruf != ' ' && sfxKetik != null)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = sfxKetik;
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.Play(); 
                }
            }
            yield return new WaitForSeconds(kecepatanKetik);
        }

        if (audioSource != null) audioSource.Stop();
        sedangKetik = false;

        // ✨ BARU: Munculkan kembali tombol Next setelah seluruh teks selesai diketik
        if (tombolLanjut != null) tombolLanjut.gameObject.SetActive(true);
    }

    public void KalimatBerikutnya()
    {
        // Pengaman ekstra jika tombol ditekan secara paksa saat mengetik
        if (sedangKetik) return;

        if (indeksCerita < daftarCerita.Length - 1)
        {
            if (!string.IsNullOrEmpty(daftarCerita[indeksCerita + 1])) 
            {
                indeksCerita++;
                if (efekKetik != null) StopCoroutine(efekKetik);
                efekKetik = StartCoroutine(KetikKalimat());
            }
            else 
            {
                MulaiGameplay();
            }
        }
        else
        {
            MulaiGameplay();
        }
    }

    public void SkipCerita()
    {
        // Hentikan efek ketik yang sedang berjalan
        if (efekKetik != null) StopCoroutine(efekKetik);
        
        // Hentikan suara ketikan
        if (audioSource != null) audioSource.Stop();
        
        sedangKetik = false;

        // Langsung tutup panel dan mulai game
        MulaiGameplay();
    }

    void MulaiGameplay()
    {
        Debug.Log("Game Dimulai!");
        
        // Matikan KESELURUHAN Background abu-abu agar game-nya terlihat
        if (backgroundCerita != null) backgroundCerita.SetActive(false);

        // Ganti lagu ke game utama
        if (bgmTransition != null) bgmTransition.SetActive(false);
        if (bgmMain != null) bgmMain.SetActive(true);
    }
}