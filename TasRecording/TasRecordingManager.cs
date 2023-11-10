using Celeste.Pico8;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Celeste.Mod.WonderTools.WonderToolsModule;
using Celeste.Mod.WonderTools.Integration;

namespace Celeste.Mod.WonderTools.TasRecording
{
    public class TasRecordingManager
    {
        public static TasRecordingManager Instance { get; private set; }

        private TasRecordingFile room;
        private TasRecordingFile attempt;
        private TasRecordingFile playback;
        private TasRecordingFile flex;
        private TasRecordingFile replayBuffer;
        private TasRecordingFile manual;
        private readonly List<TasRecordingFile> tasRecordingFiles;
        public static bool Paused { get; set; }
        public static bool PausedPrev { get; set; }
        public static bool LevelPaused { get; set; }
        public static bool LevelWasPaused { get; set; }
        public static bool WasPaused { get; set; }
        public static bool StateSaved { get; set; } = false;

        public static TasRecordingState InputState { get; set; }
        public uint WaitingForChange { get; private set; }
        public static TasRecordingState ReplayBufferLoadInputState { get; set; }
        private bool replayBufferActive = false;
        private bool recordingActive = false;
        private string levelSignature = null;
        public enum ButtonInputState
        {
            BUTTON_NOT_PRESSED = 0,
            BUTTON_PRIMARY = 1,
            BUTTON_SECONDARY = 2
        }
        public struct InitTasRecordingOptions
        {
            public bool append;
            public bool copy;
            public bool noFile;
        }
        public TasRecordingManager()
        {
            Instance = this;
            Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), $"TasRecordingManager init");

            room = new TasRecordingFile("room", new InitTasRecordingOptions { append = false });
            flex = new TasRecordingFile("flex", new InitTasRecordingOptions { noFile = true, append = false }); 
            tasRecordingFiles = new List<TasRecordingFile>
            {
                flex,
                room,
            };
        }
        public void OnUpdate()
        {
            if (Engine.Scene is not Level level) { return; }
            /* can't init during constructor for some reason */
            if (null == InputState)
            {
                /* TODO: this sucks */
                try
                {
                    InputState = new TasRecordingState();
                }
                catch
                {
                    return;
                }
            }

            if (WonderToolsModule.Settings.ReplayBuffer && !replayBufferActive)
            {
                WonderLog("Init replay buffer");
                replayBufferActive = true;

            }
            if (!WonderToolsModule.Settings.ReplayBuffer && replayBufferActive)
            {
                WonderLog("Close replay buffer");
                replayBufferActive = false;
            }

            if (WonderToolsModule.Settings.KeyBufferSaveAttemptRecording.Released && WonderToolsModule.Settings.ReplayBuffer)
            {
                WonderLog("Saving Attempt");
                WonderLog($"{WonderToolsModule.Settings.ReplayBuffer} {replayBufferActive}");
                attempt.AppendBreakpoint();
                attempt.AppendLines(flex.Lines);
                SavePlaybackTasRecordingFile(attempt);
                attempt.WriteAndCloseTasRecordingFile();
            }

            if (WonderToolsModule.Settings.KeyStartRecording.Pressed)
            {
                InitTasRecordingOptions options = new();
                InputState = new TasRecordingState();
                manual = new TasRecordingFile($"{DateTime.Now:yyMMddHHmmss}_{levelSignature}", options);
                manual.AppendConsoleCommand();
                manual.AppendBreakpoint();
                recordingActive = true;
            }
            else if (recordingActive && WonderToolsModule.Settings.KeyStopRecording.Pressed)
            {
                manual.SetName(manual.filename + $"_{InputState.frameTotal}f");
                SavePlaybackTasRecordingFile(manual);
                manual.WriteAndCloseTasRecordingFile();
                recordingActive = false;
            }

            if (!recordingActive && !WonderToolsModule.Settings.ReplayBuffer) { return; }

            /* main update */
            UpdatePauseState();
            InputState.Update();
            /* *********** */

            if (InputState.Changed())
            {
                if (0 != WaitingForChange)
                {
                    InputState.frameTotal = WaitingForChange + 1;
                    WaitingForChange = 0;
                }
                if (0 == WaitingForChange) 
                {
                    if (recordingActive) { manual.AppendTasLine(); }
                    flex.AppendTasLine();
                }
                InputState.framesSinceChange = 0;
            }
        }

        private void SavePlaybackTasRecordingFile(TasRecordingFile file)
        {
            playback = new TasRecordingFile("playback", new InitTasRecordingOptions { copy = true }, file.Lines);
            playback.WriteAndCloseTasRecordingFile();
        }

        public void OnSaveState()
        {
            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"Tas save state hook");
            if (WonderToolsModule.Settings.ReplayBuffer)
            {
                StateSaved = true;
                attempt = new TasRecordingFile("attempt", new InitTasRecordingOptions { copy = true }, flex.Lines);
                ReplayBufferLoadInputState = InputState.ShallowClone();
            }
        }

        public void OnLoadState()
        {
            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"Tas load state hook");
            if (WonderToolsModule.Settings.ReplayBuffer && StateSaved)
            {
                flex.ClearTextTasRecordingFile();
                flex.AppendLines(attempt.Lines);
                InputState = ReplayBufferLoadInputState.ShallowClone();
                WaitingForChange = InputState.frameTotal;
            }
        }

        public void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            WonderLog($"Level exit {level.Session.Level}");
            if (recordingActive)
            {
                SavePlaybackTasRecordingFile(manual);
                manual.WriteAndCloseTasRecordingFile();
            }
        }

        public void LevelLoader_OnLoadingThread(Level level)
        {
            flex = new TasRecordingFile("flex", new InitTasRecordingOptions { noFile = true, append = false }); 
            flex.AppendLevelComment(level.Session.Level);

            string[] levelSID = level.Session?.Area.GetSID().Split('/');
            if (0 != levelSID.Length)
            {
                levelSignature = levelSID[^1];
            }
        }

        public void Level_OnLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"Tas level on load hook {level.Session.Level}");
            if (WonderToolsModule.Settings.ReplayBuffer)
            {
                room = new TasRecordingFile("room", new InitTasRecordingOptions { copy = true }, flex.Lines);
            }
            flex.AppendLevelComment(level.Session.Level);
            if (recordingActive) { manual.AppendLevelComment(level.Session.Level); }
        }

        public static void AppendTasInputStr(ref string inputLine, string inputStr)
        {
            if (inputStr.Equals(""))
            {
                return;
            }
            inputLine += $",{inputStr}";
        }

        private static void UpdatePauseState()
        {
            if (Engine.Scene is Level level)
            {
                PausedPrev = Paused;
                Paused = false;
                LevelWasPaused = LevelPaused;
                LevelPaused = level.Paused;
                WasPaused = DynamicData.For(level).Get<bool>("wasPaused");
                if (level.Paused || DynamicData.For(level).Get<bool>("wasPaused"))
                {
                    Paused = true;
                }
            }
        }

        public static string HeaderString()
        {
            return $"# This file was autogenerated by {nameof(WonderToolsModule)} on {DateTime.Now}";
        }

     }
}
