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

    [Header("Sistem Deck (Reset Kartu)")]
    public Transform deckArea; 
    public GameObject prefabAir;
    public GameObject prefabGaram;
    public GameObject prefabAsam;
    public GameObject prefabLogam;

    void Update()
    {
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

        // Bersihkan kotak putih setelah dicampur
        combineSlot.ClearCardsAfterThrow();

        // Munculkan Kartu Campur di titik "Posisi Result" yang baru
        GameObject kartuBaruObj = Instantiate(kartuCampurPrefab, posisiResult);
        kartuCampurAktif = kartuBaruObj.GetComponent<DraggableCard>();
        
        kartuCampurAktif.isInCombine = true;
        kartuCampurAktif.cardType = namaResep;
        
        Debug.Log("Berhasil dicampur menjadi: " + namaResep);
    }

    public void OnClickLempar()
    {
        string resepYangDilempar = "";

        // Cek apakah ada kartu hasil campur yang siap dilempar
        if (kartuCampurAktif != null)
        {
            resepYangDilempar = kartuCampurAktif.cardType;
            Destroy(kartuCampurAktif.gameObject); // Hancurkan setelah dilempar
            kartuCampurAktif = null;
        }
        else
        {
            // Kalau nggak ada kartu campur, berarti lempar 1 kartu murni dari kotak putih
            List<DraggableCard> kartuDiSlot = combineSlot.GetCards();
            if (kartuDiSlot.Count == 0) return;

            resepYangDilempar = kartuDiSlot[0].cardType;
            combineSlot.ClearCardsAfterThrow();
        }

        // Hitung damage (dibungkus List karena fungsi HitungDamage butuh List)
        List<string> listResep = new List<string> { resepYangDilempar };
        int finalDamage = HitungDamage(listResep);
        
        Debug.Log("Melempar dengan Damage: " + finalDamage);
        
        virusTarget.KenaDamage(finalDamage);

        if (virusTarget.hpSekarang > 0)
        {
            pemain.KenaSerang(damageSeranganVirus);
        }

        // Reset 4 kartu awal
        ResetDeck();
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