namespace ECOLAB.IOT.SiteManagement.Provider
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using Flurl.Http;
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;

    public interface ITokenProvider
    {
        public Task<TokenInfoDto> GetToken();
    }

    public class TokenProvider : ITokenProvider
    {
        private IConfiguration _config;
        private string _tokenUrl;//= "https://login.partner.microsoftonline.cn/2c3c280f-2394-453f-944d-6df0dd9c338e/oauth2/v2.0/token";
        public TokenProvider(IConfiguration config)
        {
            _config = config;
            _tokenUrl = _config["Distribute:Token:Url"];
        }
        
        public async Task<TokenInfoDto> GetToken()
        {
            var result = await _tokenUrl.PostUrlEncodedAsync(new
            {
                client_id = _config["Distribute:Token:Client_id"],//"dd609f2a-99e0-4c1d-bd9f-58bdd4573384",
                scope = _config["Distribute:Token:Scope"],// "dd609f2a-99e0-4c1d-bd9f-58bdd4573384/.default",
                client_secret = _config["Distribute:Token:Client_secret"],// "i~5x_Gy2UrC1~R-k8h.KbDjM8bA.Z4DbhS",
                grant_type = _config["Distribute:Token:Grant_type"]//"client_credentials"
            }).ReceiveJson<TokenInfoDto>();

            return result;
        }
    }
}
