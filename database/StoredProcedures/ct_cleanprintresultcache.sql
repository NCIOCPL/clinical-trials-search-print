USE [PercCancerGov]
GO

/****** Object:  StoredProcedure [dbo].[ct_cleanprintresultcache]    Script Date: 1/18/2022 1:36:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[ct_cleanprintresultcache]
as
BEGIN

	delete from dbo.ctprintresult 
	where printid in (select printid from dbo.ctprintresultcache where cachedate < dateadd(d,-90,getdate()))

	delete from dbo.ctPrintResultCache 
	where cachedate < dateadd(d,-90,getdate())


END

GO


