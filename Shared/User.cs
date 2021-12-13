using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPWA.Shared
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }
        [Required]
        [MaxLength(256)]
        public string Email { get; set; }
        [Required]
        [MaxLength(256)]
        public string Password { get; set; }
    }
}
