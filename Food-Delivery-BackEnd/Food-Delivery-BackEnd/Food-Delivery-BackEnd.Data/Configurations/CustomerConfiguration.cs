using Food_Delivery_BackEnd.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Food_Delivery_BackEnd.Data.Configurations
{
    public class CustomerConfiguration : UserConfiguration<Customer>
    {
        public override void Configure(EntityTypeBuilder<Customer> builder)
        {
            base.Configure(builder);
        }
    }
}
