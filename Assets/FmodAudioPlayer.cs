using System.IO;
using UnityEngine;
using FMOD;
using FMODUnity;

using Debug = UnityEngine.Debug;

public class FmodAudioPlayer : MonoBehaviour
{
    private FMOD.Sound sound;
    private FMOD.Channel channel;

    public string filePath = "test.wav";
    // Can be absolute or relative path depending on mode
    
    public bool loop = false;
    
    public void PlaySound()
    {
        FMOD.System system = RuntimeManager.CoreSystem;

        if (!system.hasHandle())
        {
            Debug.LogError("FMOD system not initialized.");
            return;
        }

        // Create sound
        MODE mode = MODE.DEFAULT | MODE._2D;
        if (loop) mode |= MODE.LOOP_NORMAL;

        var result = system.createSound(Path.Join(Application.streamingAssetsPath, filePath), mode, out sound);
        if (result != RESULT.OK)
        {
            Debug.LogError($"Failed to create sound: {result}");
            return;
        }

        // Play sound
        result = system.playSound(sound, default, false, out channel);
        if (result != RESULT.OK)
        {
            Debug.LogError($"Failed to play sound: {result}");
        }
        else
        {
            Debug.Log("Sound playing!");
        }
    }

    private void OnDestroy()
    {
        // Release sound to free memory
        sound.release();
    }
}