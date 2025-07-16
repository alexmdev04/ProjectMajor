using System;
using System.Collections.Generic;
using Major.Debug;
using Major.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Major {

    namespace UI {
        public class DebugConsole : MonoBehaviour {
            private static TMP_InputField inputField;
            public static List<string> previousInputs = new() { string.Empty };
            public static int previousInputsIndex = 0;

            private void Awake() {
                inputField = GetComponent<TMP_InputField>();
                inputField.onSubmit.AddListener(input => Execute(input));
                gameObject.SetActive(false);
            }

            private void OnEnable() {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            private void OnDisable() {
                if (!GameManager.startupComplete) {
                    return;
                }
                GameManager.instance.SetCursorVisible(GameManager.isCursorVisible);
                previousInputs[0] = string.Empty;
                inputField.text = string.Empty;
                previousInputsIndex = 0;
            }

            private void Update() {
                inputField.ActivateInputField();
                PreviousInputUpdate();
            }

            public static void Execute(string input) {
                var inputTrimmed = input.Trim();

                if (inputTrimmed == string.Empty) {
                    return;
                }

                previousInputs.Insert(1, input);
                previousInputsIndex = 0;

                ExecuteWithArgs(inputTrimmed.ToLower().Split(' '));
            }

            private static void ExecuteWithArgs(string[] args) {
                if (args.Length == 0) {
                    return;
                }

                if (!cmds.TryGetValue(args[0], out var cmd)) {
                    UnknownCommand(args[0]);
                    return;
                }

                if (args.Length < cmd.args) {
                    InvalidArgCount(args, cmd.args);
                    return;
                }

                cmd.cmd(args);

                inputField.text = "";
            }

            private static bool TryParseVec3Arg(string[] args, ref Vector3 result) {
                if (args.Length < 4) {
                    InvalidArgCount(args, 3);
                    return false;
                }

                for (int i = 1; i < args.Length; i++) {
                    var arg = args[i];
                    var isDelta = arg.StartsWith('~');

                    if (!float.TryParse(isDelta ? arg[1..] : arg, out var value)) {
                        InvalidArgs(args);
                        return false;
                    }

                    result[i - 1] = isDelta ? result[i - 1] + value : value;
                }
                return true;
            }

            private void PreviousInputUpdate() {
                if (previousInputsIndex == 0) { previousInputs[0] = inputField.text; }
                if (previousInputs.Count > 1) {
                    if (Keyboard.current.upArrowKey.wasPressedThisFrame && previousInputsIndex < previousInputs.Count - 1) {
                        previousInputsIndex++;
                        inputField.text = previousInputs[previousInputsIndex];
                        inputField.stringPosition = inputField.text.Length;
                    }
                    if (Keyboard.current.downArrowKey.wasPressedThisFrame && previousInputsIndex > 0) {
                        previousInputsIndex--;
                        inputField.text = previousInputs[previousInputsIndex];
                        inputField.stringPosition = inputField.text.Length;
                    }
                }
            }

            private static void UnknownCommand(string cmd) {
                Log2.Debug("Unknown Command '" + cmd + "'", "DebugConsole", true);
            }

            private static void InvalidArgCount(string[] args, uint argsNeeded) {
                Log2.Debug("Invalid argument count (" + (args.Length - 1) + ") for '" + args[0] + "' (" + argsNeeded + ")", "DebugConsole", true);
            }

            private static void InvalidArgs(string[] args) {
                Log2.Debug("Invalid arguments for '" + args[0] + "'", "DebugConsole", true);
            }

            public struct Command {
                public Action<string[]> cmd;
                public uint args;
                public Command(Action<string[]> cmd, uint args = 0) {
                    if (cmd == null) {
                        cmd = (args) => { Log2.Debug("Command not implemented", "DebugConsole", true); };
                    }
                    this.cmd = cmd;
                    this.args = args;
                }
                public Command(string cmd) {
                    this.args = 0;
                    this.cmd = (args) => {
                        args[0] = cmd;
                        ExecuteWithArgs(args);
                    };
                }
            }

            public static Dictionary<string, Command> cmds = new() {
                { "fov",
                    new(args: 1, cmd: (args) => {
                        if (!float.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                        Camera.main.fieldOfView = Mathf.Clamp(value, 1.0f, 179.0f);
                    } )
                },
                { "fieldofview", new("fov") },

                { "level",
                    new(args: 1, cmd: (args) => {
                        LevelManager.LoadLevel(args[1]);
                    } )
                },
                { "map", new("level") },

                { "tp",
                    new(args: 3, cmd: (args) => {
                        var value = Player.instance.rb.position;
                        if (!TryParseVec3Arg(args, ref value)) {
                            return;
                        }
                        Player.instance.rb.position = value;
                    } )
                },
                { "teleport", new("tp") },

                { "fps",
                    new(args: 0, cmd: (args) => {
                        if (args.Length < 2) {
                            // show hide FPS
                        }
                        else {
                            if (!int.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                            Application.targetFrameRate = value;
                        }
                    } )
                },
                { "framerate", new("fps") },

                { "vsync",
                    new(args: 1, cmd: (args) => {
                        if (!int.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                        QualitySettings.vSyncCount = value;
                    } )
                },

                { "sens",
                    new(args: 1, cmd: (args) => {
                        if (!float.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                        Input.Handler.instance.sensitivity = value;
                    } )
                },
                { "sensitivity", new("sens") },

                { "reload",
                    new(args: 0, cmd: (args) => {
                        LevelManager.RestartHard();
                    } )
                },

                { "restart",
                    new(args: 0, cmd: (args) => {
                        if (args.Length > 2) {
                            if (args[0] == "soft") {
                                LevelManager.RestartSoft();
                                return;
                            }
                            else if (args[0] == "hard") {
                                LevelManager.RestartHard();
                                return;
                            }
                        }
                        LevelManager.RestartSoft();
                    } )
                },
                { "reset", new("restart") },

                { "checkpoint",
                    new(null)
                },

                { "post",
                    new(null)
                },

                { "menu",
                    new(null)
                },

                { "exit",
                    new(args: 0, cmd: (args) => {
                        #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                        #endif
                        Application.Quit();
                    } )
                },
                { "quit", new("exit") },


                // { "say", cmdDict["echo"] },
                // { "msg", cmdDict["echo"] },
                // { "echo",
                //     new(args: 1, cmd: (args) => {
                //         // Log2.Debug(input.TrimStart()[(args[0].Length + 1)..], "DebugConsole", true);
                //     } )
                // },

                { "kill",
                    new(args: 0, cmd: (args) => {
                        if (args.Length > 1) {
                            if (args[1] == "kevin") {
                                GameManager.instance.OnKevinKilled();
                                return;
                            }
                            else if (args[1] == "player") {
                                GameManager.instance.OnPlayerKilled();
                                return;
                            }
                            else {
                                InvalidArgs(args);
                                return;
                            }
                        }
                        GameManager.instance.OnPlayerKilled();
                    } )
                },

                { "suicide",
                    new(args: 0, cmd: (args) => {
                    GameManager.instance.OnPlayerKilled();
                    } )
                },

                { "kevin",
                    new(args: 0, cmd: (args) => {

                    } )
                },

                { "player",
                    new(args: 0, cmd: (args) => {

                    } )
                },

                // { "walkvel",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "sprintvel",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "crouchvel",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "itemtravelspeed",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "jumpforce",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "accel",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "accelair",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "interactdist",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "autodropdist",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "autodropspeed",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "autodropblocked",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "decel",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "friction",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "mass",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "soft",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "hard",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "move",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "look",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "freeze",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "gravity",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "dropitem",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "scale",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                // { "gravityscale",
                //     new(args: 0, cmd: (args) => {

                //     } )
                // },

                { "ping",
                    new(args: 0, cmd: (args) => {
                        Log2.Debug("pong", "DebugConsole", true);
                    } )
                },

                { "noclip",
                    new(args: 0, cmd: (args) => {
                        if (args.Length < 2) { GameManager.instance.ToggleNoclip(); return; }
                        if (!float.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                        GameManager.instance.dbg_noclipSpeed = Mathf.Max(value, 0.0f);
                    } )
                },
                { "stats",
                    new(args: 0, cmd: (args) => {
                        if (args.Length < 2) {
                            Stats.isEnabled = !Stats.isEnabled;
                        }
                        else {
                            switch (args[1]) {
                                case "fps":
                                    Stats.isFpsEnabled = !Stats.isFpsEnabled;{
                                    break;
                                }
                                case "speed":
                                    Stats.isSpeedEnabled = !Stats.isSpeedEnabled;{
                                    break;
                                }
                                case "level":
                                    Stats.isLevelEnabled = !Stats.isLevelEnabled;{
                                    break;
                                }
                                default: {
                                    InvalidArgs(args);
                                    break;
                                }
                            }
                        }
                    } )
                }
            };
        }
    }
}