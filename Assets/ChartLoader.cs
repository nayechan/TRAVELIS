using System.Collections;
using System.IO;
using UnityEngine;

public class ChartLoader : MonoBehaviour
{
    [SerializeField] private string fileName;

    public ChartData Chart { get; private set; }

    private IBmsParser parser = new BmsParser();
    public bool IsLoaded { get; private set; } = false;

    public IEnumerator Load()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, fileName);
        string[] lines;

#if UNITY_ANDROID && !UNITY_EDITOR
        var www = UnityEngine.Networking.UnityWebRequest.Get(fullPath);
        yield return www.SendWebRequest();
        lines = www.downloadHandler.text.Split('\n');
#else
        lines = File.ReadAllLines(fullPath);
#endif

        yield return parser.Parse(fullPath, lines, result =>
        {
            Chart = result;
            IsLoaded = true;
            Debug.Log($"Chart parsed: {Chart.title} by {Chart.artist}, BPM: {Chart.bpm}");
        });
    }
}