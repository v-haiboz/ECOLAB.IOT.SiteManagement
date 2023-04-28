namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;

    public class SiteRequestDto
    {
        public List<RegistryRequestDto>? Registry { get; set; }

        public bool Validate()
        {
            if (Registry == null || Registry.Count==0)
            {
                throw new Exception("Registry doesn't Empty.");
            }

            foreach (var item in Registry)
            {
                item.Validate();
            }

            return true;
        }
    }
}
