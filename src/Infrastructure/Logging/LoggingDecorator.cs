using MoodTracker_back.Application.Services;
using MoodTracker_back.Application.Common.Abstractions;

namespace MoodTracker_back.Infrastructure.Logging;

public class LoggingDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _handler;
    private readonly ILoggingService _logger;

    public LoggingDecorator(
        ICommandHandler<TCommand, TResult> handler,
        ILoggingService logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _logger.LogInformationAsync(
                "Executing command {CommandName} with data {@Command}",
                typeof(TCommand).Name,
                command);

            var result = await _handler.HandleAsync(command, cancellationToken);

            await _logger.LogInformationAsync(
                "Command {CommandName} executed successfully with result {@Result}",
                typeof(TCommand).Name,
                result);

            return result;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(
                ex,
                "Error executing command {CommandName} with data {@Command}",
                typeof(TCommand).Name,
                command);
            throw;
        }
    }
}