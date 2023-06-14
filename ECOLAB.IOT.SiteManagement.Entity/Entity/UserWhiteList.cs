namespace ECOLAB.IOT.SiteManagement.Data.Entity
{
    using System;

    public class UserWhiteList 
    {
        public float Id { get; set; }

        /// <summary>
        /// ECOLAB AAD
        /// </summary>

        public string Email { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
