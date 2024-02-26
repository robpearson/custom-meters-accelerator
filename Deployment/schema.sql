-- This script is used to create the schema for the database
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'ApplicationConfigurations')
BEGIN
    CREATE TABLE [ApplicationConfigurations]
    (
        id NVARCHAR(50) PRIMARY KEY,
        Name NVARCHAR(150),
        Value NVARCHAR(4000),
        Description NVARCHAR(150)
    );

        INSERT INTO ApplicationConfiguration
    	([Name],[Value],[Description])
    VALUES
        ('SMTPFromEmail','','SMTP Email'),
    	('SMTPPassword','','SMTP Password'),
    	('SMTPHost','','SMTP Host'),
    	('SMTPPort','','SMTP Port'),
    	('SMTPUserName','','SMTP User Name'),
    	('SMTPSslEnabled','','SMTP Ssl Enabled'),
    	('ApplicationName','Contoso','Application Name'),
    	('SchedulerEmailTo','true','Active Email Enabled'),
    	('EnablesSuccessfulSchedulerEmail','false','Enable Successful Email'),
    	('EnablesFailureSchedulerEmail','false','Enable Failure Email'),
    	('EnablesMissingSchedulerEmail','false','Enable Missing Email'),
        ('Failure_Subject', 'Scheduled Metered Task Failure!', 'Failure Email Subject'),
        ('Accepted_Subject', 'Scheduled Metered Task Submitted Successfully!', 'Successful Email Subject'),
        ('Missing_Subject', 'Scheduled Metered Task was Skipped!', 'Missing Email Subject'),
        ('Missing_Email', '<html><head></head><body><center><table align=center><tr><td><h2 >Subscription ****SubscriptionName****</h2><br><p>The Scheduled Task ****SchedulerTaskName**** was <b>skipped</b> by scheduler engine!</b></p><br>Please try again or contact technical support to troubleshoot the issue.<p>The following section is the deatil results.</p><hr/>****ResponseJson****</td></tr></table></center></body> </html>', 'Missing Email Template'),
        ('Accepted_Email', '<html><head></head><body><center><table align=center><tr><td><h2>Subscription ****SubscriptionName****</h2><br><p>The Scheduled Task ****SchedulerTaskName**** was fired <b>Successfully</b></p><p>The following section is the deatil results.</p><hr/>****ResponseJson****</td></tr></table></center></body> </html>', 'Successful Email Template'),
        ('Failure_Email', '<html><head></head><body><center><table align=center><tr><td><h2 >Subscription ****SubscriptionName****</h2><br><p>The Scheduled Task ****SchedulerTaskName**** was fired<b> but Failed to Submit Data</b></p><br>Please try again or contact technical support to troubleshoot the issue.<p>The following section is the deatil results.</p><hr/>****ResponseJson****</td></tr></table></center></body> </html>', 'Failure Email Template')
END;
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Plans')
BEGIN
    CREATE TABLE [Plans]
    (
        id NVARCHAR(50) PRIMARY KEY,
        PlanId NVARCHAR(1000),
        PlanName NVARCHAR(1000),
        OfferId NVARCHAR(1000),
        OfferName NVARCHAR(1000),
        Dimension NVARCHAR(4000)
    );
END;
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Subscriptions')
BEGIN
    CREATE TABLE [Subscriptions]
    (
        id NVARCHAR(50) PRIMARY KEY,
        PlanId NVARCHAR(1000),
        Publisher NVARCHAR(1000),
        Product NVARCHAR(1000),
        Version NVARCHAR(50),
        ProvisionState NVARCHAR(50),
        ProvisionTime DATETIME,
        ResourceUsageId NVARCHAR(1000),
        SubscriptionStatus NVARCHAR(50),
        Dimension NVARCHAR(250),
        SubscriptionKey NVARCHAR(1000),
        ResourceUri NVARCHAR(1000)
    );
END;
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Payments')
BEGIN
    CREATE TABLE [Payments]
    (
        id NVARCHAR(50) PRIMARY KEY,
        PaymentName NVARCHAR(1000),
        Quantity float,
        Dimension NVARCHAR(250),
        PlanId NVARCHAR(1000),
        StartDate DATETIME,
        PaymentType NVARCHAR(50),
        OfferId NVARCHAR(1000)
    );
END;    
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'ScheduledTasks')
BEGIN
    CREATE TABLE [ScheduledTasks]
    (
        id NVARCHAR(50) PRIMARY KEY,
        ScheduledTaskName NVARCHAR(1000),
        ResourceUri NVARCHAR(1000),
        Quantity float,
        Dimension NVARCHAR(250),
        NextRunTime DATETIME,
        PlanId NVARCHAR(1000),
        Frequency NVARCHAR(50),
        StartDate DATETIME,
        Status NVARCHAR(50)
    );
END;
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'UsageResults')
BEGIN
    CREATE TABLE [UsageResults]
    (
        id NVARCHAR(50) PRIMARY KEY,
        Status NVARCHAR(50),
        UsagePostedDate DATETIME,
        UsageEventId NVARCHAR(50),
        MessageTime DATETIME,
        ResourceId NVARCHAR(1000),
        Quantity float,
        Dimension NVARCHAR(250),
        PlanId NVARCHAR(1000),
        ScheduledTaskName NVARCHAR(1000),
        ResourceUri NVARCHAR(1000),
        Message NVARCHAR(4000)
    );
END;
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'ApplicationLogs')
BEGIN
    CREATE TABLE [ApplicationLogs]
    (
        id NVARCHAR(50) PRIMARY KEY,
        ActionTime DATETIME,
        LogDetail NVARCHAR(4000)
    );
END;
GO


