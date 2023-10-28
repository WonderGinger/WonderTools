using System;
using System.Collections.Generic;
using MonoMod.Utils;
using Monocle;
using static Celeste.Mod.WonderTools.TasRecording.TasRecordingManager;
using System.Numerics;

namespace Celeste.Mod.WonderTools.TasRecording
{
    internal class TasRecordingState
    {
        public UInt32 frameTotal;
        public UInt32 framesSinceChange;
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
        public bool Paused;
        public static PrevButtonInputState JumpState;
        public static PrevButtonInputState DashState;
        public static PrevButtonInputState CrouchDashState;
        public static PrevButtonInputState PauseState;
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
            frameTotal = 0;
            framesSinceChange = 0;
            if (null == Input.MenuLeft) return;

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
                new TasRecordingButtonInput(ref Jump),
                new TasRecordingButtonInput(ref Dash),
                new TasRecordingButtonInput(ref CrouchDash),
                new TasRecordingButtonInput(ref Grab),
                new TasRecordingButtonInput(ref Pause),
                new TasRecordingButtonInput(ref Confirm),
                new TasRecordingButtonInput(ref Cancel),
                new TasRecordingButtonInput(ref Talk)
            };
            Line = ToString();
        }

        public static string ButtonChar(PrevButtonInputState buttonState, string s1, string s2)
        {
            return buttonState switch
            {
                PrevButtonInputState.BUTTON_PRIMARY => s1,
                PrevButtonInputState.BUTTON_SECONDARY => s2,
                PrevButtonInputState.BUTTON_NOT_PRESSED => "",
                _ => throw new Exception($"Unexpected button state {buttonState}"),
            };
        }
        public static string JumpStateCharacter(PrevButtonInputState buttonState) { return ButtonChar(buttonState, "J", "K"); }
        public static string DashStateCharacter(PrevButtonInputState buttonState) { return ButtonChar(buttonState, "C", "X"); }
        public static string CrouchDashStateChar(PrevButtonInputState buttonState) { return ButtonChar(buttonState, "Z", "V"); }
        public static string PauseChar(PrevButtonInputState buttonState) { return ButtonChar(buttonState, "S", "Q"); }

        public static void UpdateButtonState(VirtualButton button, ref PrevButtonInputState buttonState)
        {
            if (!button.Check) buttonState = PrevButtonInputState.BUTTON_NOT_PRESSED;
            else if (button.Binding.Pressed(button.GamepadIndex, button.Threshold))
            {
                if (buttonState == PrevButtonInputState.BUTTON_PRIMARY) buttonState = PrevButtonInputState.BUTTON_SECONDARY;
                else buttonState = PrevButtonInputState.BUTTON_PRIMARY;
            }
        }

        private void UpdateAxisStates()
        {

            MoveX = AxisX.Value;
            MoveY = AxisY.Value;

            if (Math.Abs(Aim.Value.X) < Aim.Threshold)
                AimX = 0;
            else
                AimX = (int)(Aim.Value.X / Math.Abs(Aim.Value.X));
            if (Math.Abs(Aim.Value.Y) < Aim.Threshold)
                AimY = 0;
            else
                AimY = (int)(Aim.Value.Y / Math.Abs(Aim.Value.Y));

            if (Math.Abs(Feather.Value.X) < Feather.Threshold)
                FeatherX = 0;
            else
                FeatherX = (int)(Feather.Value.X / Math.Abs(Feather.Value.X));
            if (Math.Abs(Feather.Value.Y) < Feather.Threshold)
                FeatherY = 0;
            else
                FeatherY = (int)(Feather.Value.Y / Math.Abs(Feather.Value.Y));
        }

        public void Update()
        {

            frameTotal++;
            PrevLine = Line;

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
            if (Paused)
            {
                //Logger.Log(LogLevel.Debug, nameof(WonderToolsModule), $"MenuDown {(bool)Input.MenuDown} MenuRight {(bool)Input.MenuRight} MenuLeft {(bool)Input.MenuLeft} MenuUp {(bool)Input.MenuUp} AxisX {AxisX.Value} AxisY {AxisY.Value}");
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

            //Logger.Log(LogLevel.Info, nameof(WonderToolsModule), String.Format("{0, -10} {1} {2} {3}", Engine.Scene.TimeActive,JumpState, Jump.Check, Jump.Binding.Pressed(Jump.GamepadIndex, Jump.Threshold)));
            //Logger.Log(LogLevel.Info, nameof(WonderToolsModule), string.Format("{0, -10}\n\tMove {{X:{1} Y:{2}}} \n\tDash {3}", Engine.Scene.TimeActive, Input.MoveX.Value, Input.MoveY.Value, Input.Aim.Value));

            //Logger.Log(LogLevel.Info, nameof(WonderToolsModule), $"MenuDown {MenuDown} MenuRight {MenuRight} MenuLeft {MenuLeft} MenuUp {MenuUp} AxisX {AxisX.Value} AxisY {AxisY.Value}");

            trbiList.ForEach(button => ret += button.ToString());
            return ret;
        }
    }
}
