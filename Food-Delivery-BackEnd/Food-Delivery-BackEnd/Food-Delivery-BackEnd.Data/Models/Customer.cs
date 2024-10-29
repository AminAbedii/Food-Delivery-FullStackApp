using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Data.Models
{
    public class Customer : User
    {
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
