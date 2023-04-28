namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;

    public class GatewayRequestUpdateDto
    {
        public string Old { set; get; }

        public string New { set; get; }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(Old) || string.IsNullOrEmpty(Old))
            {
                throw new Exception("old Or new  doesn't Empty");
            }

            return true;
        }
    }
}
