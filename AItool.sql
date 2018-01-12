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

	insert into AITool.dbo.[IncidentManagement_Data]
	([IncidentId],[NotificationText],[SeverityNumber],[Status],[SuspendReason],[Assignee],[AssigneeGroup],CreatedDateTime,MergeDateTime) 
	select 
	[IncidentId],[NotificationText],[SeverityNumber],[Status],[SuspendReason],[Assignee],[AssigneeGroup],CreatedDateTime,getdate()
	From AITool.dbo.[IncidentManagement_WeeklyData]
	
	Truncate Table IncidentManagement_WeeklyData
END 

