using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using UnityEngine;

namespace Major {
    public class Tester : MonoBehaviour {
        public static string hwid { get; private set; }
        public static Tester instance { get; private set; }
        public static bool consent { get; private set; }
        public static float skillRating { get; private set; }
        [SerializeField] private int lowestFps = 10;
        [SerializeField] private float fpsChangeRate = 0.1f;
        [SerializeField] private float fpsSetDelay = 1.0f;
        private Coroutine fpsChangeCoroutine;

        private void Awake() {
            instance = this;
#if UNITY_EDITOR
            enabled = false;
#endif
        }

        private void Start() {
            hwid = Guid.NewGuid().ToString();
            hwid = hwid[..Math.Min(5, hwid.Length)];
            GameManager.onStartGame += () => {
                consent = true;
                SendStartGame();
            };
        }

        private void OnEnable() {
            Levels.Manager.onLevelCompleted += SendLevelComplete;
            fpsChangeCoroutine = StartCoroutine(FpsChangeCoroutine());
        }

        private IEnumerator FpsChangeCoroutine() {
            while (true) {
            Loop:
                if (!GameManager.isInGame) {
                    Application.targetFrameRate = GameManager.baseFrameRateLimit;
                    yield return new WaitForEndOfFrame();
                    goto Loop;
                }
                yield return new WaitForSeconds(fpsSetDelay);
                float t = (Mathf.Sin(Time.time * fpsChangeRate * Math.Max(0.1f, skillRating)) + 1) * 0.5f;
                Application.targetFrameRate = (int)Mathf.Lerp(lowestFps, (float)Screen.currentResolution.refreshRateRatio.value, t);
            }
        }

        public void UpdateSkillRating() {
            if (GameManager.skillRatingData.Count > 0) {
                skillRating = GameManager.skillRatingData.Values.Average();
            }
        }

        private void OnDisable() {
            Application.targetFrameRate = GameManager.baseFrameRateLimit;
            QualitySettings.vSyncCount = GameManager.baseVsyncCount;
            Levels.Manager.onLevelCompleted -= SendLevelComplete;
            if (fpsChangeCoroutine != null) {
                StopCoroutine(fpsChangeCoroutine);
                fpsChangeCoroutine = null;
            }
        }

        public static void SendStartGame() {
            SendData(
                "Type=StartGame," +
                "Platform=" + SystemInfo.operatingSystem + "," +
                "RefreshRate=" + Screen.currentResolution.refreshRateRatio + "," +
                "CPU=" + SystemInfo.processorModel + "," +
                "GPU=" + SystemInfo.graphicsDeviceName + "," +
                "RAM=" + SystemInfo.systemMemorySize
            );
        }

        public static void SendLevelComplete(Levels.Level level) {
            SendData(
                "Type=LevelComplete," +
                "SessionTime=" + Time.time + "," +
                "LevelKey=" + level.key + "," +
                "LevelTime=" + (level.stopwatch.ElapsedMilliseconds / 1000.0f) + "," +
                "LevelRestarts=" + level.restarts + "," +
                "PlayerDeaths=" + level.playerDeaths + "," +
                "KevinDeaths=" + level.kevinDeaths + "," +
                "SkillRating=" + GameManager.skillRatingData[level.key] + "," +
                "Cheats=" + (Debug.Console.cheats ? 1 : 0)
            );
        }

        public static void SendQuitGame() {
            SendData(
                "Type=QuitGame," +
                "SessionTime=" + Time.time + "," +
                "SkillRatingAverage=" + skillRating + "," +
                "GameCompleted=" + GameManager.gameCompleted
            );
        }

        public static void SendGameComplete() {
            SendData(
                "Type=GameComplete," +
                "SessionTime=" + Time.time + "," +
                "SkillRatingAverage=" + skillRating + "," +
                "GameCompleted=" + GameManager.gameCompleted
            );
        }

        public static void SendData(string data) {
            if (!consent) {
                return;
            }

            data = data.Replace("=", "': '");
            data = data.Replace(",", "', '");
            data = data.Replace("'", "\\\"");
            string url = "https://discord.com/api/webhooks/1390122173804576858/vOFHdN2WMt6uHdfkxobOyOmKwHxdEsHfXXQfuOhdWS5JLo0ACtlr2rcyjkiXzwvPj6rL";
            string input = "{" + $@"""username"":""{hwid}"", ""content"": ""``\""ID\"": \""{hwid}\"", \""UnixTime\"": \""{Extensions.GetUnixTimestamp()}\"", {"{ \\\"" + data + "\\\" }"}``""" + "}";

            UnityEngine.Networking.UnityWebRequest.Post(url, input, "application/json").SendWebRequest();
            Log2.Debug("Sent testing data: \n" + input, "Tester");
        }

        public static string GetSurveyLink() {
            return "https://docs.google.com/forms/d/e/1FAIpQLSc_2zzvvrlbFv4KTyZuTLXuNgmG4_uslNtHxFFi3KTV0QIgjg/viewform?usp=pp_url&entry.628381564=" + hwid;
        }
    }
}