using System.Windows.Forms;

namespace BasicFacebookFeatures
{
    public interface ILogger
    {
        ILoggerNotifier LoggerNotifier { get; set; }
    }
}