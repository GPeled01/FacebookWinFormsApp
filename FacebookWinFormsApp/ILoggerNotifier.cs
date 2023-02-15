using System;

namespace BasicFacebookFeatures
{
    public interface ILoggerNotifier
    {
        event Action<string> m_ReportLoggers;
    }
}