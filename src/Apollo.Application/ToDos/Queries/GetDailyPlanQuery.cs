using Apollo.Application.ToDos.Models;
using Apollo.Domain.People.ValueObjects;
using FluentResults;
using MediatR;

namespace Apollo.Application.ToDos.Queries;

public sealed record GetDailyPlanQuery(PersonId PersonId) : IRequest<Result<DailyPlan>>;
