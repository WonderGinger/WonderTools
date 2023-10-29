using FMOD.Studio;
using static Celeste.TextMenuExt;
using System.Windows.Markup;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.WonderTools
{
    [SettingName("WonderTools_Setting")]
    public class WonderToolsModuleSettings : EverestModuleSettings
    {
        public static WonderToolsModuleSettings Instance { get; private set; }
        public WonderToolsModuleSettings()
        {
            Instance = this;
        }

        #region Settings
        public bool Enabled { get; set; } = true;
        public bool Streaks { get; set; } = false;

        #endregion

        #region Hotkey

        [SettingName("WT_KEY_START_RECORDING")]
        [DefaultButtonBinding(0, Keys.F7)]
        public ButtonBinding KeyStartRecording { get; set; } = new(0, Keys.F7);

        [SettingName("WT_KEY_END_RECORDING")]
        [DefaultButtonBinding(0, Keys.F8)]
        public ButtonBinding KeyStopRecording { get; set; } = new(0, Keys.F8);

        [SettingName("WT_KEY_SAVE_BUFFER_ROOM")]
        [DefaultButtonBinding(0, Keys.F8)]
        public ButtonBinding KeyBufferSaveRoomRecording { get; set; } = new(0, Keys.F9);

        [SettingName("WT_KEY_SAVE_BUFFER_STATE")]
        [DefaultButtonBinding(0, Keys.F8)]
        public ButtonBinding KeyBufferSaveStateRecording { get; set; } = new(0, Keys.F10);

        [SettingName("WT_KEY_STREAK_INCREMENT")]
        [DefaultButtonBinding(0, Keys.OemPlus)]
        public ButtonBinding KeyStreakIncrement { get; set; } = new(0, Keys.OemPlus);

        [SettingName("WT_KEY_STREAK_RESET")]
        [DefaultButtonBinding(0, Keys.Delete)]
        public ButtonBinding KeyStreakReset { get; set; } = new(0, Keys.Delete);

        [SettingName("WT_KEY_STREAK_TOGGLE")]
        public ButtonBinding KeyStreakToggle { get; set; }


        #endregion


    }
}
