using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Vision_Align
{
    public enum AlarmCode
    {
        None = 0,                      // No Alarm

        PmcAlarm = 1001,               // PMC Alarm
        PreAlignRetryOver = 1002,      // Pre Align Retry Over
        ContactAlignRetryOver = 1003,  // Contact Align Retry Over

        CameraCommError = 1008,        // Camera Communication Error
        LightCommError = 1009,         // Light Communication Error
        UvwStageCommError = 1010,      // UVW Stage Communication Error
        CameraMotionCommError = 1011,  // Camera Motion Communication Error
        UvwStageMoveError = 1012,      // UVW Stage Move Command Error
        MarkTeachingError = 1013,      // Mark Teaching Error
        BoardInvertError = 1014,       // Board Invert Error
        InterfaceTimeout = 1015,       // Interface Timeout Error
        UvwStageOverTravel = 1016,     // UVW Stage Over Travel Error
        CalibrationError = 1017,       // Calibration Error
        StorageLowError  = 1018,       // Drive Free Space Insufficient
        Unknown = 9999,
    }

    public class AlarmRecord
    {
        public DateTime Time { get; set; }
        public AlarmCode Code { get; set; }
        public string Description { get; set; }
        public string Context { get; set; }
        public string Type { get; set; } // "SET" or "CLEAR"
    }

    public class ClsAlarmManager
    {
        private AlarmCode _currentAlarm = AlarmCode.None;
        private bool _alarmClearReq = false;

        private static readonly Dictionary<AlarmCode, string> AlarmDescriptions = new Dictionary<AlarmCode, string>
        {
            { AlarmCode.None,                   "No Alarm" },
            { AlarmCode.PmcAlarm,               "PMC Alarm" },
            { AlarmCode.PreAlignRetryOver,      "Pre Align Retry Over" },
            { AlarmCode.ContactAlignRetryOver,  "Contact Align Retry Over" },
            { AlarmCode.CameraCommError,        "Camera Communication Error" },
            { AlarmCode.LightCommError,         "Light Communication Error" },
            { AlarmCode.UvwStageCommError,      "UVW Stage Communication Error" },
            { AlarmCode.CameraMotionCommError,  "Camera Motion Communication Error" },
            { AlarmCode.UvwStageMoveError,      "UVW Stage Move Command Error" },
            { AlarmCode.MarkTeachingError,      "Mark Teaching Error" },
            { AlarmCode.BoardInvertError,       "Board Invert Error" },
            { AlarmCode.InterfaceTimeout,       "Interface Timeout Error" },
            { AlarmCode.UvwStageOverTravel,     "UVW Stage Over Travel Error" },
            { AlarmCode.CalibrationError,       "Calibration Error" },
            { AlarmCode.StorageLowError,        "Drive Free Space Insufficient" },
            { AlarmCode.Unknown,                "Unknown Alarm" },
        };

        public AlarmCode CurrentAlarm
        {
            get { return _currentAlarm; }
        }

        public bool IsAlarm
        {
            get { return _currentAlarm != AlarmCode.None; }
        }

        public bool AlarmClearReq
        {
            get { return _alarmClearReq; }
            set { _alarmClearReq = value; }
        }

        public void SetAlarm(AlarmCode code, string context = "")
        {
            _currentAlarm = code;

            if (code == AlarmCode.None)
                return;

            string desc = GetDescription(code);
            string logMsg = string.Format("[SET] [{0}] [{1}] {2}", (int)code, code, string.IsNullOrEmpty(context) ? desc : context);

            try
            {
                Global.logger[LogType.ALARM].Write(logMsg);
            }
            catch { }
        }

        public void ClearAlarm()
        {
            AlarmCode prevCode = _currentAlarm;
            _currentAlarm = AlarmCode.None;
            _alarmClearReq = false;

            string logMsg = string.Format("[CLEAR] [{0}] [{1}] Alarm Cleared (was: {2})", (int)AlarmCode.None, AlarmCode.None, prevCode);

            try
            {
                Global.logger[LogType.ALARM].Write(logMsg);
            }
            catch { }
        }

        public static string GetDescription(AlarmCode code)
        {
            string desc;
            if (AlarmDescriptions.TryGetValue(code, out desc))
                return desc;
            return "Unknown Alarm";
        }

        public List<AlarmRecord> GetAlarmHistory(DateTime date)
        {
            var records = new List<AlarmRecord>();

            string logDir = Path.Combine(Environment.CurrentDirectory, "LOG", "ALARM");
            string fileName = date.ToString("yyyyMMdd") + ".log";
            string filePath = Path.Combine(logDir, fileName);

            if (!File.Exists(filePath))
                return records;

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var record = ParseLogLine(line);
                    if (record != null)
                        records.Add(record);
                }
            }
            catch { }

            return records;
        }

        private AlarmRecord ParseLogLine(string line)
        {
            try
            {
                // Format: [ 2026-02-16 10:30:15:123 ] [SET] [1001] [PmcAlarm] PMC Alarm
                // NLog format: [ {datetime} ] {message}
                // message format: [SET/CLEAR] [code] [name] description

                int firstBracketEnd = line.IndexOf(']');
                if (firstBracketEnd < 0)
                    return null;

                // Parse datetime from "[ 2026-02-16 10:30:15:123 ]"
                string dateStr = line.Substring(2, firstBracketEnd - 2).Trim();
                DateTime time;
                if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss:fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
                    return null;

                string remaining = line.Substring(firstBracketEnd + 1).Trim();

                // Parse [SET] or [CLEAR]
                int typeStart = remaining.IndexOf('[');
                int typeEnd = remaining.IndexOf(']');
                if (typeStart < 0 || typeEnd < 0)
                    return null;
                string type = remaining.Substring(typeStart + 1, typeEnd - typeStart - 1);
                remaining = remaining.Substring(typeEnd + 1).Trim();

                // Parse [code]
                int codeStart = remaining.IndexOf('[');
                int codeEnd = remaining.IndexOf(']');
                if (codeStart < 0 || codeEnd < 0)
                    return null;
                string codeStr = remaining.Substring(codeStart + 1, codeEnd - codeStart - 1);
                int codeInt;
                int.TryParse(codeStr, out codeInt);
                remaining = remaining.Substring(codeEnd + 1).Trim();

                // Parse [name]
                int nameStart = remaining.IndexOf('[');
                int nameEnd = remaining.IndexOf(']');
                string name = "";
                string context = "";
                if (nameStart >= 0 && nameEnd > nameStart)
                {
                    name = remaining.Substring(nameStart + 1, nameEnd - nameStart - 1);
                    context = remaining.Substring(nameEnd + 1).Trim();
                }

                AlarmCode alarmCode;
                try { alarmCode = (AlarmCode)codeInt; } catch { alarmCode = AlarmCode.Unknown; }

                return new AlarmRecord
                {
                    Time = time,
                    Code = alarmCode,
                    Description = GetDescription(alarmCode),
                    Context = context,
                    Type = type,
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
