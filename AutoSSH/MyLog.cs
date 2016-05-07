using NLog;
using NLog.Config;
using NLog.Targets;

namespace AutoSSH
{
    public class MyLog
    {
        public static void initconfig()
        {
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties
            consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";
            fileTarget.FileName = @"${basedir}/Logs/${date:format=MM-dd-yyyy}-AutoSSH.log";
            fileTarget.ArchiveAboveSize = 104857600;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            fileTarget.CreateDirs = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.EnableArchiveFileCompression = true;
            fileTarget.Layout = @"${date:format=MM-dd-yyyy-HH\:mm\:ss} ${logger} ${message}";

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            // Example usage
            //Logger logger = LogManager.GetLogger("Example");
            //logger.Trace("trace log message");
            //logger.Debug("debug log message");
            //logger.Info("info log message");
            //logger.Warn("warn log message");
            //logger.Error("error log message");
            //logger.Fatal("fatal log message");
        }
    }
}