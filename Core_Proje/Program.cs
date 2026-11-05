using DataAccessLayer.Concrete;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Core_Proje.Infrastructure;

bool prepareAdmin;
bool prepareContent;
try
{
    prepareAdmin = InitialSetup.ReadFlag("CORE_PROJE_PREPARE_ADMIN");
    prepareContent = InitialSetup.ReadFlag("CORE_PROJE_PREPARE_CONTENT");
}
catch (InitialSetup.SetupException exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
    return;
}

var builder = WebApplication.CreateBuilder(args);
if (prepareAdmin || prepareContent)
{
    // Hazırlık hatalarında EF bağlantı/parametre ayrıntılarını loglama.
    builder.Logging.ClearProviders();
}

// Add services to the container.
builder.Services.AddDbContext<Context>();
builder.Services.AddControllersWithViews(config=>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    config.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddIdentity<WriterUser, WriterRole>().AddEntityFrameworkStores<Context>();

builder.Services.ConfigureApplicationCookie(options=>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1000);
    options.AccessDeniedPath = "/ErrorPage/Index/";
    options.LoginPath = "/Writer/Login/Index/";
});

var app = builder.Build();

if (prepareAdmin || prepareContent)
{
    try
    {
        await InitialSetup.RunAsync(app.Services, prepareAdmin, prepareContent);
        Console.WriteLine("Başlangıç hazırlığı tamamlandı. Web sunucusu başlatılmadı.");
    }
    catch (InitialSetup.SetupException exception)
    {
        Console.Error.WriteLine(exception.Message);
        Environment.ExitCode = 1;
    }
    catch (Exception)
    {
        Console.Error.WriteLine("Başlangıç hazırlığı başarısız. Bağlantı, uygulanmış migration'lar ve Identity ayarlarını kontrol edin. Gizli ayrıntılar loglanmadı.");
        Environment.ExitCode = 1;
    }
    finally
    {
        await app.DisposeAsync();
    }
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseStatusCodePages();
app.UseStatusCodePagesWithReExecute("/ErrorPage/Error404/");
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Default}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}",
    defaults: new { area = "" })
    .WithStaticAssets();

app.Run();
