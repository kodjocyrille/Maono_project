using MediatR;

namespace Maono.Application.Common.Interfaces;

/// <summary>
/// Marker interface for CQRS commands (write operations).
/// Commands pass through ValidationBehavior → TransactionBehavior → Handler.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;

/// <summary>
/// Marker interface for CQRS queries (read operations).
/// Queries pass through ValidationBehavior → LoggingBehavior → Handler.
/// No transaction is opened for queries.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
