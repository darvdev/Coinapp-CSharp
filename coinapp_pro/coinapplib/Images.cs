using System.Drawing;
using coinapplib.Properties;

namespace coinapplib
{
    public class Images
    {

        //Icons
        public static Icon admin { get { return Resources.admin; } }
        public static Icon admin_x { get { return Resources.admin_x; } }
        public static Icon coinapp { get { return Resources.coinapp; } }
        public static Icon coinappSvc { get { return Resources.coinappSvc; } }
        public static Icon[] icons
        {
            get
            {
                Icon[] animated = new Icon[8];
                animated[0] = Resources.a0;
                animated[1] = Resources.a1;
                animated[2] = Resources.a2;
                animated[3] = Resources.a3;
                animated[4] = Resources.a4;
                animated[5] = Resources.a5;
                animated[6] = Resources.a6;
                animated[7] = Resources.a7;
                return animated;
            }
        }


        //PNGs
        public static Bitmap logo { get { return Resources.logo; } }
        public static Bitmap image { get { return Resources.image; } }
        public static Bitmap button_resume { get { return Resources.button_resume; } }
        public static Bitmap button_resume_x { get { return Resources.button_resume_x; } }
        public static Bitmap button_shutdown { get { return Resources.button_shutdown; } }
        public static Bitmap button_shutdown_x { get { return Resources.button_shutdown_x; } }
        public static Bitmap error { get { return Resources.error; } }
        public static Bitmap initialize { get { return Resources.initialize; } }
        public static Bitmap ready { get { return Resources.ready; } }

    }
}
