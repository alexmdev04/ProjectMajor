using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace Major.Debug {
    public class Stats : MonoBehaviour {
        public static bool isEnabled;


        private TextMeshProUGUI textBox;

        private void Awake() {
            textBox = GetComponent<TextMeshProUGUI>();
            isEnabled =
#if UNITY_EDITOR
            true;
#else
            false;
#endif
        }

        private void Update() {
            textBox.enabled = isEnabled;
            if (!isEnabled) {
                return;
            }

            var text = new StringBuilder();

            if (!Tester.instance.enabled || Console.cheats) {
                text.Append("FPS: ").Append((int)Math.Round(1.0f / Time.unscaledDeltaTime, 2)).Append("\n");
            }

            if (Tester.instance.enabled) {
                text.Append("Testing: ").Append(Application.targetFrameRate).Append("\n");
            }

            if (Player.instance) {
                text.Append("Speed: ").Append(MathF.Round(Player.instance.rb.linearVelocity.magnitude, 4)).Append("\n");
            }

            if (Levels.Manager.levelCurrent) {
                text.Append("Level: ").Append(Levels.Manager.levelCurrent.levelAsset.name).Append("\n");
                if (Levels.Manager.levelCurrent.checkpointCurrent) {
                    text.Append("Checkpoint: ").Append(Levels.Manager.levelCurrent.checkpointCurrent.name).Append("\n");
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