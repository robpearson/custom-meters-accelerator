-- This script is used to create the schema for the database
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'ApplicationConfigurations')
BEGIN
    CREATE TABLE [ApplicationConfigurations]
    (
        id int PRIMARY KEY,
        Name NVARCHAR(150),
        Value NVARCHAR(4000),
        Description NVARCHAR(150)
    );

        INSERT INTO ApplicationConfigurations
    	([id],[Name],[Value],[Description])
    VALUES
        (1,'SMTPFromEmail','','SMTP Email'),
    	(2,'SMTPPassword','','SMTP Password'),
    	(3,'SMTPHost','','SMTP Host'),
    	(4,'SMTPPort','','SMTP Port'),
    	(5,'SMTPUserName','','SMTP User Name'),
    	(6,'SMTPSslEnabled','','SMTP Ssl Enabled'),
    	(7,'ApplicationName','Contoso','Application Name'),
    	(8,'SchedulerEmailTo','true','Active Email Enabled'),
    	(9,'EnablesSuccessfulSchedulerEmail','false','Enable Successful Email'),
    	(10,'EnablesFailureSchedulerEmail','false','Enable Failure Email'),
    	(11,'EnablesMissingSchedulerEmail','false','Enable Missing Email'),
        (12,'Failure_Subject', 'Scheduled Metered Task Failure!', 'Failure Email Subject'),
        (13,'Accepted_Subject', 'Scheduled Metered Task Submitted Successfully!', 'Successful Email Subject'),
        (14,'Missing_Subject', 'Scheduled Metered Task was Skipped!', 'Missing Email Subject'),
        (15,'Missing_Email', '<html><head></head><body><center><table align=center><tr><td><h2 >Subscription ****SubscriptionName****</h2><br><p>The Scheduled Task ****SchedulerTaskName**** was <b>skipped</b> by scheduler engine!</b></p><br>Please try again or contact technical support to troubleshoot the issue.<p>The following section is the deatil results.</p><hr/>****ResponseJson****</td></tr></table></center></body> </html>', 'Missing Email Template'),
        (16,'Accepted_Email', '<html><head></head><body><center><table align=center><tr><td><h2>Subscription ****SubscriptionName****</h2><br><p>The Scheduled Task ****SchedulerTaskName**** was fired <b>Successfully</b></p><p>The following section is the deatil results.</p><hr/>****ResponseJson****</td></tr></table></center></body> </html>', 'Successful Email Template'),
        (17,'Failure_Email', '<html><head></head><body><center><table align=center><tr><td><h2 >Subscription ****SubscriptionName****</h2><br><p>The Scheduled Task ****SchedulerTaskName**** was fired<b> but Failed to Submit Data</b></p><br>Please try again or contact technical support to troubleshoot the issue.<p>The following section is the deatil results.</p><hr/>****ResponseJson****</td></tr></table></center></body> </html>', 'Failure Email Template')
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


