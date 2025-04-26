using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BmsParser : IBmsParser
{
    public IEnumerator Parse(string _path, string[] lines, Action<ChartData> onComplete)
    {
        var chart = new ChartData
        {
            path = Path.GetDirectoryName(_path),
            wavPath = new Dictionary<string, string>(),
            beat = new List<BeatData>[129]
        };
        for (int i = 0; i < chart.beat.Length; i++)
            chart.beat[i] = new List<BeatData>();

        int yieldEvery = 50; // Yield every 50 lines
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string rawLine = lines[lineIndex].Trim();
            if (string.IsNullOrEmpty(rawLine) || !rawLine.StartsWith("#")) continue;

            if (rawLine.StartsWith("#TITLE "))
                chart.title = rawLine[7..].Trim();
            else if (rawLine.StartsWith("#ARTIST "))
                chart.artist = rawLine[8..].Trim();
            else if (rawLine.StartsWith("#GENRE "))
                chart.genre = rawLine[7..].Trim();
            else if (rawLine.StartsWith("#BPM "))
                double.TryParse(rawLine[5..].Trim(), out chart.bpm);
            else if (rawLine.StartsWith("#PLAYER "))
                int.TryParse(rawLine[8..].Trim(), out chart.player);
            else if (rawLine.StartsWith("#PLAYLEVEL "))
                int.TryParse(rawLine[11..].Trim(), out chart.playlevel);
            else if (rawLine.StartsWith("#RANK "))
                int.TryParse(rawLine[6..].Trim(), out chart.rank);
            else if (rawLine.StartsWith("#TOTAL "))
                double.TryParse(rawLine[7..].Trim(), out chart.total);
            else if (rawLine.StartsWith("#DIFFICULTY "))
                int.TryParse(rawLine[12..].Trim(), out chart.difficulty);
            else if (rawLine.StartsWith("#WAV") && rawLine.Length >= 7)
            {
                string id = rawLine.Substring(4, 2).ToUpper();
                string wav = rawLine.Substring(7).Trim();
                chart.wavPath[id] = wav;
            }
            else if (rawLine.Length >= 6 && rawLine[6] == ':')
            {
                if (int.TryParse(rawLine.Substring(1, 3), out int measure) &&
                    int.TryParse(rawLine.Substring(4, 2), out int channel))
                {
                    string data = rawLine.Substring(7).Trim();
                    int divisions = data.Length / 2;

                    for (int i = 0; i < divisions; i++)
                    {
                        string wavId = data.Substring(i * 2, 2).ToUpper();
                        if (wavId != "00")
                        {
                            double fractionalMeasure = measure + (double)i / divisions;
                            chart.beat[channel].Add(new BeatData
                            {
                                measure = fractionalMeasure,
                                wav = wavId
                            });
                        }
                    }
                }
            }

            if (lineIndex % yieldEvery == 0)
                yield return null;
        }

        onComplete(chart);
    }
}
