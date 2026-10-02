using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// The player's own top runs, saved on this device with PlayerPrefs. On a web build that is the
    /// browser's storage, so it works on GitHub Pages with no server; each browser keeps its own list.
    /// Only completed runs are recorded (failed runs score 0). Ranked by score, then by time left.
    /// </summary>
    public static class BestScores
    {
        public const int MaxEntries = 5;

        [Serializable]
        public class Entry
        {
            public float score;
            public float secondsLeft;
            public int passengers;
            public string date;
        }

        [Serializable]
        class Book
        {
            public List<Entry> entries = new List<Entry>();
        }

        static string Key(string routeName) => "FarePlay.BestScores." + (string.IsNullOrEmpty(routeName) ? "Route" : routeName);

        public static List<Entry> Load(string routeName)
        {
            string json = PlayerPrefs.GetString(Key(routeName), "");
            if (string.IsNullOrEmpty(json)) return new List<Entry>();
            try
            {
                Book book = JsonUtility.FromJson<Book>(json);
                return book != null && book.entries != null ? book.entries : new List<Entry>();
            }
            catch (ArgumentException)
            {
                return new List<Entry>();   // corrupted save: start fresh rather than crash
            }
        }

        /// <summary>Adds a finished run. Returns its rank (0 = new best), or -1 if it didn't make the list.</summary>
        public static int Record(string routeName, RunResult result)
        {
            List<Entry> entries = Load(routeName);
            var entry = new Entry
            {
                score = result.Total,
                secondsLeft = result.SecondsRemaining,
                passengers = result.PassengersDelivered,
                date = DateTime.Now.ToString("MMM d")
            };
            entries.Add(entry);
            entries.Sort((a, b) => a.score != b.score ? b.score.CompareTo(a.score) : b.secondsLeft.CompareTo(a.secondsLeft));

            int rank = entries.IndexOf(entry);
            if (entries.Count > MaxEntries) entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
            if (rank >= MaxEntries) rank = -1;

            PlayerPrefs.SetString(Key(routeName), JsonUtility.ToJson(new Book { entries = entries }));
            PlayerPrefs.Save();   // on the web this writes to browser storage right away
            return rank;
        }
    }
}
