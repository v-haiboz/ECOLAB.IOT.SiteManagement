namespace ECOLAB.IOT.SiteManagement.Service
{
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using ECOLAB.IOT.SiteManagement.Repository;
    using System;
    using System.Collections.Generic;

    public interface IUserWhiteListService
    {
        public Task<List<UserWhiteListDto>>? GetUserWhiteList(string email, int pageIndex, int pageSize, out int total);

        public Task<UserWhiteList> InsertUserWhiteList(InsertUserWhiteListDto insertUserWhiteListDto);

        public Task<bool> DeleteUserWhiteList(string email);

        public Task<UserWhiteList> GetUserWhiteListByEmail(string email);
    }

    public class UserWhiteListService : IUserWhiteListService
    {
        private readonly IUserWhiteListRepository _userWhiteListRepository;
        private readonly IMemoryCacheService _memoryCacheService;
        public const string UserWhiteListKeyPrefix = "userwhitelist_";

        public UserWhiteListService(IUserWhiteListRepository userWhiteListRepository, IMemoryCacheService memoryCacheService)
        {
            _userWhiteListRepository=userWhiteListRepository;
            _memoryCacheService = memoryCacheService;
        }

        public Task<bool> DeleteUserWhiteList(string email=null)
        {
            var bl= _userWhiteListRepository.DeleteUserWhiteList(email);
            if (bl)
            {
                _memoryCacheService.RemoveValue(email.AddPrefix(UserWhiteListKeyPrefix));
            }

            return Task.FromResult<bool>(bl);
        }

        public Task<List<UserWhiteListDto>>? GetUserWhiteList(string email, int pageIndex, int pageSize, out int total)
        {
            var list= _userWhiteListRepository.GetUserWhiteList(email,pageIndex,pageSize,out total);

            var listDto =new List<UserWhiteListDto>();
            foreach (var item in list)
            {
                listDto.Add(new UserWhiteListDto()
                { 
                   Email=item.Email,
                    CreatedAt=item.CreatedAt,
                     UpdatedAt=item.UpdatedAt
                });
            }

            return Task.FromResult(listDto);
        }

        public Task<UserWhiteList> InsertUserWhiteList(InsertUserWhiteListDto insertUserWhiteListDto)
        {
            var list= _userWhiteListRepository.InsertUserWhiteList(new UserWhiteList() {  Email= insertUserWhiteListDto.Email});
            return Task.FromResult(list);
        }

        public Task<UserWhiteList> GetUserWhiteListByEmail(string email)
        {
            var list = _userWhiteListRepository.GetUserWhiteListByEmail(email);
            return Task.FromResult(list);
        }
    }
}
