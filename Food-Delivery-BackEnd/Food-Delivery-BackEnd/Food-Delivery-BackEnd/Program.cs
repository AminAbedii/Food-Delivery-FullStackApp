using AutoMapper;
using FluentValidation;
using Food_Delivery_BackEnd.Core.Mapping;
using Food_Delivery_BackEnd.Core.Services;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Food_Delivery_BackEnd.Core.Validators;
using Food_Delivery_BackEnd.Data.Context;
using Food_Delivery_BackEnd.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


// Add AuthenticationSchema and JwtBearer
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
    });




builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter your token with this format: ''Bearer YOUR_TOKEN''",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "bearer",
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "Bearer",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});


var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();

//database
string connString = builder.Configuration.GetConnectionString("FoodDeliveryConnection");
builder.Services.AddDbContext<FoodDeliveryDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("FoodDeliveryConnection"));

});




builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VerifiedPartner", policy => policy.RequireClaim("Status", "Accepted"));
});


//DE
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IValidator<User>, UserValidator>();

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

//builder.Services.AddAutoMapper(typeof(Program));


MapperConfiguration mapperConfig = new MapperConfiguration(config =>
{
    config.AddProfile(new AdminProfile());
    config.AddProfile(new PartnerProfile());
    config.AddProfile(new CustomerProfile());
    config.AddProfile(new AuthProfile());
    config.AddProfile(new StoreProfile());
    config.AddProfile(new ProductProfile());
    config.AddProfile(new OrderProfile());
    config.AddProfile(new CoordinateProfile());
});
builder.Services.AddSingleton(mapperConfig.CreateMapper());



var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(options =>
{
    options
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin();
});
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
