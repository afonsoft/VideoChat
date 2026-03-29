using Volo.Abp.Modularity;
using Volo.Abp.AutoMapper;
using FamilyChat.Application.Contracts;

namespace FamilyChat.Application;

[DependsOn(
    typeof(FamilyChatApplicationContractsModule),
    typeof(AbpAutoMapperModule)
)]
public class FamilyChatApplicationModule : AbpModule
{
}
