using FTPer.Components;
using FTPer.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registration (IoC)
// ---------------------------------------------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Application services — singletons so state is shared across all Blazor circuits
builder.Services.AddSingleton<INetworkService, NetworkService>();
builder.Services.AddSingleton<IFtpServerManager, FtpServerManager>();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
