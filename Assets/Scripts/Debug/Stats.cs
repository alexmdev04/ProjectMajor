using System;
using System.Text;
using Major.Levels;
using TMPro;
using UnityEngine;

namespace Major.Debug {
    public class Stats : MonoBehaviour {
        private TextMeshProUGUI textBox;
        private void Awake() {
            textBox = GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        private void Update() {
            var text = new StringBuilder("FPS: ").Append((int)(1.0f / Time.deltaTime)).Append("\n");
            if (Player.instance) {
                text.Append("Speed: ").Append(MathF.Round(Player.instance.rb.linearVelocity.magnitude, 4)).Append("\n");                
            }
            if (LevelManager.levelCurrent) {
                text.Append("Level: ").Append(LevelManager.levelCurrent.levelAsset.name).Append("\n");
            }
            textBox.text = text.ToString();
        }
    }
}