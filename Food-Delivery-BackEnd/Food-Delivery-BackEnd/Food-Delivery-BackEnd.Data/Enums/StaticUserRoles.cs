using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food_Delivery_BackEnd.Data.Enums
{
    public class StaticUserRoles
    {
        public const string Admin = "ADMIN";
        public const string Partner = "Partner";
        public const string Customer = "Customer";

        public const string AdminPartner = "ADMIN,PARTNER";
        public const string AdminPartnerCustomer = "ADMIN,Partner,Customer";
    }
}
