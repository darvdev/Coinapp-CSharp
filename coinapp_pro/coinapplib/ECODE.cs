namespace coinapplib
{
    public class ECODE
    {
        //Return a string from XCODE.
        public static string ToString(int ecode)
        {
            switch (ecode)
            {
                case 0: return "DEVICE_READY";

                case 1: return "DEVICE_NOT_DETECTED";
                case 2: return "DEVICE_DISCONNECT";
                case 3: return "DEVICE_INFO_ERROR";
                case 4: return "DEVICE_NOT_COMPATIBLE";
                case 5: return "DEVICE_PORT_CANNOT_OPEN";
                case 6: return "DEVICE_PORT_CANNOT_CLOSE";

                case 10: return "SERVICE_PROCESS_TERMINATED";
                case 11: return "SERVICE_NOT_INSTALLED";
                case 12: return "SERVICE_CANNOT_START";
                case 13: return "SERVICE_INVALID_PATH";
                case 14: return "SERVICE_DISCONNECTED";

                case 20: return "APP_PROCESS_TERMINATED";
                case 21: return "CONFIGURATION_FILE_NOT_FOUND";
                case 22: return "INVALID_LOCK_PARAMETERS";
                case 23: return "SYSTEM_CONFIGURATION_NOT_FOUND";
                case 24: return "REGISTRY_STARTUP_NOT_FOUND";
                case 25: return "REGISTRY_STARTUP_INVALID";
                case 26: return "REGISTRY_DISABLETASKMGR_NOT_FOUND";
                case 27: return "REGISTRY_DISABLETASKMGR_INVALID";
                case 28: return "LOCK_SCREEN_TERMINATED";
                case 29: return "TIMER_DISPLAY_TERMINATED";

                default: return "INVALID_ECODE";
            }
        }

        public const int

            //Boot of the app.
            DEVICE_READY = 0,

            //Device problem.
            DEVICE_NOT_DETECTED = 1,
            DEVICE_DISCONNECT = 2,
            DEVICE_INFO_ERROR = 3,
            DEVICE_NOT_COMPATIBLE = 4,
            DEVICE_PORT_CANNOT_OPEN = 5,
            DEVICE_PORT_CANNOT_CLOSE = 6,

            //Service problem.
            SERVICE_PROCESS_TERMINATED = 10,
            SERVICE_NOT_INSTALLED = 11,
            SERVICE_CANNOT_START = 12,
            SERVICE_INVALID_PATH = 13,
            SERVICE_DISCONNECTED = 14,

        //App problem.
            APPLICATION_PROCESS_TERMINATED = 20,
            CONFIGURATION_FILE_NOT_FOUND = 21,
            INVALID_LOCK_PARAMETERS = 22,
            SYSTEM_CONFIGURATION_FILE_NOT_FOUND = 23,
            REGISTRY_STARTUP_NOT_FOUND = 24,
            REGISTRY_STARTUP_INVALID = 25,
            REGISTRY_DISABLETASKMGR_NOT_FOUND = 26,
            REGISTRY_DISABLETASKMGR_INVALID = 27,
            LOCK_SCREEN_TERMINATED = 28,
            TIMER_DISPLAY_TERMINATED = 29;
            
    }

}
