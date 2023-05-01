using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LiverDie
{
    public class LeaderboardEntrySpawner : MonoBehaviour
    {
        [SerializeField]
        private int _entries = 0;
        [SerializeField]
        private LeaderboardEntry _entryTemplate = null!;
        [SerializeField]
        private float _entrySpacing = 0;
        [SerializeField]
        private float _entryBaseY = 0;
        private List<LeaderboardEntry> _leaderboardEntries = null;
        public List<LeaderboardEntry> LeaderboardEntries
        {
            get
            {
                if(_leaderboardEntries == null)
                {
                    FillEntries();
                }
                return _leaderboardEntries;
            }
        }
        void Start()
        {
            if(_leaderboardEntries == null)
            {
                Debug.Log("ohno");
            }
        }
        private void FillEntries()
        {
            _leaderboardEntries = new List<LeaderboardEntry>();
            for (int i = 0; i < _entries; i++)
            {
                var entry = Instantiate(_entryTemplate, this.transform);
                var entryTransform = entry.GetComponent<RectTransform>();
                var pos = entryTransform.localPosition;
                pos.y = _entryBaseY + (i * _entrySpacing);
                entryTransform.localPosition = pos;
                _leaderboardEntries.Add(entry);
            }
        }
        public void ClearEntries()
        {
            foreach (LeaderboardEntry entry in _leaderboardEntries) entry.WipeEntry();
        }
        ~LeaderboardEntrySpawner()
        {
            for (int i = 0; i < _leaderboardEntries.Count; i++)
            {
                Destroy(_leaderboardEntries[i].gameObject);
            }
            _leaderboardEntries.Clear();
            _leaderboardEntries = null;
        }
        // Update is called once per frame
    }
}
