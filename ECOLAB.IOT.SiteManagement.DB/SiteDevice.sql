/****** Object:  Table [dbo].[SiteDevice]    Script Date: 2023/4/27 16:57:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SiteDevice](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SiteId] [bigint] NOT NULL,
	[SiteRegistryId] [bigint] NOT NULL,
	[DeviceNo] [nvarchar](100) NOT NULL,
	[JObjectInAllowList] [nvarchar](4000) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO


