using Volo.Abp.Modularity;
using FamiyChat.Domain.Shared;

namespace FamiyChat.Application.Contracts;

[DependsOn(
    typeof(FamiyChatDomainSharedModule)
)]
public class FamiyChatApplicationContractsModule : AbpModule
{
}
