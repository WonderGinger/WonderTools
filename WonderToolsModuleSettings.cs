namespace Celeste.Mod.WonderTools
{
    public class WonderToolsModuleSettings : EverestModuleSettings
    {
        public bool Enabled { get; set; } = true;
        public bool Streaks { get; set; } = false;

        [SettingName("Tree Depth")]
        [SettingRange(0, 10)]
        public int TreeDepth { get; set; } = 5;
    }
}
