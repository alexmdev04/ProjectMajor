using System.Collections.Generic;
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
                inputField.onSubmit.AddListener(input => Command(input));
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

            public static void Command(string input) {
                previousInputs.Insert(1, input);
                previousInputsIndex = 0;

                if (input == string.Empty) {
                    return;
                }

                string[] args = input.Trim().ToLower().Split(' ');

                if (args.Length == 0) {
                    return;
                }

                if (!cmdDict.TryGetValue(args[0], out var cmdIndex)) {
                    UnknownCommand(args[0]);
                    return;
                }

                switch (cmdIndex) {
                    case int_Fov: {
                            if (args.Length < 2) { InvalidArgCount(args[0]); break; }
                            if (!float.TryParse(args[1], out var value)) { InvalidArgs(args[0]); break; }
                            Camera.main.fieldOfView = Mathf.Clamp(value, 1.0f, 179.0f);
                            break;
                        }
                    case int_Level: {
                            if (args.Length < 2) { InvalidArgCount(args[0]); break; }
                            LevelManager.LoadLevel(args[1]);
                            break;
                        }
                    case int_Tp:
                    case int_Teleport: {
                            if (args.Length < 4) { InvalidArgCount(args[0]); break; }
                            if (!float.TryParse(args[1], out var value1)) { InvalidArgs(args[0]); break; }
                            if (!float.TryParse(args[2], out var value2)) { InvalidArgs(args[0]); break; }
                            if (!float.TryParse(args[3], out var value3)) { InvalidArgs(args[0]); break; }
                            Player.instance.rb.position = new Vector3(value1, value2, value3);
                            break;
                        }
                    case int_Fps: {
                            if (args.Length < 2) {
                                // show hide FPS
                            }
                            else {
                                if (!int.TryParse(args[1], out var value)) { InvalidArgs(args[0]); break; }
                                Application.targetFrameRate = value;
                            }
                            break;
                        }
                    case int_Vsync: {
                            if (args.Length < 2) { InvalidArgCount(args[0]); break; }
                            if (!int.TryParse(args[1], out var value)) { InvalidArgs(args[0]); break; }
                            QualitySettings.vSyncCount = value;
                            break;
                        }
                    case int_Sens:
                    case int_Sensitivity: {
                            if (args.Length < 2) { InvalidArgCount(args[0]); break; }
                            if (!float.TryParse(args[1], out var value)) { InvalidArgs(args[0]); break; }
                            Input.Handler.instance.sensitivity = value;
                            break;
                        }
                    case int_Restart:
                    case int_Reload:
                    case int_Reset: {
                            if (args.Length > 2) { }
                            UnknownCommand(args[0]);
                            break;
                        }
                    case int_Checkpoint: {
                            UnknownCommand(args[0]);
                            break;
                        }
                    case int_Post: {
                            UnknownCommand(args[0]);
                            break;
                        }
                    case int_Menu: {
                            UnknownCommand(args[0]);
                            break;
                        }
                    case int_Exit:
                    case int_Quit: {
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#endif
                            Application.Quit();
                            break;
                        }
                    case int_Echo:
                    case int_Say:
                    case int_Msg: {
                            if (args.Length < 2) { InvalidArgCount(args[0]); break; }
                            Log2.Debug(input.TrimStart()[(args[0].Length + 1)..], "DebugConsole", true);
                            break;
                        }
                    case int_Kill: {
                            if (args.Length > 1) {
                                if (args[1] == str_Kevin) {
                                    GameManager.instance.OnKevinKilled();
                                    break;
                                }
                                else if (args[1] == str_Player) {
                                    GameManager.instance.OnPlayerKilled();
                                    break;
                                }
                                else {
                                    InvalidArgs(args[0]);
                                    break;
                                }
                            }
                            GameManager.instance.OnPlayerKilled();
                            break;
                        }
                    case int_Suicide: {
                            GameManager.instance.OnPlayerKilled();
                            break;
                        }
                    case int_Ping: {
                            Log2.Debug("pong", "DebugConsole", true);
                            break;
                        }
                    default: {
                            UnknownCommand(args[0]);
                            break;
                        }

                }

                inputField.text = "";
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

            private static void InvalidArgCount(string cmd) {
                Log2.Debug("Invalid argument count for '" + cmd + "'", "DebugConsole", true);
            }

            private static void InvalidArgs(string cmd) {
                Log2.Debug("Invalid arguments for '" + cmd + "'", "DebugConsole", true);
            }

            public static Dictionary<string, uint> cmdDict = new() {
                { str_Ping, int_Ping },
                { str_Fov, int_Fov },
                { str_Level, int_Level },
                { str_Tp, int_Tp },
                { str_Teleport, int_Teleport },
                { str_Fps, int_Fps },
                { str_Vsync, int_Vsync },
                { str_Sens, int_Sens },
                { str_Sensitivity, int_Sensitivity },
                { str_Restart, int_Restart },
                { str_Reload, int_Reload },
                { str_Reset, int_Reset },
                { str_Checkpoint, int_Checkpoint },
                { str_Post, int_Post },
                { str_Menu, int_Menu },
                { str_Exit, int_Exit },
                { str_Quit, int_Quit },
                { str_Echo, int_Echo },
                { str_Say, int_Say },
                { str_Msg, int_Msg },
                { str_Kill, int_Kill },
                { str_Suicide, int_Suicide },
                { str_Kevin, int_Kevin },
                { str_Player, int_Player },
                { str_Walkvel, int_Walkvel },
                { str_Sprintvel, int_Sprintvel },
                { str_Crouchvel, int_Crouchvel },
                { str_Itemtravelspeed, int_Itemtravelspeed },
                { str_Jumpforce, int_Jumpforce },
                { str_Accel, int_Accel },
                { str_Accelair, int_Accelair },
                { str_Interactdist, int_Interactdist },
                { str_Autodropdist, int_Autodropdist },
                { str_Autodropspeed, int_Autodropspeed },
                { str_Autodropblocked, int_Autodropblocked },
                { str_Decel, int_Decel },
                { str_Friction, int_Friction },
                { str_Mass, int_Mass },
                { str_Soft, int_Soft },
                { str_Hard, int_Hard },
                { str_Move, int_Move },
                { str_Look, int_Look },
                { str_Freeze, int_Freeze },
                { str_Gravity, int_Gravity },
                { str_Dropitem, int_Dropitem },
                { str_Scale, int_Scale },
                { str_GravityScale, int_GravityScale },
            };

            public const string
                str_Ping = "ping",
                str_Fov = "fov",
                str_Level = "level",
                str_Tp = "tp",
                str_Teleport = "teleport",
                str_Fps = "fps",
                str_Vsync = "vsync",
                str_Sens = "sens",
                str_Sensitivity = "sensitivity",
                str_Restart = "restart",
                str_Reload = "reload",
                str_Reset = "reset",
                str_Checkpoint = "checkpoint",
                str_Post = "post",
                str_Menu = "menu",
                str_Quit = "quit",
                str_Exit = "exit",
                str_Echo = "echo",
                str_Say = "say",
                str_Msg = "msg",
                str_Kill = "kill",
                str_Suicide = "suicide",
                str_Kevin = "kevin",
                str_Player = "player",
                str_Walkvel = "walkvel",
                str_Sprintvel = "sprintvel",
                str_Crouchvel = "crouchvel",
                str_Itemtravelspeed = "itemtravelspeed",
                str_Jumpforce = "jumpforce",
                str_Accel = "accel",
                str_Accelair = "accelair",
                str_Interactdist = "interactdist",
                str_Autodropdist = "autodropdist",
                str_Autodropspeed = "autodropspeed",
                str_Autodropblocked = "autodropblocked",
                str_Decel = "decel",
                str_Friction = "friction",
                str_Mass = "mass",
                str_Soft = "soft",
                str_Hard = "hard",
                str_Move = "move",
                str_Look = "look",
                str_Freeze = "freeze",
                str_Gravity = "gravity",
                str_Dropitem = "dropitem",
                str_Scale = "scale",
                str_GravityScale = "gravityscale";

            public const int
                int_Fov = 0,
                int_Level = 1,
                int_Tp = 2,
                int_Teleport = 3,
                int_Fps = 4,
                int_Vsync = 5,
                int_Sens = 6,
                int_Sensitivity = 7,
                int_Restart = 8,
                int_Reload = 9,
                int_Reset = 10,
                int_Checkpoint = 11,
                int_Post = 12,
                int_Menu = 13,
                int_Quit = 14,
                int_Exit = 15,
                int_Echo = 16,
                int_Say = 17,
                int_Msg = 18,
                int_Kill = 19,
                int_Suicide = 20,
                int_Kevin = 21,
                int_Player = 22,
                int_Walkvel = 23,
                int_Sprintvel = 24,
                int_Crouchvel = 25,
                int_Itemtravelspeed = 26,
                int_Jumpforce = 27,
                int_Accel = 28,
                int_Accelair = 29,
                int_Interactdist = 30,
                int_Autodropdist = 31,
                int_Autodropspeed = 32,
                int_Autodropblocked = 33,
                int_Decel = 34,
                int_Friction = 35,
                int_Mass = 36,
                int_Soft = 37,
                int_Hard = 38,
                int_Move = 39,
                int_Look = 40,
                int_Freeze = 41,
                int_Gravity = 42,
                int_Dropitem = 43,
                int_Scale = 44,
                int_GravityScale = 45,
                int_Ping = 46;
        }
    }
}