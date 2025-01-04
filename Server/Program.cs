using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();


// Configure Kestrel with HTTPS and Client Certificate Authentication
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps(httpsOptions =>
        {
            var certPath = Path.Combine(builder.Environment.ContentRootPath, "Certificates", "server.pfx");
            httpsOptions.ServerCertificate = new X509Certificate2(certPath, "Sonugupt@7");
            httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls13;
            httpsOptions.CheckCertificateRevocation = false;
            httpsOptions.AllowAnyClientCertificate(); // For production, use custom validation
        });
    });

    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate; // Ensure this is set
    });
});


var app = builder.Build();

// Middleware to validate client certificates
app.Use(async (context, next) =>
{
    var clientCert = await context.Connection.GetClientCertificateAsync();
    if (clientCert == null)
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Client certificate is required.");
        return;
    }

    if (!IsValidCertificate(clientCert))
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Invalid client certificate.");
        return;
    }

    await next();
});

app.UseRouting();
app.UseHttpsRedirection();
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}
app.MapGet("/", () => "Hello, authenticated client!");
app.Run();

static bool IsValidCertificate(X509Certificate2 cert)
{
    return cert.Subject.Contains("E=client@learning.com") &&
           cert.Issuer.Contains("E=rraavvii7@gmail.com");
}