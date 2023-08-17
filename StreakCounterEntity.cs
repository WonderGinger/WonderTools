using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;

namespace Celeste.Mod.WonderTools
{
    public class StreakCounterEntity : Entity
    {
        public StreakCounterEntity(int _streakCount) 
        {
            Tag = Tags.HUD;
            streakCount= _streakCount;
        }
        public int streakCount { get; set; } = 0;
        public override void Render()
        {
            if (!WonderToolsModule.Settings.Streaks) return;
            var scale = 0.5f;
            var fontSize = ActiveFont.LineHeight * scale;
            var x = 10f;
            var y = 120f;

            ActiveFont.DrawOutline(
                String.Format("Streak: {0}", streakCount),
                position: new Vector2(x, y),
                justify: new Vector2(0f, 0f),
                scale: Vector2.One * scale,
                color: Color.White, 
                stroke: 2f,
                strokeColor: Color.Black);
        }
    }
}
