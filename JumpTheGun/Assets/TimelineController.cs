using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    private bool isPausedForInput = false;

    void Update()
    {
        // Check if the timeline is waiting for the player to press "E"
        if (isPausedForInput && Input.GetKeyDown(KeyCode.E))
        {
            ResumeTimeline();
        }
    }

    // This method will be called by the Timeline Signal
    public void PauseTimeline()
    {
        director.Pause();
        isPausedForInput = true;
        Debug.Log("Timeline Paused. Press 'E' to continue.");
    }

    private void ResumeTimeline()
    {
        director.Play();
        isPausedForInput = false;
        Debug.Log("Timeline Resumed.");
    }
}
