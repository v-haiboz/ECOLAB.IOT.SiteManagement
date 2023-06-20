namespace ECOLAB.IOT.SiteManagement.Service
{
    using AutoMapper;
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Dto.CertificationInfos;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using ECOLAB.IOT.SiteManagement.Data.Entity.CertificationInfos;
    using ECOLAB.IOT.SiteManagement.Repository;
    using System;
    using System.Collections.Generic;

    public interface ICertificationService
    {

        public Task<CertificationInfoDto> Insert(InsertCertificationInfoDto insertCertificationInfoDto);
        public List<CertificationInfoDto> GetCertificationInfos(string? certificationName, string? certificationToken, int pageIndex, int pageSize, out int total);

        public Task<CertificationInfoDto>? Refresh(string certificationName, RefreshCertificationInfoDto refreshCertificationInfoDto);

        public Task<bool> Delete(string certificationName);

        public Task<CertificationInfoDto> Update(string certificationName,UpdateCertificationInfoDto updateCertificationInfoDto);

        public Task<CertificationInfoDto> GetCertificationInfoByCertificationName(string certificationName);

    }

    public class CertificationService : ICertificationService
    {
        private readonly ICertificationRepository _certificationRepository;
        public readonly IMapper _mapper;
        public const string CertificationPrefix = "certification_";
        private readonly IMemoryCacheService _memoryCacheService;

        public CertificationService(ICertificationRepository certificationRepository, IMapper mapper, IMemoryCacheService memoryCacheService)
        {
            _certificationRepository = certificationRepository;
            _mapper = mapper;
            _memoryCacheService = memoryCacheService;
        }

        public async Task<CertificationInfoDto> Insert(InsertCertificationInfoDto insertCertificationInfoDto)
        {
            var _mappedCertificationInfo = _mapper.Map<CertificationInfo>(insertCertificationInfoDto);
            _mappedCertificationInfo.CertificationToken = Guid.NewGuid().ToString();
            var result=_certificationRepository.Insert(_mappedCertificationInfo);
            var _mappedResult = _mapper.Map<CertificationInfoDto>(result);
            return await Task.FromResult(_mappedResult);
        }

        public List<CertificationInfoDto> GetCertificationInfos(string? certificationName, string? certificationToken, int pageIndex, int pageSize, out int total)
        {
            var list = _certificationRepository.GetCertificationInfos(out total,certificationName, certificationToken, pageIndex, pageSize);
            var _mappedResult = _mapper.Map<List<CertificationInfoDto>>(list);
            return _mappedResult;
        }

        public async Task<CertificationInfoDto>? Refresh(string certificationName, RefreshCertificationInfoDto refreshCertificationInfoDto)
        {
            var _mappedCertificationInfo = _mapper.Map<CertificationInfo>(refreshCertificationInfoDto);
            _mappedCertificationInfo.CertificationName = certificationName;
            _mappedCertificationInfo.CertificationToken = Guid.NewGuid().ToString();
            var result = _certificationRepository.Refresh(_mappedCertificationInfo);
            if (result != null && !string.IsNullOrEmpty(result.CertificationToken))
            {
                _memoryCacheService.RemoveValue(result.CertificationToken.AddPrefix(CertificationPrefix));
            }
          
            var _mappedResult = _mapper.Map<CertificationInfoDto>(result);
            return await Task.FromResult(_mappedResult);
        }

        public async Task<bool> Delete(string certificationName)
        {
            var item = _certificationRepository.GetCertificationInfoByCertificationName(certificationName);
            if (item != null && !string.IsNullOrEmpty(item.CertificationToken))
            {
                _memoryCacheService.RemoveValue(item.CertificationToken.AddPrefix(CertificationPrefix));
            }

            var result = _certificationRepository.Delete(certificationName);
            return await Task.FromResult(result);
        }

        public async Task<CertificationInfoDto> Update(string certificationName,UpdateCertificationInfoDto updateCertificationInfoDto)
        {
            var _mappedCertificationInfo = _mapper.Map<CertificationInfo>(updateCertificationInfoDto);
            _mappedCertificationInfo.CertificationName = certificationName;
            _mappedCertificationInfo.CertificationToken = Guid.NewGuid().ToString();
            var result = _certificationRepository.Update(_mappedCertificationInfo);
            if (result != null && !string.IsNullOrEmpty(result.CertificationToken))
            {
                _memoryCacheService.RemoveValue(result.CertificationToken.AddPrefix(CertificationPrefix));
            }
            var _mappedResult = _mapper.Map<CertificationInfoDto>(result);
            return await Task.FromResult(_mappedResult);
        }

        public async Task<CertificationInfoDto> GetCertificationInfoByCertificationName(string certificationName)
        {
            var item =_certificationRepository.GetCertificationInfoByCertificationName(certificationName);
            var _mappedResult = _mapper.Map<CertificationInfoDto>(item);
            return  await Task.FromResult(_mappedResult);
        }
    }
}
