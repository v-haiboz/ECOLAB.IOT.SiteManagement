/****** Object:  Table [dbo].[CertificationInfo]    Script Date: 2023/6/20 12:41:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CertificationInfo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CertificationName] [nvarchar](max) NOT NULL,
	[CertificationDesc] [nvarchar](max) NULL,
	[CertificationToken] [nvarchar](max) NOT NULL,
	[CertificationTokenExpirationUtcTime] [bigint] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_CertificationInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


