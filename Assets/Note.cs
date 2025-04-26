using UnityEngine;

public class Note : MonoBehaviour
{
    public double measure;
    public string wavId;
    public bool HasPlayed { get; set; }

    private Gameplay gameplay;
    private SoundLoader soundLoader;
    private int lane;

    public void Setup(Gameplay _gameplay, SoundLoader _soundLoader, double _measure, string _wav, int _lane)
    {
        gameplay = _gameplay;
        soundLoader = _soundLoader;
        measure = _measure;
        wavId = _wav;
        lane = _lane;
        HasPlayed = false;
    }

    private void Update()
    {
        float timeSinceStart = Time.time - gameplay.StartTime;
        float playTime = (float)measure * gameplay.SecondsPerMeasure;

        // Play sound when it hits the play point
        if (!HasPlayed && Mathf.Abs(timeSinceStart - playTime) < 0.02f)
        {
            soundLoader.Play(wavId);
            HasPlayed = true;
        }

        // Move note down over time
        float noteY = gameplay.SpawnY + (playTime - timeSinceStart) * 3.0f;
        transform.localPosition = new Vector3(transform.localPosition.x, noteY, 0);
    }
}