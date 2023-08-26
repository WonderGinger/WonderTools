using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;

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
        private PrevJumpState _jumpState;
        private enum PrevJumpState {
            JUMP_NOT_PRESSED = 0,
            JUMP_K = 1,
            JUMP_J = 2
        };

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
            Line = this.ToString();
        }

        public override string ToString()
        {
            string ret = "";
            if (Jump == null)
            {
                return ret;
            }
            if (AxisX.Value == -1 || MenuLeft) ret += ",L";
            if (AxisX.Value == 1 || MenuRight) ret += ",R";
            if (AxisY.Value == -1 || MenuUp) ret += ",U";
            if (AxisY.Value == 1 || MenuDown) ret += ",D";

            Logger.Log(LogLevel.Info, nameof(WonderToolsModule), String.Format("{0} {1} {2}", _jumpState, Jump.Check, Jump.Pressed));


            if (Jump.Check)
            {
                if (Jump.Pressed)
                {
                    switch (_jumpState)
                    {
                    case PrevJumpState.JUMP_J:
                        ret += ",K";
                        _jumpState = PrevJumpState.JUMP_K;
                        break;
                    case PrevJumpState.JUMP_K:
                        ret += ",J";
                        _jumpState = PrevJumpState.JUMP_J;
                        break;
                    default:
                        ret += ",J";
                        _jumpState = PrevJumpState.JUMP_J;
                        break;
                    }
                } // Jump.Pressed
                else
                { 
                    switch (_jumpState)
                    {
                    case PrevJumpState.JUMP_J:
                        ret += ",J";
                        break;
                    case PrevJumpState.JUMP_K:
                        ret += ",K";
                        break;
                    default:
                        ret += ",J";
                        _jumpState = PrevJumpState.JUMP_J;
                        break;
                    }
                } // !Jump.Pressed
            }  // Jump.Check
            else
            {
                _jumpState = PrevJumpState.JUMP_NOT_PRESSED;
            }

            if (Dash.Check || Cancel.Check || Talk.Check) ret += ",X";
            if (CrouchDash.Check) ret += ",Z";
            if (Grab.Check) ret += ",G";
            if (Pause.Check) ret += ",S";
            if (Confirm.Check) ret += ",O";
            
            return ret;
        }
    }
}
