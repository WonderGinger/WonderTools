using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;

namespace Celeste.Mod.WonderTools.Streaks
{
    public class StreakCounterEntity : Entity
    {
        public StreakCounterEntity()
        {
            Tag = Tags.HUD;
        }
        public override void Render()
        {
            if (!WonderToolsModule.Settings.Streaks) return;
            var scale = 0.5f;
            var fontSize = ActiveFont.LineHeight * scale;
            var x = 15f;
            var y = 130f;

            ActiveFont.DrawOutline(
                string.Format("Streak: {0}", WonderToolsModule.Instance.StreakManager.StreakCount),
                position: new Vector2(x, y),
                justify: new Vector2(0f, 0f),
                scale: Vector2.One * scale,
                color: Color.LightGray,
                stroke: 1f,
                strokeColor: Color.Black);
        }
    }
}
