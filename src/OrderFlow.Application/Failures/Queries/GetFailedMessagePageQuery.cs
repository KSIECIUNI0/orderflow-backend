using MediatR;
using OrderFlow.Application.Failures.DTOs;

namespace OrderFlow.Application.Failures.Queries;

public record GetFailedMessagePageQuery(
    int PageSize,
    string? ContinuationToken)
    : IRequest<FailedMessagePage>;