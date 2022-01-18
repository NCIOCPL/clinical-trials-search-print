USE [PercCancerGov]
GO

/****** Object:  StoredProcedure [dbo].[ct_insertPrintresultcache]    Script Date: 1/18/2022 1:09:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create procedure [dbo].[ct_insertPrintresultcache]
(@content nvarchar(max), @searchparams nvarchar(max), @trialids udt_trialids READONLY, @printid uniqueidentifier output, @islive bit = 1)
AS
BEGIN
set nocount on
declare @pid table(printid uniqueidentifier)

	insert into dbo.ctPrintResultCache(content, searchparams, islive)
	OUTPUT inserted.printid
	INTO @pid
	values(@content, @searchparams, @islive)

	select top 1 @printid = printid from @pid

	insert into dbo.ctprintresult(printid, trialid)
	select @printid, trialid
	from @trialids 

	return
END
GO


