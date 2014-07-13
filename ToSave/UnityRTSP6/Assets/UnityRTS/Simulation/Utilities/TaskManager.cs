using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TaskManager {

    private struct TaskEntry {
        public int Id;
        public int DurationMS;
        public int StartMS;
        public int EndMS { get { return StartMS + DurationMS; } }
        public System.Action Callback;
    }

    private int lastId;
    private int timeMS;

    private List<TaskEntry> entries = new List<TaskEntry>();

    public int NextEvent { get { return entries.Min(t => t.EndMS); } }

    public void Step(int ms) {
        timeMS += ms;
        for (int e = entries.Count - 1; e >= 0; --e) {
            if (entries[e].EndMS < timeMS) Debug.LogError("Item expired!");
            else if (entries[e].EndMS == timeMS) {
                entries[e].Callback();
                entries.RemoveAt(e--);
            }
        }
    }

    public int QueueTask(int duration, System.Action callback) {
        if (duration == 0) Debug.LogError("Duration should be greater than 0");
        var entry = new TaskEntry() {
            Id = lastId++,
            DurationMS = duration,
            StartMS = timeMS,
        };
        entries.Add(entry);
        return entry.Id;
    }

    private int GetIndex(int id) {
        for (int t = 0; t < entries.Count; ++t) if (entries[t].Id == id) return t;
        return -1;
    }
    public void RemoveTask(ref int id) {
        int index = GetIndex(id);
        entries.RemoveAt(index);
    }

}
