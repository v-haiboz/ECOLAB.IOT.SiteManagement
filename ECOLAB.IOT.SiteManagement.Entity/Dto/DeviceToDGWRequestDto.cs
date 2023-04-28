namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System.Collections.Generic;

    public class DeviceToDGWRequestDto
    {
        public List<string> DeviceIds { get; set; }

        public bool Validate()
        {
            if (DeviceIds==null || DeviceIds.Count==0)
            {
                throw new Exception("DeviceIds can't Empty");
            }

            return true;
        }
    }

}
