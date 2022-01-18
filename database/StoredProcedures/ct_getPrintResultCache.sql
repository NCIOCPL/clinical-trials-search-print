USE [PercCancerGov]
GO

/****** Object:  StoredProcedure [dbo].[ct_getPrintResultCache]    Script Date: 1/18/2022 1:15:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create procedure [dbo].[ct_getPrintResultCache](@printid uniqueidentifier, @islive bit = 1)
AS
BEGIN
	set nocount on 
	select top 1 content from dbo.ctPrintResultCache where printid = @printid and islive = @isLive
END
GO


