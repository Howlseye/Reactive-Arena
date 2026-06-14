using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    [Header("UI & Interaksi")]
    public CombineSlot combineSlot;
    public Button btnCampur;
    public Button btnLempar;
    public GameObject kartuCampurPrefab; 

    // === TAMBAHAN BARU ===
    [Header("Posisi Kartu Hasil")]
    public Transform posisiResult; // Titik tempat kartu campur muncul
    private DraggableCard kartuCampurAktif; // Nyimpen data kartu campurnya

    [Header("Sistem Pertarungan")]
    public VirusManager virusTarget;
    public PlayerManager pemain;
    public int damageSeranganVirus = 15; 
    public GameObject vfxSeranganPrefab;
    [Range(0.1f, 5f)] public float skalaEfek = 1f; // Slider pengatur ukuran efek
    public AudioClip sfxSerangan;
    public AudioClip sfxCampur; // TAMBAHAN: Suara untuk animasi campur
    public AudioClip sfxMonsterSerang; // TAMBAHAN: Suara monster menyerang balik

    private bool sedangAnimasi = false;

    [Header("Sistem Deck (Reset Kartu)")]
    public Transform deckArea; 
    public GameObject prefabAir;
    public GameObject prefabGaram;
    public GameObject prefabAsam;
    public GameObject prefabLogam;

    void Update()
    {
        if (sedangAnimasi) return; // Mencegah tombol nyala tiba-tiba saat animasi berlangsung

        int totalKartuDiKotak = combineSlot.GetCards().Count;
        
        // Campur aktif kalau ada > 1 kartu di kotak
        btnCampur.interactable = totalKartuDiKotak > 1; 
        
        // Lempar HANYA aktif kalau:
        // 1. Ada tepat 1 kartu murni di kotak, ATAU
        // 2. Ada kartu hasil campur yang siap dilempar di sebelah kanan
        btnLempar.interactable = (totalKartuDiKotak == 1) || (kartuCampurAktif != null); 
    }

    public void OnClickCampur()
    {
        List<DraggableCard> kartuDiSlot = combineSlot.GetCards();
        if (kartuDiSlot.Count < 2) return;

        List<string> bahanReaksi = new List<string>();
        foreach (DraggableCard kartu in kartuDiSlot)
        {
            bahanReaksi.Add(kartu.cardType);
        }
        
        bahanReaksi.Sort();
        string namaResep = string.Join("+", bahanReaksi);

        // Jangan hapus kartu secara instan, jalankan animasi terlebih dahulu
        StartCoroutine(AnimasiCampurRoutine(namaResep));
    }

    private System.Collections.IEnumerator AnimasiCampurRoutine(string namaResep)
    {
        sedangAnimasi = true;
        
        // Matikan interaksi tombol agar pemain tidak iseng mengklik berulang-ulang
        btnCampur.interactable = false;
        btnLempar.interactable = false;

        List<DraggableCard> kartuDiSlot = new List<DraggableCard>(combineSlot.GetCards());
        List<Vector2> posisiAwal = new List<Vector2>();
        List<CanvasGroup> canvasGroups = new List<CanvasGroup>();

        // Simpan posisi dan matikan sensor klik pada kartu agar tidak bisa diseret saat animasi
        foreach (var kartu in kartuDiSlot)
        {
            posisiAwal.Add(kartu.GetComponent<RectTransform>().anchoredPosition);
            CanvasGroup cg = kartu.GetComponent<CanvasGroup>();
            if (cg != null) 
            {
                cg.blocksRaycasts = false; 
                canvasGroups.Add(cg);
            }
        }

        float durasi = 0.7f; // Lama waktu animasi (detik)
        float waktu = 0f;
        bool suaraDimainkan = false;

        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            float progress = waktu / durasi;
            float smoothT = Mathf.SmoothStep(0f, 1f, progress); // Memberikan pergerakan yang mulus

            for (int i = 0; i < kartuDiSlot.Count; i++)
            {
                if (kartuDiSlot[i] != null)
                {
                    // Gerakkan kartu perlahan menuju titik pusat kotak combine (Vector2.zero)
                    kartuDiSlot[i].GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(posisiAwal[i], Vector2.zero, smoothT);
                    
                    // Pudarkan transparansi kartu perlahan
                    if (canvasGroups.Count > i && canvasGroups[i] != null)
                    {
                        canvasGroups[i].alpha = Mathf.Lerp(1f, 0f, smoothT);
                    }
                }
            }

            // Saat animasi mencapai 80%, otomatis mainkan efek suara
            if (progress >= 0.8f && !suaraDimainkan)
            {
                if (sfxCampur != null)
                {
                    AudioSource.PlayClipAtPoint(sfxCampur, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
                }
                suaraDimainkan = true;
            }

            yield return null;
        }

        // Hancurkan kartu-kartu mentah setelah efek animasi selesai sepenuhnya
        combineSlot.ClearCardsAfterThrow();

        // Munculkan Kartu Gabungan di titik "Posisi Result" yang baru
        GameObject kartuBaruObj = Instantiate(kartuCampurPrefab, posisiResult);
        kartuCampurAktif = kartuBaruObj.GetComponent<DraggableCard>();
        
        kartuCampurAktif.isInCombine = true;
        kartuCampurAktif.isResultCard = true; // Tandai sebagai kartu result (mengunci kliknya)
        kartuCampurAktif.cardType = namaResep;
        
        // ANIMASI FADE IN UNTUK KARTU RESULT
        CanvasGroup cgResult = kartuBaruObj.GetComponent<CanvasGroup>();
        if (cgResult != null)
        {
            cgResult.alpha = 0f; // Mulai dari menghilang
            float durasiFadeIn = 0.4f;
            float waktuFade = 0f;

            while (waktuFade < durasiFadeIn)
            {
                waktuFade += Time.deltaTime;
                cgResult.alpha = Mathf.Lerp(0f, 1f, waktuFade / durasiFadeIn);
                yield return null;
            }
            cgResult.alpha = 1f; // Pastikan 100% solid di akhir
        }

        Debug.Log("Berhasil dicampur menjadi: " + namaResep);

        // Kunci semua kartu di deck agar tidak bisa diseret-seret lagi
        combineSlot.SetLockState(true);

        // Izinkan UI aktif kembali
        sedangAnimasi = false;
    }

    public void OnClickLempar()
    {
        if (sedangAnimasi) return;
        StartCoroutine(AnimasiLemparRoutine());
    }

    private System.Collections.IEnumerator AnimasiLemparRoutine()
    {
        sedangAnimasi = true;
        btnCampur.interactable = false;
        btnLempar.interactable = false;

        string resepYangDilempar = "";
        GameObject objekYangDilempar = null;

        // Tentukan kartu mana yang sedang dilempar
        if (kartuCampurAktif != null)
        {
            resepYangDilempar = kartuCampurAktif.cardType;
            objekYangDilempar = kartuCampurAktif.gameObject;
        }
        else
        {
            List<DraggableCard> kartuDiSlot = combineSlot.GetCards();
            if (kartuDiSlot.Count > 0)
            {
                resepYangDilempar = kartuDiSlot[0].cardType;
                objekYangDilempar = kartuDiSlot[0].gameObject;
            }
        }

        // ANIMASI LEMPAR: Menggeser kartu ke atas sambil memudar perlahan
        if (objekYangDilempar != null)
        {
            RectTransform rT = objekYangDilempar.GetComponent<RectTransform>();
            CanvasGroup cg = objekYangDilempar.GetComponent<CanvasGroup>();
            
            if (cg != null) cg.blocksRaycasts = false;

            Vector2 posisiAwal = rT.anchoredPosition;
            Vector2 posisiTarget = posisiAwal + new Vector2(0, 150f); // Geser 150 piksel ke atas

            float durasiLempar = 0.5f; // Kecepatan lempar
            float waktu = 0f;

            while (waktu < durasiLempar)
            {
                waktu += Time.deltaTime;
                float t = waktu / durasiLempar;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                rT.anchoredPosition = Vector2.Lerp(posisiAwal, posisiTarget, smoothT);
                if (cg != null) cg.alpha = Mathf.Lerp(1f, 0f, smoothT);

                yield return null;
            }
        }

        // Hancurkan fisik kartu di UI setelah dilempar
        if (kartuCampurAktif != null)
        {
            Destroy(kartuCampurAktif.gameObject);
            kartuCampurAktif = null;
        }
        else
        {
            combineSlot.ClearCardsAfterThrow();
        }

        // Hitung damage (dibungkus List karena fungsi HitungDamage butuh List)
        List<string> listResep = new List<string> { resepYangDilempar };
        int finalDamage = HitungDamage(listResep);
        
        Debug.Log("Melempar dengan Damage: " + finalDamage);
        
        if (vfxSeranganPrefab != null && virusTarget != null)
        {
            // Memunculkan efek ledakan/serangan tepat di posisi virus
            GameObject efek = Instantiate(vfxSeranganPrefab, virusTarget.transform.position, Quaternion.identity);
            
            // Ubah ukurannya sesuai dengan pengaturan slider
            efek.transform.localScale = new Vector3(skalaEfek, skalaEfek, skalaEfek);
            
            // Hancurkan efek setelah 2 detik agar tidak memberatkan memori
            Destroy(efek, 2f);
        }

        if (sfxSerangan != null)
        {
            // Memainkan suara (SFX) tepat di posisi kamera utama agar suaranya paling keras/jelas
            AudioSource.PlayClipAtPoint(sfxSerangan, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }
        
        virusTarget.KenaDamage(finalDamage);

        if (virusTarget.hpSekarang > 0)
        {
            // Monster membalas serangan dengan animasi sebelum ronde selesai
            yield return StartCoroutine(AnimasiMonsterSerangRoutine());
        }

        // Reset 4 kartu awal
        ResetDeck();
        
        // Buka kembali kunci kartu di deck untuk ronde lemparan selanjutnya
        combineSlot.SetLockState(false);

        sedangAnimasi = false;
    }

    private System.Collections.IEnumerator AnimasiMonsterSerangRoutine()
    {
        // Jeda 2 detik sebelum monster membalas serangan
        yield return new WaitForSeconds(2.0f);

        // Mainkan SFX Monster jika ada
        if (sfxMonsterSerang != null)
        {
            AudioSource.PlayClipAtPoint(sfxMonsterSerang, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }

        if (virusTarget != null)
        {
            Transform monsterT = virusTarget.transform;
            Vector3 posisiAwal = monsterT.localPosition;
            
            // Gerakan animasi: monster "menukik" atau maju ke arah bawah layar (mendekati posisi deck/pemain)
            Vector3 posisiMaju = posisiAwal + new Vector3(0, -100f, 0); 

            float durasiMaju = 0.1f;
            float durasiMundur = 0.2f;
            float waktu = 0f;

            // Gerakan maju cepat
            while (waktu < durasiMaju)
            {
                waktu += Time.deltaTime;
                monsterT.localPosition = Vector3.Lerp(posisiAwal, posisiMaju, waktu / durasiMaju);
                yield return null;
            }
            monsterT.localPosition = posisiMaju;

            // Berikan damage kepada pemain saat animasi berada di puncak serangan
            pemain.KenaSerang(damageSeranganVirus);

            // Gerakan mundur kembali ke posisi semula
            waktu = 0f;
            while (waktu < durasiMundur)
            {
                waktu += Time.deltaTime;
                monsterT.localPosition = Vector3.Lerp(posisiMaju, posisiAwal, waktu / durasiMundur);
                yield return null;
            }
            monsterT.localPosition = posisiAwal;
        }
        else
        {
            // Jika objek virus error, tetap jalankan damage-nya
            pemain.KenaSerang(damageSeranganVirus);
        }

        // Jeda sedikit sebelum memunculkan kartu baru agar perpindahannya mulus
        yield return new WaitForSeconds(0.2f);
    }

    private void ResetDeck()
    {
        foreach (Transform sisaKartu in deckArea)
        {
            Destroy(sisaKartu.gameObject);
        }

        Instantiate(prefabAir, deckArea);
        Instantiate(prefabGaram, deckArea);
        Instantiate(prefabAsam, deckArea);
        Instantiate(prefabLogam, deckArea);
    }

    private int HitungDamage(List<string> bahan)
    {
        bahan.Sort();
        string reaksi = string.Join("+", bahan);

        switch (reaksi)
        {
            case "Air": return 3;
            case "Garam": return 5;                
            case "Logam": return 5;
            case "Asam": return 8;
            case "Air+Garam": return 8; 
            case "Air+Asam": return 11; 
            case "Garam+Logam": return 10; 
            case "Air+Logam": return 8; 
            case "Asam+Garam": return 13; 
            case "Asam+Logam": return 13; 
            case "Air+Asam+Garam": return 16; 
            case "Air+Garam+Logam": return 13; 
            case "Air+Asam+Logam": return 16; 
            case "Asam+Garam+Logam": return 18; 
            default: return 0; 
        }
    }
}