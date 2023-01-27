using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class StatisticsCollector<T>
{
    public List<Entry> history { get; private set; }
    public long lastTime { get; private set; }
    public long frameNumber { get; private set; }

    public StatisticsCollector()
    {
        history = new List<Entry>();
    }

    public void reset()
    {
        history.Clear();
        frameNumber = 0;
    }

    public void add(long time, T sample)
    {

        long dTime;
        if (history.Count == 0)
            dTime = 0;
        else
            dTime = time - lastTime;

        frameNumber++;
        lastTime = time;

        history.Insert(0, new Entry()
        {
            time = time,
            dTime = dTime,
            frameNum = frameNumber,
            sample = sample
        });
    }


    /// <summary>
    /// function: frame,time,sample => one line of the dump
    /// </summary>
    /// <param name="sampleToString"></param>
    /// <returns></returns>
    public string dump(Func<long, long, T, string> sampleToString )
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in history)
        {
            sb.Append(sampleToString(item.frameNum, item.time, item.sample));
            sb.Append("\n");
        }

        return sb.ToString();
    }

    /// <summary>
    /// default dump : "#[frame] t[time] : sample
    /// </summary>
    /// <returns></returns>
    public string dump()
    {
        return dump((sample) => sample.ToString());
    }

    /// <summary>
    /// one line of the dump : "#[frame] t[time] : sampleToString(sample)
    /// </summary>
    /// <param name="sampleToString"></param>
    /// <returns></returns>
    public string dump(Func<T, string> sampleToString)
    {
        return dump((frame, time, sample) => string.Format("{0:0000}\t{1}\t{2}", frame, time, sampleToString(sample)));
    }

    public struct Entry
    {
        public long time;
        public long dTime;

        public long frameNum;
        public T sample;
    }

}
