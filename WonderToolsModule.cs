using Celeste.Mod.WonderTools.Integration;
using Celeste.Mod.WonderTools.Streaks;
using Celeste.Mod.WonderTools.TasRecording;

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.WonderTools
{
    public class WonderToolsModule : EverestModule
    {
        public static WonderToolsModule Instance { get; private set; }

        public override Type SettingsType => typeof(WonderToolsModuleSettings);
        public static WonderToolsModuleSettings Settings => (WonderToolsModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(WonderToolsModuleSession);
        public static WonderToolsModuleSession Session => (WonderToolsModuleSession)Instance._Session;
        public TasRecordingManager TasRecordingManager;

        public static readonly string REPLAY_ROOT = Path.Combine(Everest.PathGame, "WonderToolsRoot");

        public StreakManager StreakManager = new();

        public WonderToolsModule()
        {
            Instance = this;
            TasRecordingManager= new TasRecordingManager();
            
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(WonderToolsModule), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(WonderToolsModule), LogLevel.Info);
#endif
        }

        private void Level_OnLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            TasRecordingManager.Level_OnLoad(level,playerIntro, isFromLoader);
        }

        private void AddStreaks(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            level.Add(new Streaks.StreakCounterEntity());
        }

        private void LevelLoader_OnLoadingThread(Level level) 
        {
            TasRecordingManager.LevelLoader_OnLoadingThread(level);
        }

        private void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) 
        {
            TasRecordingManager.Level_OnExit(level, exit, mode, session, snow);
        }


        public override void Load()
        {
            On.Monocle.Engine.Update += Engine_Update;
            Everest.Events.Level.OnLoadLevel += Level_OnLoad;
            Everest.Events.Level.OnExit += Level_OnExit;
            Everest.Events.Level.OnLoadLevel += AddStreaks;
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
            SpeedrunToolIntegration.Load();
            CelesteTasIntegration.Load();
        }

        private void Engine_Update(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
        {
            orig(self, gameTime);
            if (!Settings.Enabled) return;
            TasRecordingManager.OnUpdate();
            if (Settings.KeyStreakToggle.Pressed) Settings.Streaks = !Settings.Streaks;
            else if (Settings.KeyStreakIncrement.Pressed) StreakManager.StreakCount++;
            else if (Settings.KeyStreakReset.Pressed) StreakManager.StreakCount = 0;
        }

        public override void Unload()
        {
            On.Monocle.Engine.Update -= Engine_Update;
            Everest.Events.Level.OnLoadLevel -= Level_OnLoad;
            Everest.Events.Level.OnExit -= Level_OnExit;
            Everest.Events.Level.OnLoadLevel -= AddStreaks;
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
            SpeedrunToolIntegration.Unload();
            CelesteTasIntegration.Unload();
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, FMOD.Studio.EventInstance snapshot)
		{
			base.CreateModMenuSection(menu, inGame, snapshot);
			//Settings.CreateOptions(menu, inGame, snapshot);
		}
        public static void WonderLog(string s)
        {
            Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), s);
        }
    }
}


