using System;
using System.IO;
using System.Collections.Generic;

using static Celeste.Mod.WonderTools.TasRecording.TasRecordingManager;
using Celeste.Mod.WonderTools.Integration;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.WonderTools.TasRecording
{
    internal class TasRecordingFile
    {
        public string filename { get; private set; }
        private string path;
        private StreamWriter _tasRecordingWriter;
        public List<string> Lines { get; set; }
        public bool IsOpen { get; set;} = false;
        private InitTasRecordingOptions options;
        public TasRecordingFile (string name, InitTasRecordingOptions options = default, List<string> lines = null)
        {
            this.options = options;

            if (!options.noFile) Directory.CreateDirectory(Path.Combine(WonderToolsModule.REPLAY_ROOT));
            filename = name;
            SetPath();

            Lines = new List<string>();
            if (!options.append)
            {
                ClearTextTasRecordingFile();
            }
            if (options.copy && lines != null)
            {
                AppendLines(lines);
            }
        }

        public bool SetName (string name)
        {
            if (IsOpen) { return false; }
            filename = name;
            SetPath();
            return true;
        }

        private void SetPath()
        {
            path = Path.Combine(WonderToolsModule.REPLAY_ROOT, filename + ".tas");
        }

        public void WriteTasRecordingFile()
        {
            Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), $"Writing recording file {path}");
            OpenTasRecordingFile(options.append);
            Lines.Insert(0, item: HeaderString());
            Lines.Insert(1, item: string.Empty);
            Lines.ForEach(_tasRecordingWriter.WriteLine);
        }

        public void OverwriteTasRecordingFile(List<string> lines)
        {
            ClearTextTasRecordingFile();
            AppendLines(lines);
            WriteAndCloseTasRecordingFile();
        }

        public void AppendLines(List<string> lines)
        {
            Lines.AddRange(lines);
        }

        public void ClearTextTasRecordingFile()
        {
            if (!File.Exists(path)) { return; }
            //Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), $"Clearing recording file {path}");
            if (IsOpen)
            {
                CloseTasRecordingFile();
            }
            File.WriteAllText(path, string.Empty);
        }

        public void WriteAndCloseTasRecordingFile()
        {
            WriteTasRecordingFile();
            CloseTasRecordingFile();
        }

        public void AppendBreakpoint(bool save = false)
        {
            Lines.Add(save ? "***S" : "***");
        }
        public void AppendLevelComment(string level) 
        {
            Lines.Add(string.Empty); 
            Lines.Add($"# lvl_{level}"); 
        }

        public void OpenTasRecordingFile(bool append)
        {
            if (IsOpen)
            {
                CloseTasRecordingFile();
            }
            _tasRecordingWriter = new StreamWriter(path, append);
            IsOpen = true;
        }

        private void CloseTasRecordingFile()
        {
            if (!IsOpen)
            {
                return;
            }

            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"Closing recording file {path}");

            _tasRecordingWriter.Flush();
            _tasRecordingWriter.Close();
            IsOpen = false;

        }

        public void ResetTasRecordingFile()
        {
            ClearTextTasRecordingFile();
            Lines.Clear();
        }

        public void AppendTasLine()
        {
            string tasLine =  string.Format("{0, 4}", InputState.framesSinceChange.ToString()) + InputState.PrevLine;
            Lines.Add(tasLine);
        }

        public void AppendConsoleCommand()
        {
            Lines.Add(CelesteTasIntegration.CreateConsoleCommand(false));
            Lines.Add(string.Format("{0, 4}", "1"));
            Lines.Add(string.Format("{0, 4}", "36"));
            Lines.Add(string.Empty);
        }

        public override string ToString()
        {
            string ret = Environment.NewLine;
            int ii = 0;
            foreach (string line in Lines)
            {
                ii++;
                ret += ii + " " + line + Environment.NewLine;
            }
            return ret;
        }

        ~TasRecordingFile()
        {
            CloseTasRecordingFile();
        }
    }
}
