using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SML.Exceptions {
    public class InvalidFileFormatException : Exception {
        public InvalidFileFormatException(string message) : base(message) { }
    }

}
