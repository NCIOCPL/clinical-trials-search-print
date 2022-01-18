USE [PercCancerGov]
GO

/****** Object:  Table [dbo].[ctPrintResultCache]    Script Date: 1/18/2022 1:10:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ctPrintResultCache](
	[printid] [uniqueidentifier] NOT NULL,
	[cachedate] [datetime] NULL,
	[content] [nvarchar](max) NULL,
	[searchparams] [nvarchar](max) NULL,
	[isLive] [bit] NULL,
 CONSTRAINT [PK_printresultcache] PRIMARY KEY CLUSTERED 
(
	[printid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ctPrintResultCache] ADD  CONSTRAINT [DF_printid]  DEFAULT (newsequentialid()) FOR [printid]
GO

ALTER TABLE [dbo].[ctPrintResultCache] ADD  CONSTRAINT [DF_cachedate]  DEFAULT (getdate()) FOR [cachedate]
GO

ALTER TABLE [dbo].[ctPrintResultCache] ADD  CONSTRAINT [df_isLive]  DEFAULT ((1)) FOR [isLive]
GO


