using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FMOD;
using FMODUnity;
using Debug = UnityEngine.Debug;

public class SoundLoader
{
    private Dictionary<string, Sound> soundMap = new();
    private FMOD.System fmodSystem;

    public SoundLoader()
    {
        fmodSystem = RuntimeManager.CoreSystem;
    }

    public IEnumerator LoadSoundsAsync(ChartData chart)
    {
        foreach (var entry in chart.wavPath)
        {
            string id = entry.Key.ToUpper();
            string filename = entry.Value;
            string fullPath = Path.Combine(chart.path, filename);

#if UNITY_ANDROID && !UNITY_EDITOR
            var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV);
            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Failed to load audio file {filename}: {www.error}");
                continue;
            }

            // Save temporarily to persistent path since FMOD can't stream UnityWebRequest buffer directly
            string tempPath = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllBytes(tempPath, www.downloadHandler.data);
            fullPath = tempPath;
#endif

            RESULT result = fmodSystem.createSound(fullPath, MODE.DEFAULT, out Sound sound);
            if (result != RESULT.OK)
            {
                Debug.LogError($"FMOD failed to load {fullPath}: {result}");
                continue;
            }

            soundMap[id] = sound;

            // Yield to prevent frame spikes
            yield return null;
        }

        Debug.Log($"Loaded {soundMap.Count} sounds.");
    }

    public void Play(string wavId)
    {
        wavId = wavId.ToUpper();
        if (soundMap.TryGetValue(wavId, out Sound sound))
        {
            fmodSystem.playSound(sound, default, false, out Channel channel);
        }
        else
        {
            Debug.LogWarning($"Tried to play missing WAV ID: {wavId}");
        }
    }

    public void ReleaseAll()
    {
        foreach (var pair in soundMap)
        {
            pair.Value.release();
        }
        soundMap.Clear();
    }
}
