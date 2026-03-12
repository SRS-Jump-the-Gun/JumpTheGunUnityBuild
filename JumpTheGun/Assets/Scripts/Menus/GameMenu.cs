using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameMenu : MonoBehaviour
{
   [SerializeField] private string gameSceneName;

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
