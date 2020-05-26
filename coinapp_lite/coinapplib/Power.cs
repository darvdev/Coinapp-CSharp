using System;
using System.Runtime.InteropServices;

namespace coinapplib
{
    public class Power
    {

        [DllImport("kernel32")]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        enum EXECUTION_STATE : UInt32
        {
            /// Informs the system that the state being set should remain in effect until the next call that uses ES_CONTINUOUS and one of the other state flags is cleared.
            ES_CONTINUOUS = 0x80000000,
            /// Forces the display to be on by resetting the display idle timer.
            ES_DISPLAY_REQUIRED = 0x2,
            /// Forces the system to be in the working state by resetting the system idle timer.
            ES_SYSTEM_REQUIRED = 0x1
        }

        private static bool state = false;

        /// <summary>
        /// TRUE computer state is awake all the time. Hibernate and sleeps are disabled.<br />
        /// FALSE computer state is normal. Hibernate and sleeps are enabled.
        /// </summary>
        public static bool Enabled
        {
            get { return state; }  
            set
            {
                state = value;
                if (value) AWAKE_ALL_THE_TIME();
                else WILL_SLEEP();
            }
        }

        private static EXECUTION_STATE AWAKE_ALL_THE_TIME()
        {
            return SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }

        private static EXECUTION_STATE WILL_SLEEP()
        {
            return SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }
    }
}
