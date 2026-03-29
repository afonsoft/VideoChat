using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;
using FamilyChat.Domain.Shared;

namespace FamilyChat.Domain;

[DependsOn(
    typeof(FamilyChatDomainSharedModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class FamilyChatDomainModule : AbpModule
{
}
