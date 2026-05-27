using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameMenu : MonoBehaviour
{
   [SerializeField] private string gameSceneName;
   private void Start()
   {
        SoundManager.PlayMusic(SoundType.BACKGROUND_MUSIC, 0.5f);
   }
   public void Play()
   {
        SceneManager.LoadScene(gameSceneName);  
   }
   
   public void Quit()
    {
        #if UNITY_EDITOR    
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
   
}
