

using Prism.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace CloudDriveShell
{
    public class LoggerAdapter : ILoggerFacade
    {
        #region ILoggerFacade Members

        public void Log(string message, Category category, Priority priority)
        {
            Logger.Write(message, category.ToString(), (int)priority);
        }

        #endregion
    }
}
