using FMOD;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Celeste.Mod.WonderTools.WonderToolsModule;

namespace Celeste.Mod.WonderTools.TasRecording
{
    public class TasRecordingManager
    {
        public static TasRecordingManager Instance { get; private set; }

        private readonly List<String> lines = new();
        private readonly TasRecordingFile room;
        private readonly TasRecordingFile attempt;
        private readonly TasRecordingFile playback;
        private readonly TasRecordingFile flex;
        private readonly List<TasRecordingFile> tasRecordingFiles;
        public static bool Paused { get; set; }
        public static bool PausedPrev { get; set; }
        public static bool LevelPaused { get; set; }
        public static bool LevelWasPaused { get; set; }
        public static bool WasPaused { get; set; }

        public static TasRecordingState InputState { get; set; }
        public enum ButtonInputState
        {
            BUTTON_NOT_PRESSED = 0,
            BUTTON_PRIMARY = 1,
            BUTTON_SECONDARY = 2
        }
        public struct InitTasRecordingOptions
        {
            public bool fileClear;
            public bool fileRestart;
            public bool fileContinue;
        }
        public TasRecordingManager()
        {
            Instance = this;
            Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), $"TasRecordingManager init");
            WonderLog("initting tasrecordingmanager");
            playback = new TasRecordingFile("playback");
            room = new TasRecordingFile("room");
            attempt = new TasRecordingFile("attempt");
            flex = new TasRecordingFile("flex");
            tasRecordingFiles = new List<TasRecordingFile>
            {
                flex,
                room,
                attempt,
                playback
            };
        }
        public void OnUpdate()
        {
            /* can't init during constructor for some reason */
            if (null == InputState && WonderToolsModule.Settings.Enabled)
            {
                try
                {
                    WonderLog("initting tas recording state rn from inside tasrecording manager");
                    InputState = new TasRecordingState();
                }
                catch
                {
                    return;
                }
            }

            if (WonderToolsModule.Settings.KeyStartRecording.Pressed)
            {
                InitTasRecordingOptions options = default;
                InputState = new TasRecordingState();
                if (!flex.IsOpen)
                {
                    options.fileContinue = true;
                    Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), "Enabling recording");
                }
                else
                {
                    options.fileRestart = true;
                    Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), "Restarting recording");
                }
                flex.InitTasRecordingFile(options);
                flex.IsOpen = true;
            }
            else if (WonderToolsModule.Settings.KeyStopRecording.Pressed)
            {
                if (!flex.IsOpen) { return; }
                flex.IsOpen = false;
                flex.SaveTasRecordingFile();
            }
            if (!WonderToolsModule.Settings.ReplayBuffer && !flex.IsOpen) return;

            UpdatePauseState();
            InputState.Update();

            if (flex.IsOpen && InputState.Changed())
            {
                flex.AppendTasLine();
                InputState.framesSinceChange = 0;
            }
        }

        public void OnSaveState()
        {
            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"Tas save state hook");
        }
        public void OnLoadState()
        {
            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"Tas load state hook");
        }

        public void Level_OnLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"Tas level on load hook {level.Session.Level}");
            if (!flex.IsOpen) { return; }
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

        public override String ToString()
        {
            String ret = Environment.NewLine;
            int ii = 0;
            foreach (String line in lines)
            {
                ii++;
                ret += ii + " " + line + Environment.NewLine;
            }
            return ret;
        }
    }
}
