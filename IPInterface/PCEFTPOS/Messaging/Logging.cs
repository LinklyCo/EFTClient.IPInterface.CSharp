using System;

namespace PCEFTPOS.EFTClient.IPInterface
{
    public class TraceRecord
    {
        public TraceRecord()
        {
            Message = "";
            Data = null;
            Level = LogLevel.Off;
        }

        public void Set(string message)
        {
            Message = message;
        }

        public void Set(string message, object data)
        {
            Message = message;
            Data = data;
        }

        public void Set(string message, object data, Exception exception)
        {
            Message = message;
            Data = data;
            Exception = exception;
        }

        public void Set(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// OFF – The OFF Level has the highest possible rank and is intended to turn off logging.
        /// FATAL – The FATAL level designates very severe error events that will presumably lead the application to abort. 
        /// ERROR – The ERROR level designates error events that might still allow the application to continue running.
        /// WARN – The WARN level designates potentially harmful situations.
        /// INFO – The INFO level designates informational messages that highlight the progress of the application at coarse-grained level.
        /// DEBUG – The DEBUG Level designates fine-grained informational events that are most useful to debug an application.
        /// TRACE – The TRACE Level designates finer-grained informational events than the DEBUG
        /// ALL -The ALL Level has the lowest possible rank and is intended to turn on all logging. 
        /// </summary>
        public LogLevel Level { get; set; }

        public string Message { get; set; }
        public object Data { get; set; }
        public Exception Exception { get; set; }
    }
}
