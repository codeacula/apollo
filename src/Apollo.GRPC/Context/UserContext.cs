using Apollo.Domain.People.Models;

namespace Apollo.GRPC.Context;

public sealed class UserContext : IUserContext
{
  public Person? Person { get; set; }
}
