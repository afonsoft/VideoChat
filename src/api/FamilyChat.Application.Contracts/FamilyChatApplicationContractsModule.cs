using Volo.Abp.Modularity;
using FamilyChat.Domain.Shared;

namespace FamilyChat.Application.Contracts;

[DependsOn(
    typeof(FamilyChatDomainSharedModule)
)]
public class FamilyChatApplicationContractsModule : AbpModule
{
}
