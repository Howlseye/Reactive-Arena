using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // === TAMBAHAN 1: Wajib untuk mereload scene ===

public class BattleManager : MonoBehaviour
{
    // === TAMBAHAN 2: Penanda Coba Lagi (Agar cerita bisa di-skip) ===
    public static bool isRetry = false; 
    // ==============================================================

    [Header("UI & Interaksi")]
    public CombineSlot combineSlot;
    public Button btnCampur;
    public Button btnLempar;
    public GameObject kartuCampurPrefab; 

    // === TAMBAHAN UI KONDISI MENANG/KALAH ===
    [Header("UI Status Game")]
    public GameObject panelGameOver;
    public GameObject panelLevelSelesai;
    public GameObject panelPause; // TAMBAHAN PAUSE
    // =======================================

    // === TAMBAHAN PENGATURAN LEVEL SELANJUTNYA ===
    [Header("Sistem Pindah Level")]
    [Tooltip("Masukkan nama Scene untuk level berikutnya di Unity Inspector")]
    public string namaLevelSelanjutnya; 
    // =============================================

    [Header("Posisi Kartu Hasil")]
    public Transform posisiResult; // Titik tempat kartu campur muncul
    private DraggableCard kartuCampurAktif; // Nyimpen data kartu campurnya

    [Header("Sistem Pertarungan")]
    public VirusManager virusTarget;
    public PlayerManager pemain;
    public int damageSeranganVirus = 15; 
    
    public enum TipeAnimasiSerangan { Animator, GerakMenukik }
    public TipeAnimasiSerangan tipeAnimasiSerang = TipeAnimasiSerangan.GerakMenukik;
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

    private Vector2 initialCardSize;
    private System.Collections.Generic.List<Vector2> initialCardPositions = new System.Collections.Generic.List<Vector2>();

    // === TAMBAHAN: FUNGSI START ===
    void Start()
    {
        // Pastikan panel disembunyikan saat game baru mulai
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelLevelSelesai != null) panelLevelSelesai.SetActive(false);
        if (panelPause != null) panelPause.SetActive(false);

