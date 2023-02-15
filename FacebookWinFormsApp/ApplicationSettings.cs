using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace BasicFacebookFeatures.WithSingltonAppSettings
{
    public class ApplicationSettings
    {
        private static readonly string sr_FileName;

        static ApplicationSettings()
        {
            sr_FileName = Application.ExecutablePath + ".settings.xml";
        }

        /// <summary>
        ///  private CTOR as part as the singleton pattern
        /// </summary>
        private ApplicationSettings()
        {
        }

        /// <summary>
        /// Static reference to the single instance
        /// </summary>
        private static ApplicationSettings s_This;

        /// <summary>
        /// Public static access point to the single instance (including JIT creation)
        /// </summary>
        public static ApplicationSettings Instance
        {
            get
            {
                if (s_This == null)
                {
                    s_This = ApplicationSettings.FromFileOrDefault();
                }

                return s_This;
            }
        }

        /// C# 3.0 feature: Automatic Properties
        public bool AutoLogin { get; set; }

        public FormWindowState LastWindowState { get; set; }

        public Point LastWindowLocation { get; set; }

        public string AccessToken { get; set; }

        public void Save()
        {
            using (FileStream stream = new FileStream(sr_FileName, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
                serializer.Serialize(stream, this);
            }
        }

        public static ApplicationSettings FromFileOrDefault()
        {
            ApplicationSettings loadedThis = null;

            if (File.Exists(sr_FileName))
            {
                using (FileStream stream = new FileStream(sr_FileName, FileMode.OpenOrCreate))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
                    loadedThis = (ApplicationSettings)serializer.Deserialize(stream);
                }
            }
            else
            {
                /// C# 3.0 feature: Object Initializer
                loadedThis = new ApplicationSettings()
                {
                    AutoLogin = false,
                    LastWindowState = FormWindowState.Normal,
                    LastWindowLocation = new Point(0, 0)
                };
            }

            return loadedThis;
        }
    }
}
