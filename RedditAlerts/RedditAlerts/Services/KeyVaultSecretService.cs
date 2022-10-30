using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditAlerts.Services;

public class KeyVaultSecretService
{
    private readonly SecretClient _akvSecretClient;
    public KeyVaultSecretService(string url)
    {
        _akvSecretClient = new SecretClient(new Uri(url), new DefaultAzureCredential());
    }

    public async Task<string> GetKeyVaultSecretAsync(string secretName)
    {
        if (secretName.Contains("/secrets/"))
        {
            secretName = secretName.Split("/secrets/")[1].Split("/")[0];
        }
        KeyVaultSecret returnedSecret = await _akvSecretClient.GetSecretAsync(secretName);
        return returnedSecret.Value;
    }
}