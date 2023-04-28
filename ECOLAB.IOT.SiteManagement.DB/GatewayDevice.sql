/****** Object:  Table [dbo].[GatewayDevice]    Script Date: 2023/4/27 17:00:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[GatewayDevice](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SiteId] [bigint] NOT NULL,
	[GatewayId] [bigint] NOT NULL,
	[DeviceNo] [nvarchar](100) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO


