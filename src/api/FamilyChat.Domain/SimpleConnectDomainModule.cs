using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;
using FamiyChat.Domain.Shared;

namespace FamiyChat.Domain;

[DependsOn(
    typeof(FamiyChatDomainSharedModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class FamiyChatDomainModule : AbpModule
{
}
