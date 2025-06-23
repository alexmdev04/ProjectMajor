using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Major.Startup {
    [CreateAssetMenu(fileName = "New Startup Settings", menuName = "Scriptable Objects/Startup Settings")]
    public class Settings : ScriptableObject {
        public string levelKey = "dev";
        // public SettingsData Export() {
        //     return new() {
        //         levelKey = levelKey
        //     };
        // }
    }

    // public struct SettingsData {
    //     public string levelKey;
    // }
}