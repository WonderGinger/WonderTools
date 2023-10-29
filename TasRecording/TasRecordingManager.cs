using FMOD;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.WonderTools.TasRecording
{
    public class TasRecordingManager
    {
        public static TasRecordingManager Instance { get; private set; }

        private List<String> lines = new List<String>();
        private StreamWriter recentRoomWriter;
        private StreamWriter _tasRecordingWriter;
        private readonly TasRecordingFile room;
        private readonly TasRecordingFile attempt;
        private readonly TasRecordingFile playback;
        private readonly TasRecordingFile flex;
        private readonly List<TasRecordingFile> tasRecordingFiles;
        private bool _recording = false;
        public static bool Paused { get; set; }
        public static bool PausedPrev { get; set; }
        public static bool LevelPaused { get; set; }
        public static bool LevelPausedPrev { get; set; }
        public static bool WasPaused { get; set; }

        private TasRecordingState _state;
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
            _state = new TasRecordingState();
            playback = new TasRecordingFile("playback");
            room = new TasRecordingFile("room");
            attempt = new TasRecordingFile("attempt");
            tasRecordingFiles.Add(room);
            tasRecordingFiles.Add(attempt);
        }
        public void OnUpdate()
        {
            if (WonderToolsModule.Settings.KeyStartRecording.Pressed)
            {
                string name = "flex";
                InitTasRecordingOptions options = default;
                if (!_recording)
                {
                    //options.fileClear = true;
                    options.fileContinue = true;
                    Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), "Enabling recording");
                }
                else
                {
                    options.fileRestart = true;
                    Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), "Restarting recording");
                }
                flex.InitTasRecordingFile(name, options);
                _state = new TasRecordingState();
                _recording = true;
            }
            else if (WonderToolsModule.Settings.KeyStopRecording.Pressed)
            {
                if (!_recording) { return; }
                _recording = false;
                SaveTasRecordingFile();
            }
            if (!_recording) return;
            
            if (Engine.Scene is Level level)
            {
                PausedPrev = Paused;
                Paused = false;
                LevelPausedPrev = LevelPaused;
                LevelPaused = level.Paused;
                WasPaused = DynamicData.For(level).Get<bool>("wasPaused");
                if (level.Paused || DynamicData.For(level).Get<bool>("wasPaused"))
                {
                    Paused = true;
                }
            }
            _state.Update();

            if (_state.PrevLine == _state.Line)
            {
                _state.framesSinceChange++;
            } else
            {
                //Logger.Log(LogLevel.Info, nameof(WonderToolsModule), String.Format("{0} {1} {2}", _state.framesSinceChange, _state.PrevLine, _state.Line));
                AppendTasLine();
                _state.framesSinceChange = 1;
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
            if (!_recording) { return; }
        }
        public void InitTasRecordingFile(string path, InitTasRecordingOptions options)
        {
            string filename = Path.Combine(WonderToolsModule.REPLAY_ROOT, path + ".tas");

            if (options.fileRestart)
            {
                Logger.Log(LogLevel.Info, nameof(WonderToolsModule), "Creating recording file");
                CloseTasRecordingFile();

                Directory.CreateDirectory(Path.Combine(WonderToolsModule.REPLAY_ROOT));
                ClearTasRecordingFile(filename);

                _tasRecordingWriter = new StreamWriter(filename);
                lines.Add(item: $"# This file was autogenerated by {nameof(WonderToolsModule)} on {DateTime.Now}");
            }
            if (options.fileContinue)
            {
                Logger.Log(LogLevel.Info, nameof(WonderToolsModule), "Continuing recording file");
                Directory.CreateDirectory(Path.Combine(WonderToolsModule.REPLAY_ROOT));
                _tasRecordingWriter = new StreamWriter(filename);
                lines.Add(item: $"# This file was autogenerated by {nameof(WonderToolsModule)} on {DateTime.Now}");
            }
        }

        public void AppendTasLine()
        {
            string prefixFrameCount = String.Format("{0, 4}", _state.framesSinceChange.ToString());
            string tasLine =  prefixFrameCount + _state.PrevLine;
            lines.Add(tasLine);
        }

        public static void AppendTasInputStr(ref string inputLine, string inputStr)
        {
            if (inputStr.Equals(""))
            {
                return;
            }
            inputLine += $",{inputStr}";
        }

        public void SaveTasRecordingFile()
        {
            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), "Saving recording file");
            foreach (string line in lines)
            {
                _tasRecordingWriter.WriteLine(line);
            }
            CloseTasRecordingFile();
        }

        public void CloseTasRecordingFile()
        {
            _tasRecordingWriter.Flush();
            _tasRecordingWriter.Close();
        }


        public void ClearTasRecordingFile(string filename)
        {
            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), String.Format("Clearing recording file {0}", this));
            File.WriteAllText(filename, string.Empty);
            lines.Clear();
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
