using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Major.Testing {
    public class Tester : MonoBehaviour {
        public static string hwid { get; private set; }
        private bool consent;

        private void Start() {
            hwid = SystemInfo.deviceUniqueIdentifier == SystemInfo.unsupportedIdentifier ?
                SHA1.Create(
                    SystemInfo.deviceModel +
                    SystemInfo.deviceName +
                    SystemInfo.deviceType +
                    SystemInfo.graphicsDeviceID +
                    SystemInfo.operatingSystem +
                    SystemInfo.processorModel +
                    SystemInfo.processorFrequency +
                    SystemInfo.systemMemorySize
                ).ToString() :
                SystemInfo.deviceUniqueIdentifier;
            hwid = hwid[..Math.Min(5, hwid.Length)];
            GameManager.onStartGame += () => {
                consent = true;
                SendStartGame();
            };
            Levels.Manager.onLevelCompleted += SendLevelComplete;
        }

        public void SendStartGame() {
            SendData(
                "Type=StartGame," +
                "Platform=" + SystemInfo.operatingSystem + "," +
                "RefreshRate=" + Screen.currentResolution.refreshRateRatio + "," +
                "CPU=" + SystemInfo.processorModel +
                "GPU=" + SystemInfo.graphicsDeviceName + 
                "RAM=" + SystemInfo.systemMemorySize
            );
        }

        public void SendLevelComplete(Levels.Level level) {
            SendData(
                "Type=LevelComplete," +
                "SessionTime=" + Time.time + "," +
                "TotalTime=" + PlayerPrefs.GetFloat("timeplayed") + "," +
                "LevelKey=" + level.key + "," +
                "LevelTime=" + level.stopwatch.Elapsed.Seconds + "," +
                "LevelRestarts=" + level.restarts + "," +
                "PlayerDeaths=" + level.playerDeaths + "," +
                "KevinDeaths=" + level.kevinDeaths + "," +
                "Cheats=" + (Debug.Console.cheats ? 1 : 0)
            );
        }

        private void OnApplicationQuit() {
            SendQuitGame();
        }

        public void SendQuitGame() {
            SendData(
                "Type=QuitGame," +
                "SessionTime=" + Time.time + "," +
                "TotalTime=" + PlayerPrefs.GetFloat("timeplayed") + "," +
                "GameCompleted=" + PlayerPrefs.GetInt("gamecompleted")
            );
        }

        public void SendData(string data) {
            if (Debug.Console.cheats || !consent) {
                return;
            }
            //GUIUtility.systemCopyBuffer = "hello clipboard " + Time.time;
            data = data.Replace("=", "': '");
            data = data.Replace(",", "', '");
            data = data.Replace("'", "\\\"");
            string url = "https://example.com";
            string input = "{" + $@"""content"": ""``\""ID\"": \""{hwid}\"", \""UnixTime\"": \""{ Extensions.GetUnixTimestamp() }\"", {"{ \\\"" + data + "\\\" }"}``""" + "}";

            UnityEngine.Networking.UnityWebRequest.Post(url, input, "application/json").SendWebRequest();
            Log2.Debug("Sent testing data: \n" + input, "Tester");
        }
    }
}