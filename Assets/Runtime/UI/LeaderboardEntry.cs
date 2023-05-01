using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LiverDie
{
    public class LeaderboardEntry : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        private TextMeshProUGUI _placement = null!;
        [SerializeField]
        private TextMeshProUGUI _playerName = null!;
        [SerializeField]
        private TextMeshProUGUI _liverCount = null!;
        [SerializeField]
        private TextMeshProUGUI _timeCount = null!;
        [SerializeField]
        private Color _playerColor = Color.yellow;
        [SerializeField]
        private Color _normalColor = Color.white;
        public void FillEntry(int placement, string username, int livers, int time, bool isPlayer)
        {
            Color col = isPlayer ? _playerColor : _normalColor;
            _placement.color = col;
            _playerName.color = col;
            _liverCount.color = col;
            _placement.text = $"{placement}.";
            _playerName.text = username;
            _timeCount.text = $"{((float)time/1000f).ToString("0.0")}s";
            _liverCount.text = $"{livers}";
        }
        public void WipeEntry()
        {
            _placement.text = "";
            _playerName.text = "";
            _timeCount.text = "";
            _liverCount.text = "";
        }
    }
}
