
// --- Patch: ResultExtensions.cs ---
// Place this in your Result namespace (e.g., MyProject.Common.Results)

namespace ExxerAI.Domain
{
    public static class ResultErrors
    {
        public const string OperationCancelled = "Operation was cancelled by the user.";
    }

    public static class ResultExtensions
    {
        public static Result Cancelled()
        {
            return Result.WithFailure(ResultErrors.OperationCancelled);
        }

        public static Result<T> Cancelled<T>()
        {
            return Result<T>.WithFailure(ResultErrors.OperationCancelled);
        }

        public static bool IsCancelled(this Result result)
        {
            return result.Errors.Contains(ResultErrors.OperationCancelled);
        }

        public static bool IsCancelled<T>(this Result<T> result)
        {
            return result.Errors.Contains(ResultErrors.OperationCancelled);
        }
    }
}
