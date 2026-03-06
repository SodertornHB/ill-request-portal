
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using Microsoft.Extensions.Logging;
using Moq;
using System;
namespace IllRequestPortal.Test
{ 
    public static partial class LoggerMockExtensions
    {
        public static void VerifyLoggingExact<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel, string str)
        {
            loggerMock.Verify(x => x.Log(logLevel,
                                         It.IsAny<EventId>(),
                                         It.Is<It.IsAnyType>((object message, Type t) =>
                                         message.ToString() == str),
                                         It.IsAny<Exception>(),
                                         (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }
        
        public static void VerifyLoggingContains<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel, string str)
        {
            loggerMock.Verify(x => x.Log(logLevel,
                                         It.IsAny<EventId>(),
                                         It.Is<It.IsAnyType>((object message, Type t) =>
                                         message.ToString().Contains(str)),
                                         It.IsAny<Exception>(),
                                         (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }
    }
}
