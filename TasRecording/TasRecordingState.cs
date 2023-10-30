using System;
using System.Collections.Generic;
using MonoMod.Utils;
using Monocle;
using static Celeste.Mod.WonderTools.TasRecording.TasRecordingManager;
using System.Numerics;
using Microsoft.VisualBasic.FileIO;

namespace Celeste.Mod.WonderTools.TasRecording
{
    public class TasRecordingState
    {
        public UInt32 frameTotal;
        public UInt32 framesSinceChange { get; set; }
        public VirtualJoystick Aim;
        public VirtualJoystick Feather;
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
        public static ButtonInputState JumpState = ButtonInputState.BUTTON_NOT_PRESSED;
        public static ButtonInputState DashState = ButtonInputState.BUTTON_NOT_PRESSED;
        public static ButtonInputState CrouchDashState = ButtonInputState.BUTTON_NOT_PRESSED; 
        public static ButtonInputState PauseState = ButtonInputState.BUTTON_NOT_PRESSED;
        private readonly List<TasRecordingButtonInput> trbiList;

        public int MoveX { get; set; }
        public int MoveY { get; set; }
        public int AimX { get; set; }
        public int AimY { get; set; }
        public int FeatherX { get; set; }
        public int FeatherY { get; set; }
        public enum DirectionalInputType {
            NONE_e,
            DASH_e,
            MOVEMENT_e,
            GAMEPLAY_e,
        };


        public TasRecordingState()
        {
            Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), $"TasRecordingState init");

            frameTotal = 0;
            framesSinceChange = 0;

            /* hack to see if inputs are possible */
            if (null == Input.MenuLeft)
            {
                throw new Exception("Failed to init TasRecordingState");
            }

