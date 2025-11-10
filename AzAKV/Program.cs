using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

// Replace YOUR-KEYVAULT-NAME with your actual Key Vault name
string KeyVaultUrl = "https://mykeyvaultname17689.vault.azure.net/";


// ADD CODE TO CREATE A CLIENT
DefaultAzureCredentialOptions options = new DefaultAzureCredentialOptions
{
    ExcludeEnvironmentCredential = true,
    ExcludeManagedIdentityCredential = true,
};
var client = new SecretClient(new Uri(KeyVaultUrl), new DefaultAzureCredential(options));


// ADD CODE TO CREATE A MENU SYSTEM
while (true)
{
  Console.Clear();
  Console.WriteLine("\nPlease select an option:");
  Console.WriteLine("1. Create a new secret");
  Console.WriteLine("2. List all secrets");
  Console.WriteLine("Type 'quit' to exit");
  Console.Write("Enter your choice: ");
  string? input = Console.ReadLine()?.Trim().ToLower();

  if (input == "quit")
  {
    Console.WriteLine("Goodbye!");
    break;
  }
  
  switch (input)
  {
    case "1":
        // Call the method to create a new secret
        await CreateSecretAsync(client);
        break;
    case "2":
        // Call the method to list all existing secrets
        await ListSecretsAsync(client);
        break;
    default:
        // Handle invalid input
        Console.WriteLine("Invalid option. Please enter 1, 2, or 'quit'.");
        break;
  }
    
}


// ADD CODE TO CREATE A SECRET

async Task CreateSecretAsync(SecretClient client)
{
  try
  {
    Console.Write("Enter the name of the secret: ");
    string? secretName = Console.ReadLine()?.Trim();

    Console.Write("Enter the value of the secret: ");
    string? secretValue = Console.ReadLine()?.Trim();

    if (!string.IsNullOrEmpty(secretName) && !string.IsNullOrEmpty(secretValue))
    {
      var secret = new KeyVaultSecret(secretName, secretValue);
      await client.SetSecretAsync(secret);
      Console.WriteLine($"Secret '{secretName}' has been created.");
    }
    else
    {
      Console.WriteLine("Secret name and value cannot be empty.");
    }

    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
  }
  catch (Exception ex)
  {
    Console.WriteLine($"An error occurred: {ex.Message}");
  }
}

// ADD CODE TO LIST SECRETS

async Task ListSecretsAsync(SecretClient client)
{
  try
  {
    Console.WriteLine("Listing all secrets in the Key Vault:");
    var secretProperties = client.GetPropertiesOfSecretsAsync();
    bool hasSecrets = false;

    await foreach (var secretProperty in secretProperties)
    {
      hasSecrets = true;
      try
      {
        // Retrieve the actual secret value and metadata using the secret name
        var secret = await client.GetSecretAsync(secretProperty.Name);

        // Display the secret information to the console
        Console.WriteLine($"Name: {secret.Value.Name}");
        Console.WriteLine($"Value: {secret.Value.Value}");
        Console.WriteLine($"Created: {secret.Value.Properties.CreatedOn}");
        Console.WriteLine("----------------------------------------");
      }
      catch (Exception ex)
      {
        // Handle errors for individual secrets (e.g., access denied, secret not found)
        Console.WriteLine($"Error retrieving secret '{secretProperty.Name}': {ex.Message}");
        Console.WriteLine("----------------------------------------");
      }
    }

    if (!hasSecrets)
    {
      Console.WriteLine("No secrets found in the Key Vault.");
    }
  }
  catch (Exception ex)
  {
    Console.WriteLine($"An error occurred: {ex.Message}");
  }
  
      Console.WriteLine("Press Enter to continue...");
    Console.ReadLine();
}
