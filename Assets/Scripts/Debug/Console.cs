using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Major {

    namespace Debug {
        public class Console : MonoBehaviour {
            private static TMP_InputField inputField;
            private static List<string> previousInputs = new() { string.Empty };
            private static int previousInputsIndex = 0;
            public static Console instance { get; private set; }
            private Selectable returnSelection;
            public static bool cheats { get; private set; }

            private void Awake() {
                inputField = GetComponent<TMP_InputField>();
                inputField.onSubmit.AddListener(input => Execute(input));
                gameObject.SetActive(false);
                instance = this;
            }

            private void OnEnable() {
                if (!GameManager.startupComplete) {
                    return;
                }
                if (EventSystem.current.currentSelectedGameObject) {
                    returnSelection = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
                }
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            private void OnDisable() {
                if (!GameManager.startupComplete) {
                    return;
                }
                if (returnSelection) {
                    returnSelection.Select();
                }
                GameManager.SetCursorVisible(GameManager.isCursorVisible);
                previousInputs[0] = string.Empty;
                inputField.text = string.Empty;
                previousInputsIndex = 0;
            }

            private void Update() {
                inputField.ActivateInputField();
                PreviousInputUpdate();
            }

            public static void Toggle() {
                instance.gameObject.SetActive(instance.gameObject.activeSelf);
            }

            public static void Execute(string input) {
                var inputTrimmed = input.Trim();

                if (inputTrimmed == string.Empty) {
                    return;
                }

                previousInputs.Insert(1, input);
                previousInputsIndex = 0;

                ExecuteWithArgs(inputTrimmed.Split(' '));
            }

            private static void ExecuteWithArgs(string[] args) {
                if (args.Length == 0) {
                    return;
                }

                args[0] = args[0].ToLower();

                if (!cmds.TryGetValue(args[0], out var cmd)) {
                    UnknownCommand(args[0]);
                    return;
                }

                if (cmd.cheatsRequired && !cheats) {
                    Log2.Debug(args[0] + " command requires cheats to be enabled.", "DebugConsole");
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
                public bool cheatsRequired;
                public Command(Action<string[]> cmd, bool cheatsRequired, uint args = 0) {
                    if (cmd == null) {
                        cmd = (args) => { Log2.Debug("Command not implemented", "DebugConsole", true); };
                    }
                    this.cmd = cmd;
                    this.args = args;
                    this.cheatsRequired = cheatsRequired;
                }
                public Command(string cmd) {
                    this.args = 0;
                    this.cheatsRequired = false;
                    this.cmd = (args) => {
                        args[0] = cmd;
                        ExecuteWithArgs(args);
                    };
                }
            }

            public static Dictionary<string, Command> cmds = new() {
                { "cheats",
                    new (args: 0, cheatsRequired: false, cmd: (args) => {
                        bool value = !cheats;
                        if (args.Length > 2) {
                            bool.TryParse(args[1], out value);
                        }

                        if (value) { // Disabling cheats
                            if (cheats) { // Cheats enabled
                                if (Levels.Manager.levelCurrent.key != AssetKeys.Levels.mainMenu) {
                                    Log2.Warning("You must be in the main menu to disable cheats.", "DebugConsole");
                                    return;
                                }
                                else {
                                    Log2.Debug("Cheats Disabled.", "DebugConsole", true);
                                    cheats = false;
                                }
                            }
                        }
                        else { // Enabling cheats
                            if (!cheats) {
                                UI.UI.Popup(
                                    "Warning",
                                    "Enabling cheats will disable data collection." +
                                    "You must disable them while in the main menu.",
                                    new UI.Popup.ButtonConstructor[] {
                                        new UI.Popup.ButtonConstructor() {
                                            text = "Cancel",
                                            textColor = Color.black,
                                            bgColor = Color.white,
                                            onClick = (popup) => { popup.Destroy(); },
                                        },
                                        new UI.Popup.ButtonConstructor() {
                                            text = "Enable",
                                            textColor = Color.black,
                                            bgColor = Color.white,
                                            onClick = (popup) => {
                                                popup.Destroy();
                                                cheats = true;
                                                Log2.Debug("Cheats Enabled.", "DebugConsole", true);
                                            }
                                        }
                                    }
                                );
                            }
                        }
                    } )

                },
                { "sv_cheats", new("cheats") },
                { "fov",
                    new(args: 1, cheatsRequired: false, cmd: (args) => {
                        if (!float.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                        Camera.main.fieldOfView = Mathf.Clamp(value, 1.0f, 179.0f);
                    } )
                },
                { "fieldofview", new("fov") },

                { "level",
                    new(args: 1, cheatsRequired: true, cmd: (args) => {
                        Levels.Manager.LoadLevel(args[1]);
                    } )
                },
                { "map", new("level") },

                { "tp",
                    new(args: 3, cheatsRequired: true, cmd: (args) => {
                        var value = Player.instance.rb.position;
                        if (!TryParseVec3Arg(args, ref value)) {
                            return;
                        }
                        Player.instance.rb.position = value;
                    } )
                },
                { "teleport", new("tp") },

                { "fps",
                    new(args: 0, cheatsRequired: false, cmd: (args) => {
                        if (args.Length < 2) {
                            Execute("stats");
                        }
                        else {
                            if (!int.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                            Application.targetFrameRate = value;
                        }
                    } )
                },
                { "framerate", new("fps") },

                { "vsync",
                    new(args: 1, cheatsRequired: false, cmd: (args) => {
                        if (!int.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                        QualitySettings.vSyncCount = value;
                    } )
                },

                { "sens",
                    new(args: 1, cheatsRequired: false, cmd: (args) => {
                        if (!float.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                        Input.Handler.sensitivity = value;
                    } )
                },
                { "sensitivity", new("sens") },

                { "reload",
                    new(args: 0, cheatsRequired: true, cmd: (args) => {
                        Levels.Manager.RestartHard();
                    } )
                },

                { "restart",
                    new(args: 0, cheatsRequired: false, cmd: (args) => {
                        if (args.Length > 2) {
                            if (args[1] == "soft") {
                                Levels.Manager.RestartSoft();
                                return;
                            }
                            else if (args[1] == "hard") {
                                Levels.Manager.RestartHard();
                                return;
                            }
                        }
                        Levels.Manager.RestartSoft();
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
                    new(args: 1, cheatsRequired: true, cmd: (args) => {
                        UI.UI.SetMenu(args[1]);
                    })
                },

                { "exit",
                    new(args: 0, cheatsRequired: false, cmd: (args) => {
                        GameManager.QuitToDesktop();
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
                    new(args: 0, cheatsRequired: false, cmd: (args) => {
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
                    new(args: 0, cheatsRequired: false, cmd: (args) => {
                    GameManager.instance.OnPlayerKilled();
                    } )
                },

                { "kevin",
                    new(args: 0, cheatsRequired: true, cmd: (args) => {

                    } )
                },

                { "player",
                    new(args: 0, cheatsRequired: true, cmd: (args) => {

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
                    new(args: 0, cheatsRequired: false, cmd: (args) => {
                        Log2.Debug("pong", "DebugConsole", true);
                    } )
                },

                { "noclip",
                    new(args: 0, cheatsRequired: true, cmd: (args) => {
                        if (args.Length < 2) { GameManager.instance.Dbg_ToggleNoclip(); return; }
                        if (!float.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                        GameManager.instance.dbg_noclipSpeed = Mathf.Max(value, 0.0f);
                    } )
                },
                { "stats",
                    new(args: 0, cheatsRequired: false, cmd: (args) => {
                        if (args.Length < 2) {
                            Debug.Stats.isEnabled = !Debug.Stats.isEnabled;
                        }
                        else {
                            switch (args[1]) {
                                case "speed":
                                    Debug.Stats.isSpeedEnabled = !Debug.Stats.isSpeedEnabled;{
                                    break;
                                }
                                case "level":
                                    Debug.Stats.isLevelEnabled = !Debug.Stats.isLevelEnabled;{
                                    break;
                                }
                                default: {
                                    InvalidArgs(args);
                                    break;
                                }
                            }
                        }
                    } )
                },
                { "timescale",
                    new(args: 1, cheatsRequired: true, cmd: (args) => {
                        if (!float.TryParse(args[1], out var value)) { InvalidArgs(args); return; }
                        Time.timeScale = value;
                    } )
                },
                { "popup",
                    new(args: 2, cheatsRequired: false, cmd: (args) => {
                        UI.UI.Popup(args[1], args[2]);
                    } )
                },
                { "fade",
                    new(args: 1, cheatsRequired: false, cmd: (args) => {
                        if (!float.TryParse(args[1], out var fadeIn)) { InvalidArgs(args); return; }
                        float fadeOut = 0.0f;
                        if (args.Length > 2){
                            if (!float.TryParse(args[2], out fadeOut)) { InvalidArgs(args); return; }
                        }
                        UI.UI.Fade(fadeOut, fadeIn);
                    } )
                },
            };
        }
    }
}