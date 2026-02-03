using Apollo.Domain.People.Models;

namespace Apollo.GRPC.Context;

public class UserContext : IUserContext
{
  public Person? Person { get; set; }
}
