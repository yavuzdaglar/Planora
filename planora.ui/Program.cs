using Planora.UI.Mapping;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5200");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// AutoMapper
builder.Services.AddAutoMapper(typeof(UiMapping).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calendar}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();