        // Simpan ukuran kartu pertama yang ada di scene (agar konsisten saat reset deck)
        if (deckArea != null && deckArea.childCount > 0)
        {
            RectTransform cardRt = deckArea.GetChild(0).GetComponent<RectTransform>();
            if (cardRt != null) initialCardSize = cardRt.sizeDelta;

            // Simpan posisi masing-masing kartu agar jaraknya tetap saat di-reset
            for (int i = 0; i < deckArea.childCount; i++)
            {
                RectTransform rt = deckArea.GetChild(i).GetComponent<RectTransform>();
                if (rt != null) initialCardPositions.Add(rt.anchoredPosition);
            }
        }
    }
    // ==============================

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
            GameObject efek = Instantiate(vfxSeranganPrefab, virusTarget.transform.position, Quaternion.identity);
            efek.transform.localScale = new Vector3(skalaEfek, skalaEfek, skalaEfek);
            Destroy(efek, 2f);
        }

        if (sfxSerangan != null)
        {
            AudioSource.PlayClipAtPoint(sfxSerangan, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }
        
        virusTarget.KenaDamage(finalDamage);

        // === KONDISI MENANG: CEK HP VIRUS ===
        if (virusTarget.hpSekarang <= 0)
        {
            // Jika virus mati, jalankan coroutine menang
            yield return StartCoroutine(AnimasiMenangRoutine());
            yield break; // HENTIKAN proses di sini
        }
        else
        {
            // Jika virus masih hidup, monster membalas serangan
            yield return StartCoroutine(AnimasiMonsterSerangRoutine());
        }
        // =====================================

        // Reset 4 kartu awal dengan animasi
        yield return StartCoroutine(ResetDeckRoutine());
        
        // Buka kembali kunci kartu di deck untuk ronde lemparan selanjutnya
        combineSlot.SetLockState(false);

        sedangAnimasi = false;
    }

    private System.Collections.IEnumerator AnimasiMenangRoutine()
    {
        sedangAnimasi = true; 

        // Tunggu sebentar setelah efek serangan pemain selesai
        yield return new WaitForSeconds(1.0f);

        // Animasi Monster Fade Out
        if (virusTarget != null)
        {
            SpriteRenderer monsterRenderer = virusTarget.GetComponent<SpriteRenderer>();
            UnityEngine.UI.Image monsterImage = virusTarget.GetComponent<UnityEngine.UI.Image>();
            
            float durasiFade = 1.5f;
            float waktu = 0f;

            if (monsterRenderer != null)
            {
                Color warna = monsterRenderer.color;
                while (waktu < durasiFade)
                {
                    waktu += Time.deltaTime;
                    warna.a = Mathf.Lerp(1f, 0f, waktu / durasiFade);
                    monsterRenderer.color = warna;
                    yield return null;
                }
            }
            else if (monsterImage != null)
            {
                Color warna = monsterImage.color;
                while (waktu < durasiFade)
                {
                    waktu += Time.deltaTime;
                    warna.a = Mathf.Lerp(1f, 0f, waktu / durasiFade);
                    monsterImage.color = warna;
                    yield return null;
                }
            }
            else 
            {
                // Jika objeknya tidak memiliki komponen yang bisa di-fade, tunggu saja
                yield return new WaitForSeconds(durasiFade);
            }
        }

        // Jeda sedikit sebelum panel muncul agar tidak mengagetkan
        yield return new WaitForSeconds(0.5f);

        if (panelLevelSelesai != null) yield return StartCoroutine(PanelPopUpRoutine(panelLevelSelesai));
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
            if (tipeAnimasiSerang == TipeAnimasiSerangan.Animator)
            {
                Animator monsterAnim = virusTarget.GetComponent<Animator>();
                if (monsterAnim != null)
                {
                    monsterAnim.SetTrigger("Attack");
                }
                
                // Waktu tunggu agar damage diberikan saat animasi attack sedang/telah memukul (disesuaikan dengan durasi animasi)
                yield return new WaitForSeconds(0.5f); 

                // Berikan damage kepada pemain
                pemain.KenaSerang(damageSeranganVirus);
                
                // Tunggu sedikit sisa animasi jika diperlukan sebelum giliran berakhir
                yield return new WaitForSeconds(0.5f);
            }
            else // tipeAnimasiSerang == TipeAnimasiSerangan.GerakMenukik
            {
                // FALLBACK: Animasi lama (gerakan turun/menukik manual via Transform)
                Transform monsterT = virusTarget.transform;
                Vector3 posisiAwal = monsterT.localPosition;
                
                Vector3 posisiMaju = posisiAwal + new Vector3(0, -100f, 0); 
                float durasiMaju = 0.1f;
                float durasiMundur = 0.2f;
                float waktu = 0f;

                while (waktu < durasiMaju)
                {
                    waktu += Time.deltaTime;
                    monsterT.localPosition = Vector3.Lerp(posisiAwal, posisiMaju, waktu / durasiMaju);
                    yield return null;
                }
                monsterT.localPosition = posisiMaju;

                pemain.KenaSerang(damageSeranganVirus);

                waktu = 0f;
                while (waktu < durasiMundur)
                {
                    waktu += Time.deltaTime;
                    monsterT.localPosition = Vector3.Lerp(posisiMaju, posisiAwal, waktu / durasiMundur);
                    yield return null;
                }
                monsterT.localPosition = posisiAwal;
            }
        }
        else
        {
            // Jika objek virus error, tetap jalankan damage-nya
            pemain.KenaSerang(damageSeranganVirus);
        }

        // === KONDISI KALAH: CEK HP PEMAIN ===
        if (pemain.hpSekarang <= 0) 
        {
            sedangAnimasi = true; // Mengunci UI agar tidak bisa ditekan
            yield return new WaitForSeconds(1.5f); // Jeda sebelum memunculkan panel
            if (panelGameOver != null) yield return StartCoroutine(PanelPopUpRoutine(panelGameOver));
            yield break; // Hentikan proses animasi
        }
        // =====================================

        // Jeda sedikit sebelum memunculkan kartu baru agar perpindahannya mulus
        yield return new WaitForSeconds(0.2f);
    }

    // === TAMBAHAN: Animasi Pop Up Panel ===
    private System.Collections.IEnumerator PanelPopUpRoutine(GameObject panel)
    {
        panel.SetActive(true);
        
        // Cari objek bernama "Content" di dalam panel (supaya background gelap tidak ikut memantul)
        Transform targetAnim = panel.transform.Find("Content");
        if (targetAnim == null) targetAnim = panel.transform.Find("content");
        if (targetAnim == null) targetAnim = panel.transform; // Fallback jika tidak ada objek Content

        targetAnim.localScale = Vector3.zero;

        float durasi = 0.4f;
        float waktu = 0f;

        while (waktu < durasi)
        {
            waktu += Time.unscaledDeltaTime; 
            float t = waktu / durasi;
            
            // Efek pop-up memantul (ease-out back)
            float t1 = t - 1f;
            float scale = 1f + 2.70158f * Mathf.Pow(t1, 3f) + 1.70158f * Mathf.Pow(t1, 2f);
            
            if (scale < 0) scale = 0; // Mencegah nilai scale terbalik
            
            targetAnim.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        targetAnim.localScale = Vector3.one;
    }
    // ======================================

    private System.Collections.IEnumerator ResetDeckRoutine()
    {
        foreach (Transform sisaKartu in deckArea)
        {
            Destroy(sisaKartu.gameObject);
        }

        SpawnCard(prefabAir, 0);
        yield return new WaitForSeconds(0.1f);
        SpawnCard(prefabAsam, 1);
        yield return new WaitForSeconds(0.1f);
        SpawnCard(prefabGaram, 2);
        yield return new WaitForSeconds(0.1f);
        SpawnCard(prefabLogam, 3);
    }

    private void SpawnCard(GameObject prefab, int index)
    {
        GameObject newCard = Instantiate(prefab, deckArea, false);
        
        RectTransform newRect = newCard.GetComponent<RectTransform>();
        
        if (newRect != null)
        {
            // Terapkan ukuran yang sudah kita simpan di awal Start()
            if (initialCardSize != Vector2.zero) 
            {
                newRect.sizeDelta = initialCardSize;
            }
            else 
            {
                newRect.sizeDelta = prefab.GetComponent<RectTransform>().sizeDelta;
            }

            // Kembalikan ke posisi awal yang sudah disimpan jika ada
            if (index < initialCardPositions.Count)
            {
                newRect.anchoredPosition = initialCardPositions[index];
            }

            StartCoroutine(CardPopUpRoutine(newRect));
        }
    }

    private System.Collections.IEnumerator CardPopUpRoutine(RectTransform card)
    {
        card.localScale = Vector3.zero;

        float durasi = 0.3f;
        float waktu = 0f;

        while (waktu < durasi)
        {
            waktu += Time.deltaTime; 
            float t = waktu / durasi;
            
            float t1 = t - 1f;
            float scale = 1f + 2.70158f * Mathf.Pow(t1, 3f) + 1.70158f * Mathf.Pow(t1, 2f);
            if (scale < 0) scale = 0;
            
            if (card == null) yield break; // Jaga-jaga jika dihancurkan di tengah animasi
            card.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        if (card != null) card.localScale = Vector3.one;
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

    // === TAMBAHAN 3: Fungsi untuk dipanggil oleh tombol Coba Lagi ===
    public void UlangiGame()
    {
        isRetry = true; // Tandai bahwa ini adalah ulangan, jadi cerita bisa di-skip
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // === TAMBAHAN BARU: FUNGSI PINDAH LEVEL / HALAMAN SELANJUTNYA ===
    public void PindahKeLevelSelanjutnya()
    {
        // Reset penanda retry agar level baru berjalan normal (cerita tidak ter-skip otomatis)
        isRetry = false; 

        // Pastikan variabel nama level tidak kosong
        if (!string.IsNullOrEmpty(namaLevelSelanjutnya))
        {
            Time.timeScale = 1f; // Pastikan waktu berjalan normal
            AudioListener.pause = false; // Pastikan audio menyala
            SceneManager.LoadScene(namaLevelSelanjutnya);
        }
        else
        {
            Debug.LogError("Nama Level Selanjutnya belum diisi di Inspector Unity!");
        }
    }

    // === FITUR PAUSE ===
    public void PauseGame()
    {
        Time.timeScale = 0f;          // Menghentikan waktu/animasi
        AudioListener.pause = true;   // Mematikan sementara semua suara (BGM/SFX)
        if (panelPause != null) StartCoroutine(PanelPopUpRoutine(panelPause));
    }

    public void ResumeGame()
    {
        if (panelPause != null) 
        {
            StartCoroutine(PanelPopOutRoutine(panelPause));
        }
        else 
        {
            Time.timeScale = 1f;          
            AudioListener.pause = false;  
        }
    }

    private System.Collections.IEnumerator PanelPopOutRoutine(GameObject panel)
    {
        Transform targetAnim = panel.transform.Find("Content");
        if (targetAnim == null) targetAnim = panel.transform.Find("content");
        if (targetAnim == null) targetAnim = panel.transform;

        float durasi = 0.3f;
        float waktu = 0f;

        while (waktu < durasi)
        {
            waktu += Time.unscaledDeltaTime; 
            float t = waktu / durasi;
            
            // Efek pop-out memantul terbalik (anticipation)
            float s = 1f - (2.70158f * Mathf.Pow(t, 3f) - 1.70158f * Mathf.Pow(t, 2f));
            if (s < 0) s = 0;
            
            targetAnim.localScale = new Vector3(s, s, s);
            yield return null;
        }

        panel.SetActive(false);
        targetAnim.localScale = Vector3.one; // Kembalikan ke normal

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void KeMenuUtama()
    {
        Time.timeScale = 1f;          // Wajib dikembalikan ke 1 agar scene lain tidak terhenti
        AudioListener.pause = false;
        SceneManager.LoadScene("HomeScene"); 
    }
}