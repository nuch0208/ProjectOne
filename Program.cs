using ProjectOne.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Backend", client =>
{
    client.BaseAddress = new Uri("http://localhost:5230/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ✅ สำคัญ: ป้องกัน circuit timeout ตอน upload + เพิ่มเพดานรับ message ของ SignalR
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(o =>
    {
        o.DetailedErrors = true;
        o.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(5); // ⭐ กัน RemoteJSDataStream timeout
    })
    .AddHubOptions(o =>
    {
        o.MaximumReceiveMessageSize = 50 * 1024 * 1024;          // ⭐ ต้อง >= รวมไฟล์ที่ส่ง
        o.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
        o.KeepAliveInterval = TimeSpan.FromSeconds(15);
    });

builder.Services.AddBlazoredLocalStorage();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
