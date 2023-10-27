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
        
        public static void AppendTasInputChar(ref string inputLine, char inputChar)
        {
            inputLine += $",{inputChar}";
        }

        public override string ToString()
        {
            if (Check == 0) return "";

            var ret = "";
            if (_button == Input.Jump)
            {
                AppendTasInputChar(ref ret, JumpStateCharacter(JumpState));
            }
            else if (_button == Input.Dash)
            {
                AppendTasInputChar(ref ret, DashStateCharacter(DashState));
            }
            else if (_button == Input.CrouchDash)
            {
                AppendTasInputChar(ref ret, CrouchDashStateChar(CrouchDashState));
            }
            else if (_button == Input.Grab) AppendTasInputChar(ref ret, 'G');
            else if (_button == Input.Pause)
            {
                AppendTasInputChar(ref ret, PauseChar(PauseState));
            }
            else if (_button == Input.QuickRestart) AppendTasInputChar(ref ret, 'Q');
            else if (_button == Input.MenuJournal) AppendTasInputChar(ref ret, 'N');
            else if (_button == Input.MenuConfirm && Pressed) AppendTasInputChar(ref ret, 'O');
            return ret;
        }
    }
}
