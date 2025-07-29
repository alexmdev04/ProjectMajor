using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Major.Startup {
    [CreateAssetMenu(fileName = "New Startup Settings", menuName = "Scriptable Objects/Startup Settings")]
    public class Settings : ScriptableObject {
        public int baseFpsLimit = 0;
        public string firstLevel = "slotting";
        public string finalLevel = "pit_jump";
        public bool startupMessage = true;
        public string startupMessageTitle = "Notice";
        public string startupMessageBody =
            "This game collects data about your gameplay and your hardware.\n" +
            "This data is used only by the developer for research purposes.\n" +
            "Visit xae0.itch.io/quadrasylum for more details.";
    }
}