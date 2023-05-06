namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;

    public class GatewayRequestUpdateDto
    {
        public string Old { set; get; }

        public string New { set; get; }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(Old) || string.IsNullOrEmpty(New))
            {
                throw new Exception("old Or new  can't Empty");
            }
            else if (Old == New)
            {
                throw new Exception("the old SN And the new SN cannot be the same");
            }

            return true;
        }
    }
}
