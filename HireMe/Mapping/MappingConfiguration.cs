using HireMe.Consts;
using HireMe.Contracts.Account.Responses;
using HireMe.Contracts.Auth.Requests;
using HireMe.Models;
using Mapster;

namespace HireMe.Mapping
{
    public class MappingConfigurations : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {

            // You Make Manual Mapping  Property Name Is Different

            config.NewConfig<RegisterRequest, ApplicationUser>()
                .Map(dest => dest.UserName, src => src.Email);

            config.NewConfig<ApplicationUser, UserProfileResponse>()
                .Map(dest => dest.HasPassword, src => src.PasswordHash != null);

            config.NewConfig<ApplicationUser, UserProfileResponse>()
                .Map(dest => dest.ImageProfileUrl,
                src => string.IsNullOrEmpty(src.ImageProfile) ? null : $"/{ImageProfileSettings.StoredFolderName}/{src.ImageProfile}");
        }
    }
}
