using CubArt.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Если нет валидаторов - пропускаем
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                var errorMessages = failures.Select(f => f.ErrorMessage).ToArray();

                if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure(errorMessages);
                }
                else
                {
                    // Получаем тип значения из Result<T>
                    var valueType = typeof(TResponse).GetGenericArguments()[0];

                    // Ищем метод Failure с параметрами string[] и Exception (последний необязательный)
                    var failureMethod = typeof(Result)
                        .GetMethods()
                        .First(m => m.Name == "Failure" &&
                                   m.IsGenericMethod &&
                                   m.GetParameters().Length == 2 &&
                                   m.GetParameters()[0].ParameterType == typeof(string[]) &&
                                   m.GetParameters()[1].ParameterType == typeof(Exception))
                        .MakeGenericMethod(valueType);

                    // Вызываем с двумя параметрами (exception = null)
                    return (TResponse)failureMethod.Invoke(null, new object[] { errorMessages, null });
                }
            }

            return await next();

        }
    }
}
