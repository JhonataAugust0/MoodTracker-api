namespace MoodTracker_back.Application.Common.Abstractions;

public interface ICommand<out TResult>
{
}

public interface ICommandHandler<in TCommand, TResult> 
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
