namespace ECOLAB.IOT.SiteManagement.Data.Entity.CertificationInfos
{
    using AutoMapper;
    using ECOLAB.IOT.SiteManagement.Data.Dto.CertificationInfos;

    public class CertificationDtoProfile : Profile
    {
        public CertificationDtoProfile()
        {
            CreateMap<CertificationInfo, InsertCertificationInfoDto>()
                .ForMember(
                    dest => dest.CertificationName,
                    opt => opt.MapFrom(src => src.CertificationName)
                )
                 .ForMember(
                    dest => dest.CertificationDesc,
                    opt => opt.MapFrom(src => src.CertificationDesc)
                )
                  .ForMember(
                    dest => dest.CertificationTokenExpirationUtcTime,
                    opt => opt.MapFrom(src => src.CertificationTokenExpirationUtcTime)
                );
                //  .ForMember(
                //    dest => dest.Permissions,
                //    opt => opt.MapFrom(src => src.Permissions)
                //);

            CreateMap<CertificationInfo, CertificationInfoDto>()
                .ForMember(
                    dest => dest.CertificationName,
                    opt => opt.MapFrom(src => src.CertificationName)
                )
                 .ForMember(
                    dest => dest.CertificationDesc,
                    opt => opt.MapFrom(src => src.CertificationDesc)
                )
                  .ForMember(
                    dest => dest.CertificationTokenExpirationUtcTime,
                    opt => opt.MapFrom(src => src.CertificationTokenExpirationUtcTime)
                )
                   .ForMember(
                    dest => dest.Permissions,
                    opt => opt.MapFrom(src => src.Permissions)
                );

            CreateMap<CertificationInfo, RefreshCertificationInfoDto>()
                 .ForMember(
                   dest => dest.CertificationTokenExpirationUtcTime,
                   opt => opt.MapFrom(src => src.CertificationTokenExpirationUtcTime)
               );

            CreateMap<CertificationInfo, UpdateCertificationInfoDto>()
              .ForMember(
                  dest => dest.CertificationDesc,
                  opt => opt.MapFrom(src => src.CertificationDesc)
              )
                .ForMember(
                  dest => dest.CertificationTokenExpirationUtcTime,
                  opt => opt.MapFrom(src => src.CertificationTokenExpirationUtcTime)
              );
        }
    }
}
