using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private Transform noteParent;
    [SerializeField] private int laneCount = 128;

    private ChartData chart;
    private SoundLoader soundLoader;
    private List<Queue<GameObject>> notePools;
    private List<BeatData>[] beatData;

    [field: SerializeField]
    public float SecondsPerMeasure { get; private set; } = 2f;

    [field: SerializeField]
    public float SpawnY { get; private set; } = 10f;

    [field: SerializeField]
    public float TargetY { get; private set; } = -5f;

    public float StartTime { get; private set; }

    public void Init(ChartData chartData, SoundLoader loader)
    {
        chart = chartData;
        soundLoader = loader;
        beatData = chart.beat;

        SecondsPerMeasure = (60f / (float)chart.bpm) * 4f;

        notePools = new List<Queue<GameObject>>();
        for (int i = 0; i < laneCount; i++)
            notePools.Add(new Queue<GameObject>());

        SpawnAllNotes();

        StartTime = Time.time;
    }

    private void SpawnAllNotes()
    {
        for (int lane = 0; lane < beatData.Length; lane++)
        {
            foreach (var beat in beatData[lane])
            {
                GameObject note = GetNoteFromPool(lane);
                float y = SpawnY - ((float)beat.measure * SecondsPerMeasure);
                note.transform.localPosition = new Vector3(lane, y, 0);
                note.SetActive(true);

                var noteScript = note.GetComponent<Note>();
                noteScript.Setup(this, soundLoader, beat.measure, beat.wav, lane);
            }
        }
    }

    private GameObject GetNoteFromPool(int lane)
    {
        if (notePools[lane].Count > 0)
            return notePools[lane].Dequeue();

        return Instantiate(notePrefab, noteParent);
    }
}
