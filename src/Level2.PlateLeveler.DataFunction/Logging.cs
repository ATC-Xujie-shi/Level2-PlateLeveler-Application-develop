using System;
using System.Reflection;
using log4net;

namespace Level2.PlateLeveler.DataFunction {
    public enum LoggerLevel {
        Error, Warning, Info, Debug
    }

    public static class Logging {
        public static void SendMessage(string message, string method, LoggerLevel level, Type type) {
            _ = log4net.Config.XmlConfigurator.Configure();
            var logger = LogManager.GetLogger(type);

            message += " - method: " + method + " ";
            switch (level) {
                case LoggerLevel.Debug:
                    logger.Debug(message);
                    break;
                case LoggerLevel.Error:
                    logger.Error(message);
                    break;
                case LoggerLevel.Warning:
                    logger.Warn(message);
                    break;
                default:
                    logger.Info(message);
                    break;
            }
        }

        public static void SendErrorMessage(string method, Exception ex, Type type) {
            _ = log4net.Config.XmlConfigurator.Configure();
            var logger = LogManager.GetLogger(type);

            var message = "Function: " + method + " ";
            logger.Error(message, ex);
        }

        public static void SendErrorMessage(string telegram, string method, Exception ex, Type type) {
            _ = log4net.Config.XmlConfigurator.Configure();
            var logger = LogManager.GetLogger(type);

            var message = Environment.NewLine + "Telegram: " + telegram + "Namespace: " + type.Namespace + " Function: " + method + " ";

            logger.Error(message, ex);
        }

        public static void SendTelegramLog(string message) {
            _ = log4net.Config.XmlConfigurator.Configure();
            var logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            logger.Info(message);
        }

        public static void SendTelegramLog() {
            _ = log4net.Config.XmlConfigurator.Configure();
            var logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            logger.Info("");
        }
    }
}
