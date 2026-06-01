using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using FMODUnity;

public class MainMenu : MonoBehaviour
{
    public GameObject menuUI;
    public GameObject cinematicUI;
    public VideoPlayer videoPlayer;
    public AudioSource cinematicAudio; // Audio classique pour la cinématique
    public string nextScene = "Playground 2";

    private bool cinematicStarted = false; // ✅ Flag pour savoir si la ciné est lancée

    public void PlayGame()
    {
        menuUI.SetActive(false);
        cinematicUI.SetActive(true);
        cinematicStarted = true; // ✅ Cette ligne manque !
        videoPlayer.Play();

        FMOD.Studio.Bus masterBus;
        FMODUnity.RuntimeManager.StudioSystem.getBus("bus:/", out masterBus);
        masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        FMODUnity.RuntimeManager.PlayOneShot("event:/CinematicMusic");
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        RuntimeManager.CoreSystem.mixerResume();
        SceneManager.LoadScene(nextScene);
    }

    void Update()
    {
        // ✅ Utilise cinematicStarted au lieu de videoPlayer.isPlaying
        if (cinematicStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                RuntimeManager.CoreSystem.mixerResume();
                SceneManager.LoadScene(nextScene);
            }
        }
    }
}