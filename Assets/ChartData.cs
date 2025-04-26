using System;
using System.Collections.Generic;

[Serializable]
public struct BeatData
{
    public double measure;
    public string wav;
}

[Serializable]
public struct ChartData
{
    public string path;
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
    public List<BeatData>[] beat;
}