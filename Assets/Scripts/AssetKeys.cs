using Major.Levels;
using UnityEngine.Android;

namespace Major.AssetKeys {
    // Contains keys to addressable LevelAsset data
    public static class Levels {
        public const string intro = "intro";
        public const string level1 = "level1";
    }

    // Contains keys to addressable scenes filtered by level
    namespace Scenes {
        public static class intro {
            public const string home = "intro_home";
        }
    }

    // Contains keys to addressable prefabs
    public static class Prefabs {
        public const string testCube = "testCube";
    }

    // Contains the labels used to filter addressable assets
    public static class Labels {
        public const string level = "lbl_level";
        public const string prefab = "lbl_prefab";
        public const string scene = "lbl_scene";
        public const string asset = "lbl_asset";
    }
}