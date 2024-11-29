using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ServerGame.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        [ForeignKey("Region")]
        public int RegionId { get; set; }
        public string? Avatar { get; set; }

        public bool IsDeleted { get; set; } = false;

        [JsonIgnore]
        public string OTP { get; set; }
        = DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "none";
    }
}
