using System;
using System.Collections.Generic;
using System.Text;

namespace Snacks.Entity.Core.Exceptions
{
    public class NoKeyColumnException : Exception
    {
        public NoKeyColumnException() : base()
        {
            
        }

        public NoKeyColumnException(string message) : base(message)
        {
            
        }

        public NoKeyColumnException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}
