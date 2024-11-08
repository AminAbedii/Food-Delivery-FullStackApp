﻿using Food_Delivery_BackEnd.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Data.Models
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserType UserType { get; set; }
    }
}
