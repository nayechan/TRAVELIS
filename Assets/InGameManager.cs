using UnityEngine;
using System.Collections;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private Gameplay gameplay;
    
    private SoundLoader soundLoader;

    public void StartGame()
    {
        StartCoroutine(LoadGame());
    }
    
    private IEnumerator LoadGame()
    {
        Debug.Log("Applying FMOD output settings...");
        FMODAudioSettings.Instance.ApplyOutputSettings();

        yield return null;

        Debug.Log("Loading chart...");
        yield return chartLoader.Load();

        if (!chartLoader.IsLoaded)
        {
            Debug.LogError("Chart loading failed!");
            yield break;
        }
        
        Debug.Log("Loading sounds...");
        soundLoader = new SoundLoader();
        yield return soundLoader.LoadSoundsAsync(chartLoader.Chart);


        Debug.Log("All set. Starting gameplay...");
        StartGameplay();
    }

    private void StartGameplay()
    {
        gameplay.Init(chartLoader.Chart, soundLoader);
    }

    private void OnDestroy()
    {
        soundLoader?.ReleaseAll();
    }
}