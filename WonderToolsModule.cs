using Celeste.Mod.WonderTools.Integration;
using Celeste.Mod.WonderTools.Streaks;
using Celeste.Mod.WonderTools.TasRecording;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.WonderTools
{
    public class WonderToolsModule : EverestModule
    {
        public static WonderToolsModule Instance { get; private set; }

        public override Type SettingsType => typeof(WonderToolsModuleSettings);
        public static WonderToolsModuleSettings Settings => (WonderToolsModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(WonderToolsModuleSession);
        public static WonderToolsModuleSession Session => (WonderToolsModuleSession)Instance._Session;
        public TasRecordingManager TasRecordingManager = new TasRecordingManager();

        public const string REPLAY_ROOT = "WonderToolsRoot";

        public StreakManager StreakManager { get; private set; } = new StreakManager();

        public WonderToolsModule()
        {
            Instance = this;
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
            return;
        }

        private void AddStreaks(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            level.Add(new Streaks.StreakCounterEntity());
        }

        public override void Load()
        {
            On.Monocle.Engine.Update += Engine_Update;
            Everest.Events.Level.OnLoadLevel += Level_OnLoad;
            Everest.Events.Level.OnLoadLevel += AddStreaks;
            SpeedrunToolIntegration.Load();
        }

        private void Engine_Update(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
        {
            orig(self, gameTime);
            TasRecordingManager.OnUpdate();
            if (Settings.KeyStreakIncrement.Pressed) StreakManager.StreakCount++;
            if (Settings.KeyStreakReset.Pressed) StreakManager.StreakCount = 0;
            if (Settings.KeyStreakToggle.Pressed) Settings.Streaks = !Settings.Streaks;
        }

        public override void Unload()
        {
            On.Monocle.Engine.Update -= Engine_Update;
            Everest.Events.Level.OnLoadLevel -= Level_OnLoad;
            Everest.Events.Level.OnLoadLevel -= AddStreaks;
            SpeedrunToolIntegration.Unload();
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, FMOD.Studio.EventInstance snapshot)
		{
			base.CreateModMenuSection(menu, inGame, snapshot);
			//Settings.CreateOptions(menu, inGame, snapshot);
		}
    }
}


