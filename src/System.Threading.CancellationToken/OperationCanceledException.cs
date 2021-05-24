//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Threading
{
    /// <summary>
    /// The exception that is thrown when a CancellationToken is raised.
    /// </summary>
    [Serializable]
    public class OperationCanceledException
        : SystemException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationCanceledException"/> class.
        /// </summary>
        public OperationCanceledException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the OperationCanceledException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OperationCanceledException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OperationCanceledException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception.</param>
        public OperationCanceledException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
