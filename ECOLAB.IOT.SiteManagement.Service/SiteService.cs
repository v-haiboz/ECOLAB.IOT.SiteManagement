namespace ECOLAB.IOT.SiteManagement.Service
{
    using Azure.Storage.Blobs;
    using ECOLAB.IOT.SiteManagement.Common;
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using ECOLAB.IOT.SiteManagement.Data.Exceptions;
    using ECOLAB.IOT.SiteManagement.Provider;
    using ECOLAB.IOT.SiteManagement.Repository;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface ISiteService
    {
        public Task<SiteResponseDto> Insert(SiteRequestDto siteRequestDto);

        public Task<SiteResponseDto> Update(string siteNo,SiteRequestDto siteRequestDto);

        public Task<SiteResponseDto> GetRegistryBySiteNo(string siteNo);
        public Task<List<SiteDeviceDto>> GeAllowListOfSiteDeviceBySiteNoAndDeviceNos(string siteNo,List<string> deviceNos);

        public List<SiteDeviceDetailInfoDto> GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(string siteNo,string gatewayNo, List<string> deviceNos);
    }

    public class SiteService : ISiteService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly ISiteRepository _siteRepository;
        private readonly ISiteRegistryRepository _siteRegistryRepository;
        private readonly IConfiguration _config;
        public SiteService(IStorageProvider storageProvider, ISiteRepository siteRepository,IConfiguration config, ISiteRegistryRepository siteRegistryRepository)
        {
            _storageProvider = storageProvider;
            _siteRepository = siteRepository;
            _config = config;
            _siteRegistryRepository = siteRegistryRepository;
        }

        public Task<SiteResponseDto> GetBySiteNo(string siteNo)
        {
            throw new NotImplementedException();
        }

        public async Task<SiteResponseDto> Insert(SiteRequestDto siteRequestDto)
        {
            var siteEntity = await CovertToSite(siteRequestDto);
            var bl = _siteRepository.Insert(siteEntity);

            if (bl && siteEntity.SiteRegistries != null)
            {
                var siteResponseDto = new SiteResponseDto()
                {
                    Id = siteEntity.SiteNo,
                    Registry = new List<RegistryResponseDto>()
                };

                siteEntity.SiteRegistries.ForEach(item => siteResponseDto.Registry.Add(new RegistryResponseDto()
                {
                    Checksum = item.Checksum,
                    Model = item.Model,
                    Url = item.TargetUrl,
                    Version = item.Version
                }));

                return await Task.FromResult(siteResponseDto);
            }

            throw new Exception("Failed to create site.");
        }

        public async Task<SiteResponseDto> Update(string siteNo, SiteRequestDto siteRequestDto)
        {
            var siteEntity = await CovertToSite(siteRequestDto, siteNo);
            var bl = _siteRepository.Update(siteNo,siteEntity);

            if (bl && siteEntity.SiteRegistries != null)
            {
                var siteResponseDto = new SiteResponseDto()
                {
                    Id = siteNo,
                    Registry = new List<RegistryResponseDto>()
                };

                siteEntity.SiteRegistries.ForEach(item => siteResponseDto.Registry.Add(new RegistryResponseDto()
                {
                    Checksum = item.Checksum,
                    Model = item.Model,
                    Url = item.TargetUrl,
                    Version = item.Version
                }));

                return await Task.FromResult(siteResponseDto);
            }

            throw new Exception("Failed to update site.");
        }


        private async Task<Site> CovertToSite(SiteRequestDto siteRequestDto,string siteNo=null)
        {
            if (siteRequestDto == null)
            {
                return null;
            }

            var site = new Site()
            {
                SiteNo = string.IsNullOrEmpty(siteNo)?Guid.NewGuid().ToString():siteNo,
                CreatedAt = DateTime.UtcNow
            };

            site.SiteRegistries = await CovertToSiteRegistry(siteRequestDto.Registry, site.SiteNo);

            return site;
        }

        private async Task<List<SiteRegistry>>? CovertToSiteRegistry(List<RegistryRequestDto> registryRequestDtos,string siteNo)
        {
            if (registryRequestDtos == null)
            {
                return null;
            }

            var list = new List<SiteRegistry>();
            var version = Utility.GenerateVersion();
            foreach (var registry in registryRequestDtos)
            {
                var siteDevices = new List<SiteDevice>();
                var connectionString = _config["BlobOfRegistry:ConnectionString"]; 
                Uri uri = new Uri(registry.Url);
                BlobClient blobClient = new BlobClient(uri);
                var blobContainerName = blobClient.BlobContainerName;
                var blobName = blobClient.Name;
                var md5 =await _storageProvider.GetBlobMD5(connectionString, blobContainerName, blobName);

                if (md5 != registry.Checksum)
                {
                    throw new Exception($"{registry.Url} Invalid Checksum");
                }
                
                var blobContainerNameTarget = _config["BlobOfRegistry:BlobContainerNameTarget"];
                var content_json= await _storageProvider.DownloadToText(connectionString, blobContainerName, blobName);
                var targetRelativePath = @$"deviceConfigFile/{registry.Model}/{siteNo}";
                var target_url = await _storageProvider.CopyToTargetContainer(connectionString, blobContainerName, blobName, blobContainerNameTarget, targetRelativePath);

                var deviceInfo = await ConverToSiteDevicesFromJson(content_json, registry.Model);
                if (deviceInfo.DeviceTransformerDtos != null)
                {
                    deviceInfo.DeviceTransformerDtos.ForEach(item => { siteDevices.Add(new SiteDevice() { DeviceNo = item.DeviceNo, JObjectInAllowList=item.JOjectInAllowList }); });
                }

                list.Add(new SiteRegistry()
                {
                    Model = registry.Model,
                    SourceUrl = registry.Url,
                    TargetUrl = target_url,
                    Checksum = registry.Checksum,
                    SiteDevices = siteDevices,
                    Version= version
                }); ;
            }

            return list;
        }

     

        private async Task<SiteDeviceTransformerDto> ConverToSiteDevicesFromJson(string content_json, string mode = "vcc")
        {
            if (mode.ToLower() == ModelEnum.VRC.ToString().ToLower())
            {
                var siteDeviceLora = Utilities.GetSiteDeviceFromLoraJson(content_json);
                return siteDeviceLora;
            }

            var siteDeviceMesh = Utilities.GetSiteDeviceFromMeshJson(content_json);
            return siteDeviceMesh;
        }

        public async Task<SiteResponseDto> GetRegistryBySiteNo(string siteNo)
        {
            if (string.IsNullOrEmpty(siteNo))
            {
                throw new BizException("SiteId doesn't empty.");
            }
            var siteRegisties = _siteRepository.GetSiteRegistiesBySiteNo(siteNo);
            var siteResitry = siteRegisties.FirstOrDefault();

            if (siteResitry == null)
            {
                return new SiteResponseDto();
            }

            var siteResponseDto = new SiteResponseDto()
            {
                Id = siteResitry.SiteNo
            };

            siteResitry.SiteRegistries.ForEach(item => siteResponseDto.Registry.Add(new RegistryResponseDto()
            {
                Checksum = item.Checksum,
                Model = item.Model,
                Url = item.TargetUrl,
                Version =   item.Version
            }));

            return await Task.FromResult(siteResponseDto);
        }

        public async Task<List<SiteDeviceDto>> GeAllowListOfSiteDeviceBySiteNoAndDeviceNos(string siteNo, List<string> deviceNos)
        {
            var list = _siteRepository.GeAllowListOfSiteDeviceBySiteNoAndDeviceNos(siteNo, deviceNos);

            var result =new List<SiteDeviceDto>();

            foreach (var item in list)
            {
                result.Add(item.CovertToSiteDeviceDto());
            }

            return await Task.FromResult(result);
        }

        public List<SiteDeviceDetailInfoDto> GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(string siteNo, string gatewayNo, List<string> deviceNos)
        {
            var list = _siteRepository.GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(siteNo, gatewayNo, deviceNos);

            var result = new List<SiteDeviceDetailInfoDto>();

            foreach (var item in list)
            {
                result.Add(item.CovertToSiteDeviceDto());
            }

            return result;
        }
    }
}
