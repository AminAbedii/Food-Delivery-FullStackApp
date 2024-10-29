using Food_Delivery_BackEnd.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Data.Models
{
    public class Partner : User
    {
        public PartnerStatus Status { get; set; }
        public List<Store> Stores { get; set; } = new List<Store>();
    }
}
