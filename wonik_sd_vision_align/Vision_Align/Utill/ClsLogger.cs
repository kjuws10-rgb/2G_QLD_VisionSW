using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;
using NLog.Config;
using NLog.Targets;

namespace Vision_Align
{
    public class ClsLogger
    {
        private readonly NLog.Logger _Logger;

        // NLog.XML에 설정된 Type
        [Obsolete]
        public ClsLogger(string _logName, string _fileFormat)
        {
            var logFactory = new LogFactory(SetRule(_logName, _fileFormat));
            _Logger = logFactory.GetLogger(_logName);
        }
        // Log Write
        public void Write(string _value)
        {
            _Logger.Trace(_value);
        }

        private LoggingConfiguration SetRule(string _logName, string _fileFormat)
        {
            var _config = new LoggingConfiguration();
            var _logfile = new FileTarget();
            _logfile.Name = _logName;
            _logfile.FileName = _fileFormat;
            _logfile.Layout = "[ ${date:format=yyyy-MM-dd HH\\:mm\\:ss\\:fff} ] ${message}";
            _logfile.KeepFileOpen = false;
            _config.AddTarget(_logfile.Name , _logfile);

            //Trace - 0 , Debug - 1 , Info - 2 , Warn - 3 , Error - 4 , Fatal - 5 , Off - 6
            _config.AddRule(LogLevel.Trace, LogLevel.Trace, _logfile);

            return _config;
        }

    }
}
