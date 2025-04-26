using System.Collections;

public interface IBmsParser
{
    IEnumerator Parse(string _path, string[] lines, System.Action<ChartData> onComplete);
}