/****** Object:  Table [dbo].[Site]    Script Date: 2023/4/27 16:55:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Site](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SiteNo] [nvarchar](50) NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL
) ON [PRIMARY]
GO

