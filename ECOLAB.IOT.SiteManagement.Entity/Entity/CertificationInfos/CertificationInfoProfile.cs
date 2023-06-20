namespace ECOLAB.IOT.SiteManagement.Data.Entity.CertificationInfos
{
    using AutoMapper;
    using ECOLAB.IOT.SiteManagement.Data.Dto.CertificationInfos;

    public class CertificationInfoProfile : Profile
    {
        public CertificationInfoProfile()
        {
            CreateMap<InsertCertificationInfoDto, CertificationInfo>()
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
                //   .ForMember(
                //    dest => dest.Permissions,
                //    opt => opt.MapFrom(src => src.Permissions)
                //);

            CreateMap<RefreshCertificationInfoDto, CertificationInfo>()
                .ForMember(
                  dest => dest.CertificationTokenExpirationUtcTime,
                  opt => opt.MapFrom(src => src.CertificationTokenExpirationUtcTime)
              );

            CreateMap<UpdateCertificationInfoDto, CertificationInfo>()
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
