using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.WonderTools;

namespace Celeste.Mod.WonderTools.Streaks
{
    public class StreakManager
    {
        public int StreakCount { get; set; } = 0;
        public void OnSaveState()
        {
            StreakCount = 0;
        }
    }
}
