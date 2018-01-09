USE [AITool]
GO
/****** Object:  StoredProcedure [dbo].[usp_IncidentManagement_WeeklyDataInsert]    Script Date: 09/01/2018 16:11:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[usp_IncidentManagement_WeeklyDataInsert]
(
@IncidentId varchar(50),
@NotificationText varchar(max),
@SeverityNumber varchar(50),
@Status varchar(150) ,
@SuspendReason varchar(150) ,
@Assignee varchar(150),
@AssigneeGroup varchar (150)
)
AS
Begin

Insert into IncidentManagement_WeeklyData
( [IncidentId],[NotificationText],[SeverityNumber],[Status],[SuspendReason],[Assignee],[AssigneeGroup],CreatedDateTime )
Values (@IncidentId,@NotificationText,@SeverityNumber,@Status,@SuspendReason,@Assignee,@AssigneeGroup,getdate())


END 



------------


ALTER PROCEDURE [dbo].[usp_IncidentManagement_DataInsert]
AS
Begin

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
 
END 
