using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;
using static Celeste.Mod.WonderTools.TasRecording.TasRecordingManager;
using static Celeste.Mod.WonderTools.TasRecording.TasRecordingState;

namespace Celeste.Mod.WonderTools.TasRecording
{
    public class TasRecordingButtonInput
    {
        private int Check { get; set; }
        private bool Pressed { get; set;  }
        public bool MenuCheck { get; set;  } = false;
        private VirtualButton _button;
        private List<int> _checkedBindingIds;
        public TasRecordingButtonInput(ref VirtualButton button)
        {
            _button = button;
            _checkedBindingIds = CheckCount(button);
            Check = _checkedBindingIds.Count;
            Pressed = button.Binding.Pressed(button.GamepadIndex, button.Threshold);
            //CheckPaused();
        }

        private void GetMenuCheck()
        {
            if (Engine.Scene is Level level)
            {
                // Credit: InputHistory mod
                // The frame you hit a button to close the pause menu, level.Pause becomes false,
                // so check wasPaused instead, as that stays true for one extra frame.
                if (level.Paused || DynamicData.For(level).Get<bool>("wasPaused"))
                {
                    // Various menu button hacks
                    if (_button == Input.Jump) MenuCheck = Input.MenuConfirm.Check;
                    if (_button == Input.Dash) MenuCheck = Input.MenuCancel.Check;
                }
            }
        }

        public void UpdateButtonState()
        {
            _checkedBindingIds = CheckCount(_button);
            Check = _checkedBindingIds.Count;
            Pressed = _button.Binding.Pressed(_button.GamepadIndex, _button.Threshold);
            //CheckPaused();
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
            if (Check == 0 && !MenuCheck) return "";

            var ret = "";
            if (_button == Input.Jump)
            {
                AppendTasInputStr(ref ret, JumpStateCharacter(JumpState));
            }
            else if (_button == Input.Dash)
            {
                AppendTasInputStr(ref ret, DashStateCharacter(DashState));
            }
            else if (_button == Input.Grab) AppendTasInputStr(ref ret, "G");
            else if (_button == Input.CrouchDash)
            {
                AppendTasInputStr(ref ret, CrouchDashStateChar(CrouchDashState));
            }
            else if (_button == Input.Pause)
            {
                AppendTasInputStr(ref ret, PauseChar(PauseState));
            }
            else if (_button == Input.QuickRestart) AppendTasInputStr(ref ret, "Q");
            else if (_button == Input.MenuJournal) AppendTasInputStr(ref ret, "N");
            else if (MenuCheck || _button == Input.MenuConfirm) AppendTasInputStr(ref ret, "O");
            return ret;
        }
    }
}
