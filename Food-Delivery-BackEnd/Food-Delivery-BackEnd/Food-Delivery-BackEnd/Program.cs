using AutoMapper;
using CloudinaryDotNet;
using FluentValidation;
using Food_Delivery_BackEnd.Core.Services;
using Food_Delivery_BackEnd.Core.Services.Interfaces;
using Food_Delivery_BackEnd.Core.Validators;
using Food_Delivery_BackEnd.Data.Context;
using Food_Delivery_BackEnd.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FoodDeliveryServer.Api", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();
//Add Cores
builder.Services.AddCors(options =>
{
    var frontendURL = configuration.GetValue<string>("frontend_url");
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(frontendURL).AllowAnyMethod().AllowAnyHeader();
    });
});



//AddAuthentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWTSettings:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:SecretKey"]))
    };
});



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


// Configure Cloudinary  
var cloudinaryAccount = new Account(
    "your_cloud_name",
    "your_api_key",
    "your_api_secret"
);
var cloudinary = new Cloudinary(cloudinaryAccount);
builder.Services.AddSingleton(cloudinary);


//DE
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IValidator<User>, UserValidator>();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();



//MapperConfiguration mapperConfig = new MapperConfiguration(config =>
//{
//    //config.AddProfile(new AdminProfile());
//    //config.AddProfile(new PartnerProfile());
//    //config.AddProfile(new CustomerProfile());
//    //config.AddProfile(new AuthProfile());
//    //config.AddProfile(new StoreProfile());
//    //config.AddProfile(new ProductProfile());
//    //config.AddProfile(new OrderProfile());
//    //config.AddProfile(new OrderProfile());
//    //config.AddProfile(new CoordinateProfile());
//});
//builder.Services.AddSingleton(mapperConfig.CreateMapper());


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//StripeConfiguration.ApiKey = builder.Configuration["StripeSettings:SecretKey"];

//app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
