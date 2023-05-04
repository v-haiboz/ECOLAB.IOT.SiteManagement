namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;

    public class SiteRequestDto
    {
        public List<RegistryRequestDto>? Registry { get; set; }

        public bool Validate(bool bl=false)
        {
            if (!bl)
            {
                return true;
            }

            if (Registry == null || Registry.Count==0)
            {
                throw new Exception("Registry can't Empty.");
            }

            foreach (var item in Registry)
            {
                item.Validate();
            }

            return true;
        }
    }
}
