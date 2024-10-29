using Food_Delivery_BackEnd.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Food_Delivery_BackEnd.Data.Configurations
{
    public class AdminConfiguration : UserConfiguration<Admin>
    {
        public override void Configure(EntityTypeBuilder<Admin> builder)
        {
            base.Configure(builder);
        }
    }
}
