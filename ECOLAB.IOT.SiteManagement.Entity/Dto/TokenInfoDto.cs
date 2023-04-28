namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    public class TokenInfoDto
    {
        public string? TokenType { get; set; }
        public int Expires_In { get; set; }

        public int Ext_Expires_In { get; set; }

        public string? Access_Token { get; set; }

    }
}
