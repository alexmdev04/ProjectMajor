using System;
using System.Text;
using Major.Levels;
using TMPro;
using UnityEngine;

namespace Major.Debug {
    public class Stats : MonoBehaviour {
        public static bool isEnabled;

        public static bool isFpsEnabled = true;
        public static bool isSpeedEnabled;
        public static bool isLevelEnabled;


        private TextMeshProUGUI textBox;

        private void Awake() {
            textBox = GetComponent<TextMeshProUGUI>();
            isEnabled = isSpeedEnabled = isLevelEnabled = 
#if UNITY_EDITOR
            true;
#else
            false;
#endif
        }

        private void Update() {
            textBox.gameObject.SetActive(isEnabled);
            if (!isEnabled) {
                return;
            }

            var text = new StringBuilder("FPS: ").Append((int)Math.Round(1.0f / Time.unscaledDeltaTime, 2)).Append("\n");

            if (isSpeedEnabled && Player.instance) {
                text.Append("Speed: ").Append(MathF.Round(Player.instance.rb.linearVelocity.magnitude, 4)).Append("\n");
            }

            if (isLevelEnabled && LevelManager.levelCurrent) {
                text.Append("Level: ").Append(LevelManager.levelCurrent.levelAsset.name).Append("\n");
                if (LevelManager.levelCurrent.checkpointCurrent) {
                    text.Append("Checkpoint: ").Append(LevelManager.levelCurrent.checkpointCurrent.name).Append("\n");
                }
            }

            if (GameManager.instance.dbg_noclipEnabled) {
                text.Append("Noclip\n");
            }

            if (GameManager.isPaused) {
                text.Append("Paused\n");
            }

            textBox.text = text.ToString();
        }
    }
}