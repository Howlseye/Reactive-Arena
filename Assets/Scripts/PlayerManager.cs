using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int hpPemain = 50; // Nyawa awal pemain
    
    // Dipanggil saat virus menyerang balik
    public void KenaSerang(int damageVirus)
    {
        hpPemain -= damageVirus;
        Debug.Log("Diserang Virus! Sisa HP Pemain: " + hpPemain);

        if (hpPemain <= 0)
        {
            hpPemain = 0;
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("HP HABIS! GAME OVER!");
        // Di sini kamu bisa memunculkan Panel UI "Game Over" atau mengulang Scene
    }
}