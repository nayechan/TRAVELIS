using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public struct ChartData
{
    public int player;
    public string genre;
    public string title;
    public string artist;
    public double bpm;
    public int playlevel;
    public int rank;
    public int difficulty;
    public double total;

    public Dictionary<string, string> wavPath;
    public List<string>[] beat;
}

public class ChartParser : MonoBehaviour
{
    [SerializeField] private string path;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        path = Path.Join(Application.streamingAssetsPath, path);
        
        
        
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
