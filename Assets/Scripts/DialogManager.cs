using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Wajib untuk TextMeshPro Teks
using UnityEngine.UI; // Wajib untuk UI Button

public class DialogManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI komponenTeks;  // Tarik objek TextCerita ke sini
    public GameObject panelDialog;       // Tarik objek PanelDialog ke sini
    public Button tombolLanjut;          // Tarik objek TombolLanjut (Next) ke sini
    
    [Header("Isi Cerita")]
    [TextArea(3, 5)]
    public string[] daftarCerita;        // Tempat isi 4 paragraf cerita di Inspector
    
    [Header("Pengaturan Kecepatan")]
    public float kecepatanKetik = 0.05f; // Jeda waktu antar huruf (detik)

    [Header("Audio Settings")]
    public AudioClip sfxKetik;           // Tarik file audio klik/ketik pendek kamu ke sini
    private AudioSource audioSource;
    
    private int indeksCerita = 0;
    private bool sedangKetik = false;

    void Start()
    {
        // Otomatis mengambil komponen AudioSource di objek ini, atau buat baru jika belum ada
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Pastikan settingan audio aman dari code agar tidak nge-loop berantakan
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        // Munculkan panel cerita di awal scene
        panelDialog.SetActive(true);
        tombolLanjut.gameObject.SetActive(false); 
        
        MulaiCerita();
    }

    void MulaiCerita()
    {
        indeksCerita = 0;
        StartCoroutine(KetikKalimat());
    }

IEnumerator KetikKalimat()
{
    sedangKetik = true;
    komponenTeks.text = "";
    tombolLanjut.gameObject.SetActive(false);

    int penghitungHuruf = 0; // Variabel baru untuk menghitung huruf

    foreach (char huruf in daftarCerita[indeksCerita].ToCharArray())
    {
        komponenTeks.text += huruf;

        // Putar suara jika bukan spasi
        if (huruf != ' ' && sfxKetik != null)
        {
            // Mainkan suara hanya jika sedang tidak ada suara yang dimainkan
            if (!audioSource.isPlaying)
            {
                audioSource.clip = sfxKetik;
                audioSource.pitch = Random.Range(0.9f, 1.1f); // Tambahan variasi pitch agar lebih natural
                audioSource.Play(); 
            }
        }

        penghitungHuruf++; // Tambah hitungan huruf
        yield return new WaitForSeconds(kecepatanKetik);
    }

    if (audioSource != null)
    {
        audioSource.Stop(); 
    }

    sedangKetik = false;
    tombolLanjut.gameObject.SetActive(true);
}

    // Fungsi ini dihubungkan ke Button OnClick() tombol "Next" di Unity Inspector
    public void KalimatBerikutnya()
    {
        // Jika teks masih berjalan, tombol tidak bisa diklik untuk mencegah error
        if (sedangKetik) return;

        // Jika masih ada cerita di halaman berikutnya
        if (indeksCerita < daftarCerita.Length - 1)
        {
            indeksCerita++;
            StartCoroutine(KetikKalimat());
        }
        else
        {
            // Jika seluruh cerita (Element 0 sampai 3) sudah habis
            panelDialog.SetActive(false); // Tutup kotak cerita
            MulaiGameplay();              // Jalankan game pertempurannya!
        }
    }

    void MulaiGameplay()
    {
        Debug.Log("Cerita selesai! Aktifkan sistem pertarungan ramuan.");
        // Di sini nanti kamu tinggal memunculkan musuh dan deck kartu ramuanmu
    }
}