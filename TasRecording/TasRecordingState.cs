using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using static Celeste.Mod.WonderTools.TasRecording.TasRecordingManager;

namespace Celeste.Mod.WonderTools.TasRecording
{
    internal class TasRecordingState
    {
        public TasRecordingState()
        {
            frameTotal = 0;
            framesSinceChange = 0;
            if (null == Input.MenuLeft) return;
            MenuLeft = Input.MenuLeft;
            MenuRight = Input.MenuRight;
            MenuUp = Input.MenuUp;
            MenuDown = Input.MenuDown;
            AxisX = Input.MoveX;
            AxisY = Input.MoveY;
            Jump = Input.Jump;
            Dash = Input.Dash;
            CrouchDash = Input.CrouchDash;
            Grab = Input.Grab;
            Pause = Input.Pause;
            Confirm = Input.MenuConfirm;
            Cancel = Input.MenuCancel;
            Talk = Input.Talk;
            Line = this.ToString();
        }

        public UInt32 frameTotal;
        public UInt32 framesSinceChange;
        public bool MenuLeft;
        public bool MenuRight;
        public bool MenuUp;
        public bool MenuDown;
        public VirtualIntegerAxis AxisX;
        public VirtualIntegerAxis AxisY;
        public VirtualButton Jump;
        public VirtualButton Dash;
        public VirtualButton CrouchDash;
        public VirtualButton Grab;
        public VirtualButton Pause;
        public VirtualButton Confirm;
        public VirtualButton Cancel;
        public VirtualButton Talk;
        public String Line;
        public String PrevLine;
        public bool Paused;
        public static PrevButtonInputState JumpState;
        public static PrevButtonInputState DashState;
        public static PrevButtonInputState CrouchDashState;
        public static PrevButtonInputState PauseState;
        private List<TasRecordingButtonInput> trbiList;


        public static char ButtonChar(PrevButtonInputState buttonState, char c1, char c2)
        {
            switch (buttonState)
            {
                case PrevButtonInputState.BUTTON_PRIMARY:
                    return c1;
                case PrevButtonInputState.BUTTON_SECONDARY:
                    return c2;
                default:
                    throw new Exception("Unexpected button state");
            }
        }
        public static char JumpStateCharacter(PrevButtonInputState buttonState) { return ButtonChar(buttonState, 'J', 'K'); }
        public static char DashStateCharacter(PrevButtonInputState buttonState) { return ButtonChar(buttonState, 'C', 'X'); }
        public static char CrouchDashStateChar(PrevButtonInputState buttonState) { return ButtonChar(buttonState, 'Z', 'V'); }
        public static char PauseChar(PrevButtonInputState buttonState) { return ButtonChar(buttonState, 'S', 'Q'); }

        public static void UpdateButtonState(VirtualButton button, ref PrevButtonInputState buttonState)
        {
            if (!button.Check) buttonState = PrevButtonInputState.BUTTON_NOT_PRESSED;
            else if (button.Binding.Pressed(button.GamepadIndex, button.Threshold))
            {
                if (buttonState == PrevButtonInputState.BUTTON_PRIMARY) buttonState = PrevButtonInputState.BUTTON_SECONDARY;
                else buttonState = PrevButtonInputState.BUTTON_PRIMARY;
            }
        }

        public void Update()
        {
            frameTotal++;
            AxisX = Input.MoveX;
            AxisY = Input.MoveY;
            Jump = Input.Jump;
            Dash = Input.Dash;
            CrouchDash = Input.CrouchDash;
            Grab = Input.Grab;
            Pause = Input.Pause;
            Confirm = Input.MenuConfirm;
            Cancel = Input.MenuCancel;
            Talk = Input.Talk;
            PrevLine = Line;

            UpdateButtonState(Jump, ref JumpState);
            UpdateButtonState(Dash, ref DashState);
            UpdateButtonState(CrouchDash, ref CrouchDashState);
            UpdateButtonState(Pause, ref PauseState);

            trbiList = new List<TasRecordingButtonInput>
            {
                new TasRecordingButtonInput(Jump),
                new TasRecordingButtonInput(Dash),
                new TasRecordingButtonInput(CrouchDash),
                new TasRecordingButtonInput(Grab),
                new TasRecordingButtonInput(Pause),
                new TasRecordingButtonInput(Confirm),
                new TasRecordingButtonInput(Cancel),
                new TasRecordingButtonInput(Talk)
            };

            Line = ToString();
        }

        public override string ToString()
        {
            string ret = "";
            if (Jump == null || trbiList == null)
            {
                return ret;
            }
            if (AxisX.Value == -1 || MenuLeft) TasRecordingButtonInput.AppendTasInputChar(ref ret, 'L');
            if (AxisX.Value == 1 || MenuRight) TasRecordingButtonInput.AppendTasInputChar(ref ret, 'R');
            if (AxisY.Value == -1 || MenuUp) TasRecordingButtonInput.AppendTasInputChar(ref ret, 'U');
            if (AxisY.Value == 1 || MenuDown) TasRecordingButtonInput.AppendTasInputChar(ref ret, 'D');

            //Logger.Log(LogLevel.Info, nameof(WonderToolsModule), String.Format("{0, -10} {1} {2} {3}", Engine.Scene.TimeActive,JumpState, Jump.Check, Jump.Binding.Pressed(Jump.GamepadIndex, Jump.Threshold)));

            foreach (TasRecordingButtonInput button in trbiList)
            {
                ret += button.ToString();
            }
            return ret;
        }
    }
}
