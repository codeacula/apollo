using System.ServiceModel;

using Apollo.GRPC.Attributes;
using Apollo.GRPC.Contracts;

namespace Apollo.GRPC.Service;

[ServiceContract]
public interface IConfigurationGrpcService
{
  [OperationContract]
  [RequireSuperAdmin]
  Task<GrpcResult<ConfigurationDTO>> GetConfigurationAsync();

  [OperationContract]
  [RequireSuperAdmin]
  Task<GrpcResult<ConfigurationDTO>> UpdateAiConfigurationAsync(UpdateAiConfigurationRequest request);

  [OperationContract]
  [RequireSuperAdmin]
  Task<GrpcResult<ConfigurationDTO>> UpdateDiscordConfigurationAsync(UpdateDiscordConfigurationRequest request);

  [OperationContract]
  [RequireSuperAdmin]
  Task<GrpcResult<ConfigurationDTO>> UpdateSuperAdminConfigurationAsync(UpdateSuperAdminConfigurationRequest request);

  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<ConfigurationStatusDTO>> GetConfigurationStatusAsync();
}
