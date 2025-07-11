namespace Hyprx;

/// <summary>
/// Options for environment variable expansion.
/// </summary>
public class EnvironmentException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentException"/> class.
    /// </summary>
    public EnvironmentException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public EnvironmentException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception,
    /// or a null reference if no inner exception is specified.</param>
    public EnvironmentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}