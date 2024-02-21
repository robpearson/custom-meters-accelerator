# Azure Monitoring and Azure Policy Best Practices

This document provides best practices for monitoring using Azure Monitor Diagnostics and Logs (MDC) and enforcing settings using Azure Policy.

## Azure Monitor Diagnostics and Logs (MDC)

Azure MDC provides a comprehensive solution for collecting, analyzing, and acting on telemetry from your cloud and on-premises environments. It helps you understand how your applications are performing and proactively identifies issues affecting them and the resources they depend on.

## Azure Policy

Azure Policy is a service in Azure that you use to create, assign, and manage policies. These policies enforce different rules and effects over your resources, so those resources stay compliant with your corporate standards and service level agreements.

### Monitoring and Enforcement via Azure Policy

Azure Policy can be used to monitor and enforce specific settings in your Azure environment. To support Azure Policy, you first need to expose an alias for the particular setting you want to monitor or enforce.

#### Policies for Monitoring Network Settings

You can create policies to monitor network settings such as Network Security Groups, Virtual Networks, and Subnets. For example, you can enforce that all Network Security Groups should deny traffic from a specific IP address.

#### Policies for Monitoring Data Protection Settings

You can create policies to monitor data protection settings such as Azure Storage Service Encryption, Azure SQL Database Transparent Data Encryption, and Azure Disk Encryption. For example, you can enforce that all Azure Storage Accounts should have Storage Service Encryption enabled.

## Best Practices

1. **Enable Azure MDC on all resources**: This allows you to collect and analyze performance metrics and logs to understand how your application is performing and to identify any issues that need to be addressed.

2. **Use Azure Policy for enforcement**: Azure Policy helps ensure your resources are in compliance with your corporate standards.

3. **Regularly review and analyze your policies**: This helps you understand their impact, identify opportunities for improvement, and ensure compliance with your corporate standards.
1. **Use Azure Policy for monitoring**: Azure Policy helps ensure your resources are in compliance with your corporate standards.

## Reporting Issues

If you encounter any issues with monitoring or policy enforcement, please contact our support team.



