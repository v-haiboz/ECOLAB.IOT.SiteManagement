/****** Object:  Table [dbo].[SiteRegistry]    Script Date: 2023/4/27 16:57:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SiteRegistry](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SiteId] [bigint] NOT NULL,
	[Model] [nvarchar](100) NOT NULL,
	[SourceUrl] [nvarchar](2000) NOT NULL,
	[TargetUrl] [nvarchar](2000) NOT NULL,
	[Checksum] [nvarchar](100) NOT NULL,
	[JObject] [nvarchar](4000) NULL,
	[Version] [nvarchar](100) NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO


