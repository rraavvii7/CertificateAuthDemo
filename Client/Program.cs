using System.Security.Cryptography.X509Certificates;

var handler = new HttpClientHandler();
var clientCert = new X509Certificate2("../../../Certificates/client.pfx", "Sonugupt@7");
handler.ClientCertificates.Add(clientCert);

//in case if you dont want to validate server certificate use below line
//handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
{
    if (cert is null) return false;

    // Perform custom server validation (e.g., check issuer, thumbprint)
    return cert.Issuer.Contains("E=rraavvii7@gmail.com") &&
           cert.Subject.Contains("E=server@learning.com");
};

var client = new HttpClient(handler);

try
{
    var response = await client.GetAsync("https://localhost:5001/api/WeatherForecast");
    //var response = await client.GetAsync("https://localhost:5001");
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Response: {content}");
}
catch (Exception ex)
{
    Console.WriteLine($"Request failed: {ex.Message}");
}
