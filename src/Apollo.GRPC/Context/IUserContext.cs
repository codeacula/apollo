using Apollo.Domain.People.Models;

namespace Apollo.GRPC.Context;

public interface IUserContext
{
  Person? Person { get; set; }
}
