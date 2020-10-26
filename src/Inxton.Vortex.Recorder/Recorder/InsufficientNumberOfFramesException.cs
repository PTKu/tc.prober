using System;
using System.Linq;
using System.Runtime.Serialization;


namespace Tc.Prober.Recorder
{
    public class InsufficientNumberOfFramesException : Exception
    {
        public InsufficientNumberOfFramesException()
        {
        }

        public InsufficientNumberOfFramesException(string message) : base(message)
        {
        }

        public InsufficientNumberOfFramesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InsufficientNumberOfFramesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
