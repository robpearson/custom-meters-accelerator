# Custom Meters Accelerator Encryption Policies

## Azure SQL Server and Azure Key Vault Encryption

By default, Azure SQL Server and Azure Key Vault use service-managed keys for encryption. Microsoft automatically manages and rotates these keys.

### Using Service-Managed Keys

When you create a new Azure SQL Server or Azure Key Vault, Microsoft automatically encrypts it using service-managed keys. No additional steps are required to enable this encryption.

### Using Customer-Managed Keys

If you want to manage your own keys, you can use customer-managed keys. This gives you more control over the key management, including the ability to:

- Control key rotation.
- Revoke access by deleting the key.
- Monitor the use of the key.

To use customer-managed keys, you need to:

1. Create or import a key in Azure Key Vault.
2. Assign the necessary permissions to Azure SQL Server or Azure Key Vault.
3. Configure Azure SQL Server or Azure Key Vault to use the customer-managed key.

Please refer to the official Azure documentation for detailed steps on how to use customer-managed keys with Azure SQL Server and Azure Key Vault.

## Reporting a Vulnerability

If you discover a vulnerability, please follow the [Microsoft Security Response Center (MSRC)](https://www.microsoft.com/en-us/msrc) guidelines for reporting it.