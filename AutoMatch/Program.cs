using AutoMatch.Data;
using Microsoft.EntityFrameworkCore;

using AutoMatch.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar liga��o � BD
builder.Services.AddDbContext<AutoMatchContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ? Adicionar suporte a sess�o
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddScoped<IEmailService, EmailService>();

// Dependency Injection para AdminService
builder.Services.AddScoped<IAdminService, AdminService>();

// Se ainda n�o tens, adiciona tamb�m o DbContext:
builder.Services.AddDbContext<AutoMatchContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// E sessions se ainda n�o tens:
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});


var app = builder.Build();

// Na parte do app.UseRouting() e app.UseEndpoints()
app.UseSession();  // IMPORTANTE: isto deve estar ANTES de MapControllerRoute

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AutoMatchContext>();
    db.Database.EnsureCreated();
    DbInitializer.Initialize(db); // for�a inicializa��o no arranque
}

// Middleware padr�o
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ? Usar sess�o
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
