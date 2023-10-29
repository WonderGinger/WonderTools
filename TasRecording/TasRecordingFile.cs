﻿using System;
using System.IO;
using System.Collections.Generic;

using static Celeste.Mod.WonderTools.TasRecording.TasRecordingManager;

namespace Celeste.Mod.WonderTools.TasRecording
{
    internal class TasRecordingFile
    {
        string filename;
        private StreamWriter _tasRecordingWriter;
        private List<String> lines;
        bool _disposed = false;
        public TasRecordingFile (string name)
        {
            lines = new List<String>();
            filename = Path.Combine(WonderToolsModule.REPLAY_ROOT, name);
        }
        
        ~TasRecordingFile()
        {
            if (_disposed) return;
            CloseTasRecordingFile();
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
            _disposed= true;
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
