using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.WonderTools.TasRecording
{
    public class TasRecordingManager
    {
        public static TasRecordingManager Instance { get; private set; }

        private List<String> lines = new List<String>();
        private bool _recording = false;
        public TasRecordingManager()
        {
            Instance = this;
        }
        public void OnUpdate()
        {
            if (WonderToolsModule.Settings.KeyStartRecording.Pressed)
            {
                Logger.Log(LogLevel.Info, WonderToolsModule.REPLAY_ROOT, "Enabling recording");
                _recording = true;
            }
            if (WonderToolsModule.Settings.KeyStopRecording.Pressed)
            {
                if (!_recording) { return; }
                _recording = false;
                SaveTasRecordingFile();
                ClearTasRecordingFile();
            }
            return;
        }
        public void Level_OnLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (!_recording) { return; }
            AppendTasLine();
        }

        public void InitTasRecordingFile()
        {
            Logger.Log(LogLevel.Info, WonderToolsModule.REPLAY_ROOT, "Creating recording file");
            lines.Add("0 init");
        }

        public void AppendTasLine()
        {
            String line =  lines.Count.ToString();
            Logger.Log(LogLevel.Info, WonderToolsModule.REPLAY_ROOT, String.Format("Appending {0} to Tas file", line));
            lines.Add(line);
        }

        public void SaveTasRecordingFile()
        {
            Logger.Log(LogLevel.Info, WonderToolsModule.REPLAY_ROOT, "Saving recording file");
        }

        public void ClearTasRecordingFile()
        {
            Logger.Log(LogLevel.Info, WonderToolsModule.REPLAY_ROOT, String.Format("Clearing recording file {0}", this));
            lines.Clear();
        }
        public override String ToString()
        {
            String ret = "\n";
            int ii = 0;
            foreach (String line in lines)
            {
                ii++;
                ret += ii + " " + line + "\n";
            }
            return ret;
        }
    }
}
