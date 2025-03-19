using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SML.Exceptions {
    public class InvalidMatchException : Exception {
        public InvalidMatchException(string message) : base(message) { }
    }

}
