﻿/****** Object:  Table [dbo].[GatewayAllowListTaskHistory]    Script Date: 2023/4/27 17:01:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[GatewayAllowListTaskHistory](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SiteId] [bigint] NOT NULL,
	[SiteNo] [nvarchar](100) NOT NULL,
	[GatewayId] [bigint] NOT NULL,
	[GatewayNo] [nvarchar](100) NOT NULL,
	[AllowListSASUrl] [nvarchar](2000) NOT NULL,
	[Status] [bit] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO


