using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Worlde.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WorldeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WorldeContext") ?? throw new InvalidOperationException("Connection string 'WorldeContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

app.Run();
