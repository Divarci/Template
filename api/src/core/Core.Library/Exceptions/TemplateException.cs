namespace Core.Library.Exceptions;

public sealed class TemplateException(
    string assemblyName,
    string className,
    string methodName,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public string AssemblyName { get; } = assemblyName;

    public string ClassName { get; } = className;

    public string MethodName { get; } = methodName;
}
