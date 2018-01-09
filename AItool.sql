
--Create Table Type variables

CREATE TYPE Type_IncidentManagement_Data AS TABLE 
 (
	[IncidentId] Varchar(50) , 
    [NotificationText] varchar(max) ,  
    [SeverityNumber] int,
	[Status] Varchar(150), 
	[SuspendReason] Varchar(150), 
	[Assignee] Varchar(150), 
	[AssigneeGroup] Varchar(150)

 )
 GO

 
 --Create Stored Proceure

CREATE PROCEDURE [dbo].[usp_IncidentManagement_WeeklyDataInsert]
@IncidentManagement Type_IncidentManagement_Data READONLY
AS
-- merge data to log table 
;MERGE AITool.dbo.[IncidentManagement_Data] AS TARGET_TABLE  
USING (SELECT  
  [IncidentId],
  [NotificationText],
  [SeverityNumber],
  [Status],
  [SuspendReason],
  [Assignee],
  [AssigneeGroup],
  CreatedDateTime
  FROM  AITool.dbo.[IncidentManagement_WeeklyData]
 ) AS SOURCE_TABLE  
 ON SOURCE_TABLE.[IncidentId] = TARGET_TABLE.[IncidentId]
WHEN MATCHED THEN UPDATE  
SET [NotificationText]=SOURCE_TABLE.[NotificationText],  
[SeverityNumber]=SOURCE_TABLE.[SeverityNumber],
[Status]=SOURCE_TABLE.[Status],
[SuspendReason]=SOURCE_TABLE.[SuspendReason],
[Assignee]=SOURCE_TABLE.[Assignee],
[AssigneeGroup]=SOURCE_TABLE.[AssigneeGroup] ,
CreatedDateTime = SOURCE_TABLE.CreatedDateTime,
MergeDateTime = getdate()
WHEN NOT MATCHED THEN INSERT ([IncidentId],[NotificationText],[SeverityNumber],[Status],[SuspendReason],[Assignee],[AssigneeGroup],CreatedDateTime,MergeDateTime) 
VALUES 
([IncidentId],[NotificationText],[SeverityNumber],[Status],[SuspendReason],[Assignee],[AssigneeGroup],CreatedDateTime,getdate());
 
Truncate Table [IncidentManagement_WeeklyData]

Insert into IncidentManagement_WeeklyData
( [IncidentId],[NotificationText],[SeverityNumber],[Status],[SuspendReason],[Assignee],[AssigneeGroup],CreatedDateTime )
select [IncidentId],[NotificationText],[SeverityNumber],[Status],[SuspendReason],[Assignee],[AssigneeGroup],getdate() Createddate
From @IncidentManagement 

END 
