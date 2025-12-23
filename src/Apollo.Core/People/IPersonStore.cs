using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.People;

public interface IPersonStore
{
  Task<Result<Person>> CreateAsync(PersonId id, Username username, CancellationToken cancellationToken = default);
  Task<Result<HasAccess>> GetAccessAsync(PersonId id, CancellationToken cancellationToken = default);
  Task<Result<Person>> GetAsync(PersonId id, CancellationToken cancellationToken = default);
  Task<Result<Person>> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);
  Task<Result> GrantAccessAsync(PersonId id, CancellationToken cancellationToken = default);
  Task<Result> SetTimeZoneAsync(PersonId id, PersonTimeZoneId timeZoneId, CancellationToken cancellationToken = default);
  Task<Result> AddNotificationChannelAsync(Person person, NotificationChannel channel, CancellationToken cancellationToken = default);
  Task<Result> RemoveNotificationChannelAsync(Person person, NotificationChannel channel, CancellationToken cancellationToken = default);
  Task<Result> ToggleNotificationChannelAsync(Person person, NotificationChannel channel, CancellationToken cancellationToken = default);
  Task<Result> EnsureNotificationChannelAsync(Person person, NotificationChannel channel, CancellationToken cancellationToken = default);
}
