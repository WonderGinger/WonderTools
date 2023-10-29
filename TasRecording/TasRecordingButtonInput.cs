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
        public int bufferFrames = 0;
        private VirtualButton _button;
        private List<int> _checkedBindingIds;
        const int MAX_BUFFER_PAUSE_FRAMES = 5;
        const int MAX_BUFFER_INPUT_FRAMES = 4;
        private string primary;
        private string secondary;
        public TasRecordingButtonInput(ref VirtualButton button, string primary = "", string secondary = "")
        {
            _button = button;
            _checkedBindingIds = CheckCount(button);
            Check = _checkedBindingIds.Count;
            Pressed = button.Binding.Pressed(button.GamepadIndex, button.Threshold);
            this.primary = primary;
            this.secondary = secondary;
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
            bufferFrames++;
            //GetMenuCheck();
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
        public string JumpStateCharacter(ButtonInputState buttonState) { return ButtonChar(buttonState, primary, secondary); }
        public string DashStateCharacter(ButtonInputState buttonState) { return ButtonChar(buttonState, primary, secondary); }
        public string CrouchDashStateChar(ButtonInputState buttonState) { return ButtonChar(buttonState, primary, secondary); }
        public string PauseChar(ButtonInputState buttonState) {
            /* Check is implied */
            if (LevelPaused && Pressed && bufferFrames <= MAX_BUFFER_PAUSE_FRAMES)
            {
                buttonState = ButtonInputState.BUTTON_SECONDARY;
                bufferFrames = 0;
            }
            else if (Pressed)
            {
                bufferFrames = 0;
                buttonState = ButtonInputState.BUTTON_PRIMARY;
            }
            if (bufferFrames > MAX_BUFFER_PAUSE_FRAMES)
            {
                buttonState = ButtonInputState.BUTTON_NOT_PRESSED;
                return "";
            }
            else { buttonState= ButtonInputState.BUTTON_PRIMARY; }
            return ButtonChar(buttonState, primary, secondary); 
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
            else if (MenuCheck || (Pressed && _button == Input.MenuConfirm)) AppendTasInputStr(ref ret, "O");
            else if (LevelPaused && _button == Input.MenuCancel)
            {
                AppendTasInputStr(ref ret, "C");
                DashState = ButtonInputState.BUTTON_SECONDARY;
            }
            else if (_button == Input.QuickRestart) AppendTasInputStr(ref ret, "Q");
            else if (_button == Input.MenuJournal) AppendTasInputStr(ref ret, "N");
            else if (_button == Input.Talk)
            {
                if (Pressed) bufferFrames = 0;
                if (bufferFrames < MAX_BUFFER_INPUT_FRAMES)
                { 
                    AppendTasInputStr(ref ret, "N");
                }
            }
            return ret;
        }
    }
}
