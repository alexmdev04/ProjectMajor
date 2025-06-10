using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Logging;

public static class Extensions {
    // script adding addressables
    // https://github.com/UnioGame/UniModules/blob/20da8c139f1b2d63ecc98bfcf8368e0aea6d2b55/UniGame.AddressableTools/Editor/Extensions/AddressableHelper.cs

    public static Task Debug(this Task task, Type callerType, string taskName = "Unnamed Task", bool logged = false, bool timed = false, Action onTaskComplete = null) {
        return Debug(task, callerType.Name, taskName, logged, timed, onTaskComplete);
    }

    public static Task<T> Debug<T>(this Task<T> task, Type callerType, string taskName = "Unnamed Task", bool logged = false, bool timed = false, Action<T> onTaskComplete = null) {
        return Debug<T>(task, callerType.FullName, taskName, logged, timed, onTaskComplete);
    }

    public static async Task Debug(this Task task, string caller, string taskName = "Unnamed Task", bool logged = false, bool timed = false, Action onTaskComplete = null) {
        if (timed) {
            Stopwatch timer = Stopwatch.StartNew();
            onTaskComplete += () => {
                timer.Stop();
                Log.Debug("[" + caller + "] " + taskName + " took " + timer.Elapsed.ToString());
            };
        }

        if (logged) {
            Log.Debug("[" + caller + "] " + "Beginning " + taskName + "...");
            onTaskComplete += () => { Log.Debug("[" + caller + "] " + "Finished " + taskName + "."); };
        }

        await task;
        onTaskComplete?.Invoke();
    }

    public static async Task<T> Debug<T>(this Task<T> task, string caller, string taskName = "Unnamed Task", bool logged = false, bool timed = false, Action<T> onTaskComplete = null) {
        if (timed) {
            Stopwatch timer = Stopwatch.StartNew();
            onTaskComplete += (t) => {
                timer.Stop();
                Log.Debug("[" + caller + "] " + taskName + " took " + timer.Elapsed.ToString());
            };
        }

        if (logged) {
            Log.Debug("[" + caller + "] " + "Beginning " + taskName + "...");
            onTaskComplete += (t) => { Log.Debug("[" + caller + "] " + "Finished " + taskName + "."); };
        }

        var result = await task;
        onTaskComplete?.Invoke(task.Result);
        return result;
    }
}

public static class MathExt {
    /// <summary>
    /// Returns true if a is within plus or minus the range of b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static bool Within(this float a, float b, float range) {
        return Mathf.Abs(b - a) < range;
    }

    public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max) {
        return new Vector3(
            Mathf.Clamp(value.x, min.x, max.x),
            Mathf.Clamp(value.y, min.y, max.y),
            Mathf.Clamp(value.z, min.z, max.z)
        );
    }
}

