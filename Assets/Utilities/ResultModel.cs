using System;

namespace ArtTest.Utilities
{
    public class Result<T>
    {
        private Action<T> onSuccess;
        private T cachedSuccessValue;

        private Action<Error> onError;
        private Error cachedErrorValue;

        private Action onComplete;
        private bool isCompleted;

        public void Resolve(T value)
        {
            cachedSuccessValue = value;
            onSuccess?.Invoke(value);

            isCompleted = true;
            onComplete?.Invoke();
        }

        public void Resolve(Error error)
        {
            cachedErrorValue = error;
            onError?.Invoke(error);

            isCompleted = true;
            onComplete?.Invoke();
        }

        public Result<T> Then(Action<T> onSuccess)
        {
            this.onSuccess += onSuccess;
            if (cachedSuccessValue != null)
            {
                onSuccess?.Invoke(cachedSuccessValue);
            }
            return this;
        }

        public Result<T> Catch(Action<Error> onError)
        {
            this.onError += onError;
            if (cachedErrorValue != null)
            {
                onError?.Invoke(cachedErrorValue);
            }
            return this;
        }

        public Result<T> Finally(Action onComplete)
        {
            this.onComplete += onComplete;
            if (isCompleted)
            {
                onComplete?.Invoke();
            }
            return this;
        }
    }

    public class Result
    {
        private Action onSuccess;
        private bool isSuccess;

        private Action<Error> onError;
        private Error cachedErrorValue;

        private Action onComplete;
        private bool isCompleted;

        public void Resolve()
        {
            isSuccess = true;
            onSuccess?.Invoke();

            isCompleted = true;
            onComplete?.Invoke();
        }

        public void Resolve(Error error)
        {
            cachedErrorValue = error;
            onError?.Invoke(error);

            isCompleted = true;
            onComplete?.Invoke();
        }

        public Result Then(Action onSuccess)
        {
            this.onSuccess += onSuccess;
            if (isSuccess)
            {
                onSuccess?.Invoke();
            }
            return this;
        }

        public Result Catch(Action<Error> onError)
        {
            this.onError += onError;
            if (cachedErrorValue != null)
            {
                onError?.Invoke(cachedErrorValue);
            }
            return this;
        }

        public Result Finally(Action onComplete)
        {
            this.onComplete += onComplete;
            if (isCompleted)
            {
                onComplete?.Invoke();
            }
            return this;
        }
    }

    public class Error
    {
        public string Message { get; private set; }
        public Error(string message)
        {
            Message = message;
        }
    }
}
