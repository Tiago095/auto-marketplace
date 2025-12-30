using AutoMatch.Data;
using Microsoft.EntityFrameworkCore;

using AutoMatch.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar ligação à BD
builder.Services.AddDbContext<AutoMatchContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ? Adicionar suporte a sessão
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddScoped<IEmailService, EmailService>();

// Dependency Injection para AdminService
builder.Services.AddScoped<IAdminService, AdminService>();

// Se ainda não tens, adiciona também o DbContext:
builder.Services.AddDbContext<AutoMatchContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// E sessions se ainda não tens:
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
    db.Database.EnsureCreated(); // força inicialização no arranque
}

// Middleware padrão
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ? Usar sessão
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
