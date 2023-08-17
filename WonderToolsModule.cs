using Celeste.Mod.UI;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using On.Celeste.Mod;

namespace Celeste.Mod.WonderTools
{
    public class WonderToolsModule : EverestModule
    {
        public static WonderToolsModule Instance { get; private set; }

        public override Type SettingsType => typeof(WonderToolsModuleSettings);
        public static WonderToolsModuleSettings Settings => (WonderToolsModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(WonderToolsModuleSession);
        public static WonderToolsModuleSession Session => (WonderToolsModuleSession)Instance._Session;

        private const string REPLAY_ROOT = "WonderToolsRoot";

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
          Logger.Log(LogLevel.Info, nameof(WonderToolsModule), String.Format("ifl {0}", isFromLoader));
        }

        private void AddStreaks(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            level.Add(new StreakCounterEntity(0));
        }

        public override void Load()
        {
            // TODO: apply any hooks that should always be active
            Everest.Events.Level.OnLoadLevel += Level_OnLoad;
            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), "There is a hook now");
            Everest.Events.Level.OnLoadLevel += AddStreaks;
        }


        public override void Unload()
        {
            // TODO: unapply any hooks applied in Load()
            Everest.Events.Level.OnLoadLevel -= Level_OnLoad;
            Everest.Events.Level.OnLoadLevel -= AddStreaks;
        }
    }
}


