using System.Text;

namespace CubArt.Domain.Exceptions
{
    public static class ExceptionExtensions
    {
        public static string GetFullExceptionDetails(this Exception exception)
        {
            var sb = new StringBuilder();
            var currentException = exception;
            var level = 0;

            while (currentException != null)
            {
                sb.AppendLine($"Level {level}: {currentException.GetType().Name}");
                sb.AppendLine($"Message: {currentException.Message}");
                sb.AppendLine($"StackTrace: {currentException.StackTrace}");
                sb.AppendLine("---");

                currentException = currentException.InnerException;
                level++;
            }

            return sb.ToString();
        }
    }
}
