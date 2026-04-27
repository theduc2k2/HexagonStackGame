namespace Project.Core.Common
{
    public readonly struct Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }

        private Result(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true, string.Empty);

        public static Result Failure(string error) => new Result(false, error);
    }
}
