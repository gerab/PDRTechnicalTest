using System;
using PDR.PatientBooking.Service.Enums;

namespace PDR.PatientBooking.Service.Exceptions
{
    public class DomainException : Exception
    {
        private static readonly ErrorCode DefaultErrorCode = ErrorCode.ServerError;

        public DomainException()
        {
            ErrorCode = DefaultErrorCode;
        }

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = DefaultErrorCode;
        }

        public DomainException(ErrorCode errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode { get; set; }

        public static DomainException BadRequest(string message)
        {
            return GetExceptionOfType(ErrorCode.BadRequest, message);
        }

        public static DomainException NotFound(string message)
        {
            return GetExceptionOfType(ErrorCode.NotFound, message);
        }

        private static DomainException GetExceptionOfType(ErrorCode errorCode, string message, Exception innerException = null)
        {
            var result = new DomainException(message, innerException)
            {
                ErrorCode = errorCode,
            };

            return result;
        }
    }
}