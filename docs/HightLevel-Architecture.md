# Application Design

This document provides an overview of the application design, focusing on the use of private endpoints, managed identities, and Azure Key Vault.

## Application Architecture

The application is hosted on Azure and uses several Azure services for its operations:

- **Azure Web App**: Hosts the application.
- **Azure Cosmos DB**: Stores application data.
- **Azure Key Vault**: Stores secrets used by the application.

## Private Endpoint for Cosmos DB

The application uses a private endpoint for Cosmos DB to ensure secure connectivity. The private endpoint uses an IP address from the VNet address space for Cosmos DB. Network traffic between the Web App and Cosmos DB traverses over the VNet and a private link on the Microsoft backbone network, eliminating exposure from the public internet.

## Managed Identity for Web App

The application uses a managed identity for the Web App. This managed identity is used to authenticate to services that support Azure AD authentication, without needing to store credentials in the application.

## Accessing Key Vault with Managed Identity

The managed identity is used to access secrets in Azure Key Vault. This eliminates the need to store sensitive information like connection strings in the application code. The application requests a token from Azure AD using its managed identity, and uses that token to authenticate to Key Vault.

## Key Vault with Selected Networks

The Key Vault is configured to allow access from selected networks only. This includes the Azure Web App and any other necessary services. This helps to secure the secrets stored in Key Vault.

## Web App VNet Integration

The Web App is integrated with a VNet to securely connect to the Cosmos DB private endpoint. This ensures that traffic between the Web App and Cosmos DB stays within the Azure network.

## Reporting Issues

If you encounter any issues with the application, please contact our support team.