            Aim = Input.Aim;
            Feather = Input.Feather;
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
            trbiList = new List<TasRecordingButtonInput>
            {
                new TasRecordingButtonInput(ref Jump, "J", "K"),
                new TasRecordingButtonInput(ref Dash, "X", "C"),
                new TasRecordingButtonInput(ref CrouchDash, "Z", "V"),
                new TasRecordingButtonInput(ref Grab, "G", "H"),
                new TasRecordingButtonInput(ref Pause, "S", "C"),
                new TasRecordingButtonInput(ref Confirm, "O", "J"),
                new TasRecordingButtonInput(ref Cancel, "C", "X"),
                new TasRecordingButtonInput(ref Talk, "N", "X")
            };
            Line = ToString();
        }

        public bool Changed()
        {
            //Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), string.Format("{0} \n{1}\n{2}", framesSinceChange, PrevLine, Line));
            return (!Line.Equals(PrevLine));
        }

        public static string ButtonChar(ButtonInputState buttonState, string s1, string s2)
        {
            return buttonState switch
            {
                ButtonInputState.BUTTON_PRIMARY => s1,
                ButtonInputState.BUTTON_SECONDARY => s2,
                ButtonInputState.BUTTON_NOT_PRESSED => "",
                _ => throw new Exception($"Unexpected button state {buttonState}"),
            };
        }

        public static void UpdateButtonState(VirtualButton button, ref ButtonInputState buttonState)
        {
            if (!button.Check) buttonState = ButtonInputState.BUTTON_NOT_PRESSED;
            else if (button.Binding.Pressed(button.GamepadIndex, button.Threshold))
            {
                if (buttonState == ButtonInputState.BUTTON_PRIMARY) buttonState = ButtonInputState.BUTTON_SECONDARY;
                else buttonState = ButtonInputState.BUTTON_PRIMARY;
            }
        }

        private void UpdateAxisStates()
        {

            MoveX = AxisX.Value;
            MoveY = AxisY.Value;

            AimX = (Math.Abs(Aim.Value.X) < Aim.Threshold) 
                ? 0 
                : (int)(Aim.Value.X / Math.Abs(Aim.Value.X));

            AimY = (Math.Abs(Aim.Value.Y) < Aim.Threshold)
                ? 0 
                : (int)(Aim.Value.Y / Math.Abs(Aim.Value.Y));

            FeatherX = (Math.Abs(Feather.Value.X) < Feather.Threshold) 
                ? 0 
                : (int)(Feather.Value.X / Math.Abs(Feather.Value.X));

            FeatherX = (Math.Abs(Feather.Value.X) < Feather.Threshold) 
                ? 0 
                : (int)(Feather.Value.X / Math.Abs(Feather.Value.X));

            FeatherY = (Math.Abs(Feather.Value.Y) < Feather.Threshold) 
                ? 0 
                : (int)(Feather.Value.Y / Math.Abs(Feather.Value.Y));
        }

        public void Update()
        {
            frameTotal++;
            framesSinceChange++;
            PrevLine = Line;

            /* Update state for buttons that could have multiple tas encodings */
            UpdateButtonState(Jump, ref JumpState);
            UpdateButtonState(Dash, ref DashState);
            UpdateButtonState(CrouchDash, ref CrouchDashState);
            UpdateButtonState(Pause, ref PauseState);

            UpdateAxisStates();
            trbiList.ForEach(button => button.UpdateButtonState());

            Line = ToString();
        }
        public static DirectionalInputType GetInputType(int move, int aim, int feather, int dir)
        {
            if ((3 * dir) == (move + aim + feather)) return DirectionalInputType.GAMEPLAY_e;
            if ((2 * dir) == (move + feather)) return DirectionalInputType.MOVEMENT_e;
            if (dir == aim) return DirectionalInputType.DASH_e;

            return DirectionalInputType.NONE_e;
        }

        public static string DirectionalInputTasString(DirectionalInputType inputType, string cardinal)
        {
            switch (inputType)
            {
                case DirectionalInputType.GAMEPLAY_e:
                {
                    return $",{cardinal}";
                }
                case DirectionalInputType.DASH_e:
                {
                    return $",A{cardinal}";
                }
                case DirectionalInputType.MOVEMENT_e:
                {
                    return $",M{cardinal}";
                }
                default:
                {
                    return "";
                }
            };
        }

        public override string ToString()
        {
            string ret = "";
            if (Jump == null || trbiList == null)
            {
                return ret;
            }
            if (LevelWasPaused)
            {
                Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), $"MenuDown {(bool)Input.MenuDown} MenuRight {(bool)Input.MenuRight} MenuLeft {(bool)Input.MenuLeft} MenuUp {(bool)Input.MenuUp} AxisX {AxisX.Value} AxisY {AxisY.Value}");
                if ((bool)Input.MenuLeft) AppendTasInputStr(ref ret, "L");
                if ((bool)Input.MenuRight) AppendTasInputStr(ref ret, "R");
                if ((bool)Input.MenuUp) AppendTasInputStr(ref ret, "U");
                if ((bool)Input.MenuDown) AppendTasInputStr(ref ret, "D");
            }
            else
            {
                //Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"Move {{X:{AxisX.Value} Y:{AxisY.Value}}}\tAim {{X:{Aim.Value.X} Y:{Aim.Value.Y}}}\tFeather {{X:{Feather.Value.X} Y:{Feather.Value.Y}}}");
                DirectionalInputType up = GetInputType(AxisY.Value, AimY, FeatherY, -1);
                DirectionalInputType left = GetInputType(AxisX.Value, AimX, FeatherX, -1);
                DirectionalInputType right = GetInputType(AxisX.Value, AimX, FeatherX, 1);
                DirectionalInputType down = GetInputType(AxisY.Value, AimY, FeatherY, 1);

                ret += DirectionalInputTasString(up, "U");
                ret += DirectionalInputTasString(left, "L");
                ret += DirectionalInputTasString(right, "R");
                ret += DirectionalInputTasString(down, "D");
            }

            trbiList.ForEach(button => ret += button.ToString());
            return ret;
        }
    }
}
