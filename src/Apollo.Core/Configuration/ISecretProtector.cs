namespace Apollo.Core.Configuration;

public interface ISecretProtector
{
  string Protect(string plaintext);
  string Unprotect(string ciphertext);
}
