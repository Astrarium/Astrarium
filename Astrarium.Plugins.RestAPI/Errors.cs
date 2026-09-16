using System;
using System.Net;

namespace Astrarium.Plugins.RestAPI
{
    internal abstract class BaseError :Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        internal BaseError(string message, HttpStatusCode statusCode)
         : base(message)
        {
            StatusCode = statusCode;
        }
    }

    internal class ObjectNotFoundError : BaseError
    {
        public ObjectNotFoundError(string objectName, string objectType) 
            : base($"Celestial object with name `{objectName}` {(string.IsNullOrEmpty(objectType) ? "" : $"of type `{objectType}`")}not found.", HttpStatusCode.NotFound)
        {
        
        }
    }

    internal class UnknownParameterValueError : BaseError
    {
        public UnknownParameterValueError(string message)
            : base(message, HttpStatusCode.BadRequest)
        {

        }
    }

    internal class ParameterValueIsOutOfRangeError : BaseError
    {
        public ParameterValueIsOutOfRangeError(string message)
            : base(message, HttpStatusCode.BadRequest)
        {

        }
    }
}
