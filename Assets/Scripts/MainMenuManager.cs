using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    // Fungsi ini yang akan dipanggil saat tombol diklik
    public void BukaMainScene()
    {
        SceneManager.LoadScene("MainScene"); 
    }
}