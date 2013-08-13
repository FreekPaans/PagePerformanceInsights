/****** Object:  Table [dbo].[PageIds]    Script Date: 8/12/2013 4:13:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PageIds](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PageSHA1] [char](40) NOT NULL,
	[PageName] [nvarchar](max) NULL,
 CONSTRAINT [PK_PageIds] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)

GO
/****** Object:  Table [dbo].[PreCalculatedDistribution]    Script Date: 8/12/2013 4:13:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreCalculatedDistribution](
	[Date] [date] NOT NULL,
	[PageId] [int] NOT NULL,
	[MinIncl] [int] NOT NULL,
	[MaxExcl] [int] NOT NULL,
	[Count] [int] NOT NULL,
	[CumulativePercentage] [int] NOT NULL,
 CONSTRAINT [PK_PreCalculatedDistribution] PRIMARY KEY CLUSTERED 
(
	[Date] ASC,
	[PageId] ASC,
	[MinIncl] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)

GO
/****** Object:  Table [dbo].[PreCalculatedDistributionSpecialValues]    Script Date: 8/12/2013 4:13:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreCalculatedDistributionSpecialValues](
	[Date] [date] NOT NULL,
	[PageId] [int] NOT NULL,
	[MedianBucketIndex] [int] NOT NULL,
	[MeanBucketIndex] [int] NOT NULL,
	[_90PercentileBucketIndex] [int] NOT NULL,
 CONSTRAINT [PK_PreCalculatedDistributionSpecialValues] PRIMARY KEY CLUSTERED 
(
	[Date] ASC,
	[PageId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)

GO
/****** Object:  Table [dbo].[PreCalculatedHourlyTrend]    Script Date: 8/12/2013 4:13:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreCalculatedHourlyTrend](
	[Date] [date] NOT NULL,
	[PageId] [int] NOT NULL,
	[Hour] [int] NOT NULL,
	[Count] [int] NOT NULL,
	[Median] [int] NOT NULL,
	[Mean] [int] NOT NULL,
	[_90Percentile] [int] NOT NULL,
 CONSTRAINT [PK_PreCalculatedHourlyTrend] PRIMARY KEY CLUSTERED 
(
	[Date] ASC,
	[PageId] ASC,
	[Hour] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)

GO
/****** Object:  Table [dbo].[PreCalculatedPagesStatistics]    Script Date: 8/12/2013 4:13:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreCalculatedPagesStatistics](
	[Date] [date] NOT NULL,
	[PageId] [int] NOT NULL,
	[Count] [int] NOT NULL,
	[Mean] [int] NOT NULL,
	[Median] [int] NOT NULL,
	[Sum] [int] NOT NULL,
 CONSTRAINT [PK_PreCalculatedPageStatistics] PRIMARY KEY CLUSTERED 
(
	[Date] ASC,
	[PageId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)

GO
/****** Object:  Table [dbo].[Requests]    Script Date: 8/12/2013 4:13:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Requests](
	[Timestamp] [datetime2](7) NOT NULL,
	[PageId] [int] NOT NULL,
	[Uniqifier] [int] IDENTITY(1,1) NOT NULL,
	[Duration] [int] NOT NULL,
 CONSTRAINT [PK_Requests] PRIMARY KEY CLUSTERED 
(
	[Timestamp] ASC,
	[PageId] ASC,
	[Uniqifier] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
)

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [PageSHA1]    Script Date: 8/12/2013 4:13:38 PM ******/
CREATE NONCLUSTERED INDEX [PageSHA1] ON [dbo].[PageIds]
(
	[PageSHA1] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
GO
