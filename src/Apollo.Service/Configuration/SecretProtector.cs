using Apollo.Core.Configuration;
using Microsoft.AspNetCore.DataProtection;

namespace Apollo.Service.Configuration;

public class SecretProtector(IDataProtectionProvider dataProtectionProvider) : ISecretProtector
{
  private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("Apollo.Configuration");

  public string Protect(string plaintext)
  {
    return string.IsNullOrEmpty(plaintext) ? plaintext : _protector.Protect(plaintext);
  }

  public string Unprotect(string ciphertext)
  {
    return string.IsNullOrEmpty(ciphertext) ? ciphertext : _protector.Unprotect(ciphertext);
  }
}
