using Food_Delivery_BackEnd.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Core.Dto.Request
{
    public class LoginDto
    {
        public GrantType GrantType { get; set; }
        //public StaticUserRoles userRoles { get; set; }
        public UserType userType { get; set; }

        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string? RefreshToken { get; set; }
    }
}
