using Microsoft.Identity.Client;
using dotenv.net;

// Load environment variables from .env file
DotEnv.Load();
var envVars = DotEnv.Read();

// Retrieve Azure AD Application ID and tenant ID from environment variables
string _clientId = envVars["CLIENT_ID"];
string _tenantId = envVars["TENANT_ID"];

// ADD CODE TO DEFINE SCOPES AND CREATE CLIENT
string[] _scopes = new string[] { "User.Read" };
var app = PublicClientApplicationBuilder.Create(_clientId)
    .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
    .WithDefaultRedirectUri()
    .Build();


// ADD CODE TO ACQUIRE AN ACCESS TOKEN
AuthenticationResult result;
try
{
  var accounts = await app.GetAccountsAsync();
  result = await app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
      .ExecuteAsync();
}
catch (MsalUiRequiredException)
{
  result = await app.AcquireTokenInteractive(_scopes)
      .ExecuteAsync();
}

Console.WriteLine($"Access Token:\n{result.AccessToken}");
