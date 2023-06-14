namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System.ComponentModel.DataAnnotations;

    public class InsertUserWhiteListDto
    {
        /// <summary>
        /// 艺康aad用户白名单
        /// </summary>
        [StringLength(128)]
        [Required(ErrorMessage = "Email is required"), RegularExpression(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Wrong email")]
        public string Email { get; set; }
    }
}
