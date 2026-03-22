using System.ServiceModel;

using Apollo.GRPC.Contracts;

namespace Apollo.GRPC.Service;

[ServiceContract]
public interface IConfigurationGrpcService
{
  [OperationContract]
  Task<GrpcResult<ConfigurationDTO>> GetConfigurationAsync();

  [OperationContract]
  Task<GrpcResult<ConfigurationDTO>> UpdateAiConfigurationAsync(UpdateAiConfigurationRequest request);

  [OperationContract]
  Task<GrpcResult<ConfigurationDTO>> UpdateDiscordConfigurationAsync(UpdateDiscordConfigurationRequest request);

  [OperationContract]
  Task<GrpcResult<ConfigurationDTO>> UpdateSuperAdminConfigurationAsync(UpdateSuperAdminConfigurationRequest request);

  [OperationContract]
  Task<GrpcResult<ConfigurationStatusDTO>> GetConfigurationStatusAsync();
}
