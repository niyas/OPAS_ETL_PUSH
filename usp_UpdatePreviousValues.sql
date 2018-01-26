CREATE PROCEDURE usp_UpdatePreviousValues
AS
BEGIN
	Declare Curname Cursor
	for 
	select Name,Updatedname
	from @table

	open Curname

	Fetch next from Curname into @Condition,@UpdatedCondition

	while @@FETCH_STATUS = 0 
	begin
	Set @SqlText = N';With [StatusT]
	as
	(
	select [IncidentId] ,max([CreatedDateTime])[CreatedDateTime]
	from [AITool].[dbo].[IncidentManagement_Data] With (nolock)
	where isnull('+@Condition+','''')<> ''''
	Group by [IncidentId]
	)
	,FinalStatusTracking
	as
	(
	select Data.IncidentId,
		   Data.'+@Condition+'
	from [AITool].[dbo].[IncidentManagement_Data] Data With (nolock)
	inner join [StatusT] ST
	on Data.[IncidentId] = ST.[IncidentId]
	and Data.[CreatedDateTime] = ST.[CreatedDateTime]
	)

	update IW
	set '+@UpdatedCondition+' = ST.'+@Condition+'
	from [AITool].[dbo].[IncidentManagement_WeeklyData] IW
	inner join FinalStatusTracking ST
	on ST.IncidentId = IW.IncidentId 
	'

	--print @SqlText
	exec sp_executesql @SqlText


	Fetch next from Curname into @Condition,@UpdatedCondition

	end 

	Close Curname
	Deallocate Curname
END
