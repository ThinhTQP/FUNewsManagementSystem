
using FUNews.BLL.Services;
using FUNews.DAL;
using FUNews.DAL.Entities;
using FUNewsManagementMVC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositoryUOW();
builder.Services.AddAuthen();


var app = builder.Build();



    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SystemAccounts}/{action=Login}/{id?}");

app.Run();
