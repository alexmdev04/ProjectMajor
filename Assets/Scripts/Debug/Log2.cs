using System;
using UnityEngine;

namespace Major {
    public static class Log2 {
        public static void Debug(string input, string sender = "Undefined Sender", bool onScreen = false) {
            Unity.Logging.Log.Debug("[" + sender + "] " + input);
            if (onScreen) {
                if (uiMessage.instance) {
                    uiMessage.instance.New(input, sender);
                }
            }
        }
        public static void Warning(string input, string sender = "Undefined Sender", bool onScreen = true) {
            Unity.Logging.Log.Warning("[" + sender + "] " + input);
            if (onScreen) {
                if (uiMessage.instance) {
                    uiMessage.instance.New(input, sender);
                }
            }
        }

        public static void Error(string input, string sender = "Undefined Sender", bool onScreen = true) {
            Unity.Logging.Log.Error("[" + sender + "] " + input);
            if (onScreen) {
                if (uiMessage.instance) {
                    uiMessage.instance.New(input, sender);
                }
            }
        }
    }
}