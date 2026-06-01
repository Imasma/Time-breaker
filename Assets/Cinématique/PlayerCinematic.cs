using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CinematicPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextScene = "Playground 2"; // Nom de ta scène de jeu

    void Start()
    {
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextScene);
    }

    // ✅ Optionnel — skip avec une touche
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}