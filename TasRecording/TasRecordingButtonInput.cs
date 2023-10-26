using Monocle;
using MonoMod.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.WonderTools.TasRecording.TasRecordingState;
using static Celeste.Mod.WonderTools.TasRecording.TasRecordingManager;

namespace Celeste.Mod.WonderTools.TasRecording
{
    public class TasRecordingButtonInput
    {
        public int Check { get; }
        public bool Pressed { get; }
        public bool MenuCheck { get; }
        private readonly VirtualButton _button;
        private readonly List<int> _checkedBindingIds;
        public TasRecordingButtonInput(VirtualButton button)
        {
            _button = button;
            _checkedBindingIds = CheckCount(button);
            Check = _checkedBindingIds.Count;
            Pressed = button.Binding.Pressed(button.GamepadIndex, button.Threshold);
        }
        public static List<int> CheckCount(VirtualButton button)
        {
            var ret = new List<int>();
            int idx = 0;
            foreach (var key in button.Binding.Keyboard)
            {
                if (MInput.Keyboard.Check(key)) ret.Add(idx);
                idx++;
            }

            if (button == Input.Pause && Input.ESC.Check) ret.Add(idx);

            idx = 100;
            foreach (var padButton in button.Binding.Controller)
            {
                if (MInput.GamePads[button.GamepadIndex].Check(padButton, button.Threshold)) ret.Add(idx);
                idx++;
            }
            return ret;
        }

        public override string ToString()
        {
            if (Check == 0) return "";

            var ret = "";
            if (_button == Input.Jump)
            {
                if (TasRecordingState.JumpState == PrevJumpState.JUMP_J) ret += "J";
                if (TasRecordingState.JumpState == PrevJumpState.JUMP_K) ret += "K";
            }
            else if (_button == Input.Dash) ret += "X";
            else if (_button == Input.CrouchDash) ret += "Z";
            else if (_button == Input.Grab) ret += "G";
            else if (_button == Input.Pause) ret += "S";
            else if (_button == Input.QuickRestart) ret += "Q";
            else if (_button == Input.MenuJournal) ret += "N";
            else if (_button == Input.MenuConfirm) ret += "O";
            return ret;
        }
    }
}
