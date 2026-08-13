using Planora.Application.Interfaces;
using Planora.Application.Mapping;
using Planora.Application.Services;
using Planora.Domain.Interfaces;
using Planora.Infrastructure.Context;
using Planora.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PlanoraUi", policy =>
        policy.WithOrigins("http://localhost:5200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddAutoMapper(typeof(UserMapping), typeof(BlockMapping));
builder.Services.AddDbContext<PlanoraDbContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBlockRepository, BlockRepository>();
builder.Services.AddScoped<IBlockService, BlockService>();
builder.Services.AddScoped<IAiPlannerService, AiPlannerService>();
builder.Services.AddScoped<IAiCommandService, AiCommandService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("PlanoraUi");
app.UseAuthorization();
app.MapControllers();

app.Run();