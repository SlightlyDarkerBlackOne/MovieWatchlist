using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Begin transaction for {RequestType}", typeof(TRequest).Name);

        var response = await next();

        var changes = await _unitOfWork.SaveChangesAsync();

        if (changes > 0)
        {
            _logger.LogDebug("Committed {Changes} changes for {RequestType}",
                changes, typeof(TRequest).Name);
        }

        return response;
    }
}

