using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail
{
    public class Messages //<<-- 1.2
    {
        #region Neutral/system messages
        /// <summary>
        /// Marker class to continue processing
        /// </summary>
        public class ContinueProcessing
        {

        }
        #endregion

        #region Success Messages
        /// <summary>
        /// Base class for signalling that user input was valid
        /// </summary>
        public class InputSuccess
        {
            public InputSuccess(string reason)
            {
                Reason = reason;
            }

            public object Reason { get; private set; }
        }
        #endregion

        #region Error Messages
        /// <summary>
        /// Base class for signalling that user input was invalid.
        /// </summary>
        public class InputError
        {
            public InputError(string reason)
            {
                Reason = reason;
            }

            public object Reason { get; private set; }
        }

        /// <summary>
        /// User provided blank input
        /// </summary>
        public class NullInputError : InputError
        {
            public NullInputError(string reason) : base(reason) { }
        }

        /// <summary>
        /// User provided invalid input (curretnly, input w/ odd # of characters.
        /// </summary>
        public class ValidationError : InputError
        {
            public ValidationError(string reason) : base(reason) { }
        }
        #endregion
    }
}
