using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

namespace LiverDie
{
    public class LiverboardController : MonoBehaviour
    {
        private static string _guid;
        public static string SessionID
        {
            get
            {
                if (_guid == null)
                {
                    _guid = System.Guid.NewGuid().ToString();
                }
                return _guid;
            }
        }
        private static string? _name;
        [SerializeField]
        private string _liverboardURL = null!;
        [SerializeField]
        private LeaderboardEntrySpawner _leaderboardEntrySpawner = null!;
        [SerializeField]
        private TextMeshProUGUI _loadingObject = null!;
        [SerializeField]
        private int _offsetFromRank = 0;
        [SerializeField]
        private GameObject _namePrompt;
        [SerializeField]
        private TMP_InputField _nameTextField;

        [Serializable]
        private struct SendData
        {
            public string uid;
            public int livers;
            public int time;
            public string name;
            public SendData(string uid, string name, int livers, int time)
            {
                this.uid = uid;
                this.name = name;
                this.livers = livers;
                this.time = time;
            }

        }
        [Serializable]
        private struct EntryData
        {
            public int rank;
            public string uid;
            public string name;
            public int livers;
            public int time;
            public int offset;
        }

        private void Start()
        {
            _leaderboardEntrySpawner.gameObject.SetActive(false);
            _loadingObject.gameObject.SetActive(false);
        }

        public void SetName()
        {
            _name = _nameTextField.text;
        }

        public async UniTask SendScore(int time, int livers, string uid, bool loadLeaderboard = true)
        {
            _loadingObject.gameObject.SetActive(true);
            _loadingObject.text = "LOADING...";

            _leaderboardEntrySpawner.gameObject.SetActive(false);

            if (_name == null)
            {
                _namePrompt.SetActive(true);
                await UniTask.WaitUntil(() => _name != null);
                _namePrompt.SetActive(false);
            }

            if (_name is null)
            {
                Debug.LogWarning("No name provided.");
                return;
            }

            SendData sendData = new(uid, _name, livers, time);
            var sendPayload = JsonConvert.SerializeObject(sendData);

            UploadHandlerRaw upload = new(Encoding.UTF8.GetBytes(sendPayload));
            var download = new DownloadHandlerBuffer();

            using UnityWebRequest request = new(_liverboardURL, "POST", download, upload);
            request.SetRequestHeader( "Content-Type", "application/json");
            request.timeout = 15;

            UnityWebRequest? response = null;
            try
            {
                response = await request.SendWebRequest();

            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                _loadingObject.text = "Failed to connect!";
                return;
            }

            if (response is null)
            {
                Debug.LogWarning("Failed to acquire response");
                return;
            }

            if (!loadLeaderboard)
                return;

            var ourEntry = JsonConvert.DeserializeObject<EntryData>(request.downloadHandler.text);

            var data = await UnityWebRequest.Get($"{_liverboardURL}/?limit={_leaderboardEntrySpawner.LeaderboardEntries.Count}&offset={Math.Clamp(ourEntry.offset - _offsetFromRank, 0, int.MaxValue)}").SendWebRequest();
            if (data.result is not UnityWebRequest.Result.Success)
            {
                _loadingObject.text = "Failed to get scores!";
                return;
            }

            var entries = JsonConvert.DeserializeObject<List<EntryData>>(data.downloadHandler.text)!;
            for (int i = 0; i < _leaderboardEntrySpawner.LeaderboardEntries.Count; i++)
            {
                if (i >= entries.Count)
                {
                    _leaderboardEntrySpawner.LeaderboardEntries[i].WipeEntry();
                }
                else
                {
                    var entry = entries[i];
                    _leaderboardEntrySpawner.LeaderboardEntries[i].FillEntry(entry.rank, entry.name, entry.livers,
                        entry.time, entry.uid == ourEntry.uid);
                }
            }

            _leaderboardEntrySpawner.gameObject.SetActive(true);
            _loadingObject.gameObject.SetActive(false);

            /*try
            {
                _loadingObject.gameObject.SetActive(true);
                _loadingObject.text = "LOADING...";
                _leaderboardEntrySpawner.gameObject.SetActive(false);
                if (LiverboardController._name == null) _namePrompt.SetActive(true);
                await UniTask.WaitUntil(() => LiverboardController._name != null);
                _namePrompt.SetActive(false);
                SendData sendData = new SendData(uid, LiverboardController._name, livers, time);
                string val = JsonConvert.SerializeObject(sendData);
                Debug.Log(val);

                var cts = new CancellationTokenSource();
                cts.CancelAfterSlim(TimeSpan.FromSeconds(15)); // 5sec timeout.

                var req = new UnityWebRequest();
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(val));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.method = "POST";
                req.uri = new Uri(_liverboardURL);
                req.SetRequestHeader("Content-Type", "application/json");
                UnityWebRequest postRes = null;
                try
                {
                    postRes = await req.SendWebRequest().WithCancellation(cts.Token);

                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        _loadingObject.text = "Failed to connect!";
                        return;
                    }
                }

                if (postRes.result != UnityWebRequest.Result.Success)
                {
                    _loadingObject.text = "Failed to send score!";
                    return;
                }

                if (!loadLeaderboard) return;
                EntryData ourEntry = JsonUtility.FromJson<EntryData>(postRes.downloadHandler.text);
                List<EntryData> entries = new List<EntryData>();
                var data = await UnityWebRequest
                    .Get(
                        $"{_liverboardURL}/?limit={_leaderboardEntrySpawner.LeaderboardEntries.Count}&offset={Math.Clamp(ourEntry.offset - _offsetFromRank, 0, int.MaxValue)}")
                    .SendWebRequest();
                if (data.result != UnityWebRequest.Result.Success)
                {
                    _loadingObject.text = "Failed to get scores!";
                    return;
                }

                entries = JsonConvert.DeserializeObject<List<EntryData>>(data.downloadHandler.text);
                for (int i = 0; i < _leaderboardEntrySpawner.LeaderboardEntries.Count; i++)
                {
                    if (i >= entries.Count)
                    {
                        _leaderboardEntrySpawner.LeaderboardEntries[i].WipeEntry();
                    }
                    else
                    {
                        EntryData entry = entries[i];
                        _leaderboardEntrySpawner.LeaderboardEntries[i].FillEntry(entry.rank, entry.name, entry.livers,
                            entry.time, entry.uid == ourEntry.uid);
                    }
                }

                _leaderboardEntrySpawner.gameObject.SetActive(true);
                _loadingObject.gameObject.SetActive(false);
            }
            catch
            {
            }*/
        }
    }
}
