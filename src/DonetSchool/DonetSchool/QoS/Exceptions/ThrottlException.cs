using System;
using System.Net;

namespace DonetSchool.QoS.Exceptions
{
    public class ThrottlException : Exception
    {
        public ThrottlException() : this((int)HttpStatusCode.TooManyRequests)
        {
        }

        public ThrottlException(int code) : this(code, "Please request again later")
        {
        }

        public ThrottlException(int code, string message) : base(message)
        {
            Code = code;
        }

        public int Code { get; set; }
    }
}