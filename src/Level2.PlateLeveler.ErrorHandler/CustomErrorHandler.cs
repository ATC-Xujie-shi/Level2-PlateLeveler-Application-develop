using System;
using Level2.PlateLeveler.DataFunction;

namespace Level2.PlateLeveler.ErrorHandler {
    public class CustomErrorException : Exception {
        public CustomErrorException(string message, string method)
        : base(message) {
            Logging.SendMessage(message, method, LoggerLevel.Error, this.GetType());
            Console.WriteLine(message);
        }
        public CustomErrorException() {
        }
        public CustomErrorException(string message) : base(message) {
        }
        public CustomErrorException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
