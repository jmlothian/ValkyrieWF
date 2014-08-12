USE [master]
GO
/****** Object:  Database [ValkyrieWF]    Script Date: 8/12/2014 12:11:36 PM ******/
CREATE DATABASE [ValkyrieWF] ON  PRIMARY 
( NAME = N'ValkyrieWF', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10.VALKYRIE\MSSQL\DATA\ValkyrieWF.mdf' , SIZE = 16384KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'ValkyrieWF_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10.VALKYRIE\MSSQL\DATA\ValkyrieWF_log.ldf' , SIZE = 112384KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [ValkyrieWF] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ValkyrieWF].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ValkyrieWF] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ValkyrieWF] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ValkyrieWF] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ValkyrieWF] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ValkyrieWF] SET ARITHABORT OFF 
GO
ALTER DATABASE [ValkyrieWF] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ValkyrieWF] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ValkyrieWF] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ValkyrieWF] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ValkyrieWF] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ValkyrieWF] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ValkyrieWF] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ValkyrieWF] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ValkyrieWF] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ValkyrieWF] SET  DISABLE_BROKER 
GO
ALTER DATABASE [ValkyrieWF] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ValkyrieWF] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ValkyrieWF] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ValkyrieWF] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ValkyrieWF] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ValkyrieWF] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ValkyrieWF] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ValkyrieWF] SET RECOVERY FULL 
GO
ALTER DATABASE [ValkyrieWF] SET  MULTI_USER 
GO
ALTER DATABASE [ValkyrieWF] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ValkyrieWF] SET DB_CHAINING OFF 
GO
EXEC sys.sp_db_vardecimal_storage_format N'ValkyrieWF', N'ON'
GO
USE [ValkyrieWF]
GO
/****** Object:  Table [dbo].[ExceptionStepHistory]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExceptionStepHistory](
	[WFExceptionStepHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[InstanceKey] [varchar](12) NULL,
	[WFTemplateID] [int] NULL,
	[WFExceptionStepID] [int] NULL,
	[WFTemplateStepID_From] [int] NULL,
	[ExceptionReason] [varchar](50) NULL,
	[ExceptionComments] [text] NULL,
	[Status] [varchar](12) NULL,
	[UserID_By] [int] NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[UserID_Completed] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExceptionSteps]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExceptionSteps](
	[ExceptionStepID] [int] IDENTITY(1,1) NOT NULL,
	[InstanceKey] [varchar](12) NULL,
	[WFTemplateID] [int] NULL,
	[WFExceptionStepID] [int] NULL,
	[WFTemplateStepID] [int] NULL,
	[Reason] [varchar](50) NULL,
	[Comments] [text] NULL,
	[Status] [varchar](12) NULL,
	[UserID_From] [int] NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[UserID_Completed] [int] NULL,
 CONSTRAINT [PK_wf_jeop] PRIMARY KEY CLUSTERED 
(
	[ExceptionStepID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PendingStatusUpdates]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PendingStatusUpdates](
	[PendingStatusUpdateID] [int] IDENTITY(1,1) NOT NULL,
	[WFTemplateID] [int] NOT NULL,
	[WFTemplateStepID] [int] NOT NULL,
	[InstanceKey] [varchar](16) NOT NULL,
	[NewStatus] [varchar](50) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[StepHistory]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StepHistory](
	[StepHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[InstanceKey] [varchar](12) NOT NULL,
	[WFTemplateID] [int] NULL,
	[Status] [varchar](50) NULL,
	[WFTemplateStepID] [int] NULL,
	[UserID] [int] NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[ExceptionID] [int] NULL,
 CONSTRAINT [PK_wf_history] PRIMARY KEY CLUSTERED 
(
	[StepHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[StepLinks]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StepLinks](
	[StepLinkID] [int] IDENTITY(1,1) NOT NULL,
	[WFTemplateID] [int] NOT NULL,
	[WFTemplateStepID_From] [int] NOT NULL,
	[WFTEmplateStepID_To] [int] NOT NULL,
	[LinkType] [char](1) NOT NULL,
	[Label] [varchar](50) NULL,
 CONSTRAINT [PK_wf_links] PRIMARY KEY CLUSTERED 
(
	[StepLinkID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[StepStatus]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StepStatus](
	[InstanceKey] [varchar](12) NOT NULL,
	[WFTemplateID] [int] NOT NULL,
	[Status] [varchar](50) NOT NULL,
	[WFTemplateStepID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[SyncCount] [int] NOT NULL,
	[WFIdentity] [varchar](100) NULL,
	[EndTime] [datetime] NULL,
	[ExceptionID] [int] NOT NULL,
	[ExceptionToID] [int] NULL,
	[ExceptionFlag] [bit] NULL,
 CONSTRAINT [PK_wf_status] PRIMARY KEY CLUSTERED 
(
	[InstanceKey] ASC,
	[WFTemplateID] ASC,
	[WFTemplateStepID] ASC,
	[ExceptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UserGroups]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UserGroups](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](50) NOT NULL,
	[manager] [int] NOT NULL CONSTRAINT [DF_wf_group_manager]  DEFAULT ((0)),
	[superuser] [bit] NOT NULL CONSTRAINT [DF_wf_group_superuser]  DEFAULT ((0)),
 CONSTRAINT [PK_wf_group] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Users]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](30) NULL,
	[PasswordHash] [varchar](30) NULL,
	[FirstName] [varchar](30) NULL,
	[LastName] [varchar](30) NULL,
	[IsSuperUser] [bit] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_wf_users] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[WFInstances]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[WFInstances](
	[WFInstanceID] [int] IDENTITY(1,1) NOT NULL,
	[InstanceKey] [varchar](16) NOT NULL,
	[Status] [varchar](50) NOT NULL,
	[WFType] [varchar](50) NOT NULL,
 CONSTRAINT [PK_WFInstances] PRIMARY KEY CLUSTERED 
(
	[InstanceKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[WFTemplateSteps]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[WFTemplateSteps](
	[WFTemplateStepID] [int] NOT NULL,
	[GroupID] [int] NOT NULL,
	[WFTemplateID] [int] NOT NULL,
	[Name] [varchar](75) NOT NULL,
	[IsFirstStep] [bit] NULL,
	[OnExecuteCall] [varchar](50) NULL,
	[Description] [text] NOT NULL,
	[Color] [char](7) NOT NULL CONSTRAINT [DF_wf_steps_color]  DEFAULT ('#cccccc'),
	[X] [decimal](18, 5) NOT NULL,
	[Y] [decimal](18, 5) NOT NULL,
	[SyncCount] [int] NOT NULL,
	[StepType] [varchar](50) NULL,
 CONSTRAINT [PK_wf_steps] PRIMARY KEY CLUSTERED 
(
	[WFTemplateStepID] ASC,
	[WFTemplateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[WorkflowTemplates]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[WorkflowTemplates](
	[WFTemplateID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](75) NOT NULL,
	[Version] [decimal](18, 5) NULL,
	[InProduction] [bit] NOT NULL,
	[InProductionSince] [datetime] NULL,
	[WFType] [varchar](25) NOT NULL,
	[Description] [varchar](25) NULL,
 CONSTRAINT [PK_workflow] PRIMARY KEY CLUSTERED 
(
	[WFTemplateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[wf_statname]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE   view [dbo].[wf_statname] as select wfs.id,wfs.wf_id,wfs.status,wfstp.name,wfs.wf_step_id,wfs.user_id,wfs.wait_count,wfs.start_time,wfs.end_time,wfstp.step_function,wfstp.group_id,wf_identity,jeop_id,jeop_to,jeop_flag,description from wf_status wfs inner join wf_steps wfstp on wfstp.wf_id=wfs.wf_id and wfstp.id=wfs.wf_step_id --where id='Q6UJ9A0298S3'








GO
ALTER TABLE [dbo].[StepHistory] ADD  CONSTRAINT [DF_wf_history_jeop_id]  DEFAULT ((0)) FOR [ExceptionID]
GO
ALTER TABLE [dbo].[StepStatus] ADD  CONSTRAINT [DF_wf_status_jeop_id]  DEFAULT ((0)) FOR [ExceptionID]
GO
ALTER TABLE [dbo].[StepStatus] ADD  CONSTRAINT [DF_wf_status_jeop_flag]  DEFAULT ((0)) FOR [ExceptionFlag]
GO
ALTER TABLE [dbo].[StepStatus]  WITH NOCHECK ADD  CONSTRAINT [FK_wf_status_wf_steps] FOREIGN KEY([WFTemplateStepID], [WFTemplateID])
REFERENCES [dbo].[WFTemplateSteps] ([WFTemplateStepID], [WFTemplateID])
GO
ALTER TABLE [dbo].[StepStatus] CHECK CONSTRAINT [FK_wf_status_wf_steps]
GO
ALTER TABLE [dbo].[StepStatus]  WITH NOCHECK ADD  CONSTRAINT [FK_wf_status_wf_users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[StepStatus] CHECK CONSTRAINT [FK_wf_status_wf_users]
GO
ALTER TABLE [dbo].[WFTemplateSteps]  WITH NOCHECK ADD  CONSTRAINT [FK_wf_steps_workflow] FOREIGN KEY([WFTemplateID])
REFERENCES [dbo].[WorkflowTemplates] ([WFTemplateID])
NOT FOR REPLICATION 
GO
ALTER TABLE [dbo].[WFTemplateSteps] NOCHECK CONSTRAINT [FK_wf_steps_workflow]
GO
/****** Object:  StoredProcedure [dbo].[GetPendingWFInstances]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetPendingWFInstances]
	-- Add the parameters for the stored procedure here

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT WFInstanceID, InstanceKey, [Status], WFType from WFInstances;
END

GO
/****** Object:  StoredProcedure [dbo].[InsertUpdates]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[InsertUpdates] (
	-- Add the parameters for the stored procedure here
	@InstanceKey varchar(16),
	@WFTemplateID int,
	@NewStatus varchar(50),
	@WFTemplateStepID int	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO ValkyrieWF.[dbo].PendingStatusUpdates (InstanceKey,
	WFTemplateID,
	[NewStatus],
	WFTemplateStepID
) VALUES (@InstanceKey,
	@WFTemplateID,
	@NewStatus,
	@WFTemplateStepID);
END

GO
/****** Object:  StoredProcedure [dbo].[InsertWFInstance]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[InsertWFInstance] (
	-- Add the parameters for the stored procedure here
	@InstanceKey char(12),
	@WFTemplateID int,
	@Status varchar(50),
	@WFTemplateStepID int,
	@UserID int,
	@StartTime datetime,
	@SyncCount int,
	@ExceptionID int	
	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO ValkyrieWF.[dbo].[StepStatus] (InstanceKey,
	WFTemplateID,
	[Status],
	WFTemplateStepID,
	UserID,
	StartTime,
	SyncCount,
	ExceptionID) VALUES (@InstanceKey,
	@WFTemplateID,
	@Status,
	@WFTemplateStepID,
	@UserID,
	@StartTime,
	@SyncCount,
	@ExceptionID);
END

GO
/****** Object:  StoredProcedure [dbo].[LoadChildSteps]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[LoadChildSteps] (
	-- Add the parameters for the stored procedure here
	@WFTemplateID int,
	@WFTemplateStepID int)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select * from StepLinks as sl 
	LEFT JOIN WFTemplateSteps AS ws ON (sl.WFTEmplateStepID_To=ws.WFTemplateStepID AND sl.WFTemplateID=ws.WFTemplateID) 
	where WFTEmplateStepID_From=@WFTemplateStepID AND sl.WFTemplateID=@WFTemplateID
END

GO
/****** Object:  StoredProcedure [dbo].[LoadWFTemplateFirstStep]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[LoadWFTemplateFirstStep] (
	-- Add the parameters for the stored procedure here
	@WFTemplateID int)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT WFTemplateStepID, OnExecuteCall, Name, IsFirstStep, GroupID, SyncCount  FROM WFTemplateSteps WHERE WFTemplateID=@WFTemplateID AND IsFirstStep=1;
END

GO
/****** Object:  StoredProcedure [dbo].[LoadWFTemplates]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[LoadWFTemplates]
	-- Add the parameters for the stored procedure here

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT WFTemplateID, Name, [Version], InProduction, InProductionSince, WFType, [Description] FROM [dbo].[WorkflowTemplates];
END

GO
/****** Object:  StoredProcedure [dbo].[RunUpdates]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[RunUpdates] 
	-- Add the parameters for the stored procedure here

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

UPDATE StepStatus 
SET StepStatus.[status] = PendingStatusUpdates.[NewStatus] FROM PendingStatusUpdates 
WHERE 
(cast(StepStatus.WFTemplateID as varchar(50)) +'-'+ cast(StepStatus.WFTemplateStepID as varchar(50)) +'-'+  StepStatus.InstanceKey)
IN (select (cast(PendingStatusUpdates.WFTemplateID as varchar(50)) +'-'+ cast(PendingStatusUpdates.WFTemplateStepID as varchar(50)) +'-' + InstanceKey) as tmp FROM PendingStatusUpdates)
	AND PendingStatusUpdates.WFTemplateID=StepStatus.WFTemplateID 
    AND PendingStatusUpdates.WFTemplateStepID=StepStatus.WFTemplateStepID 
    AND PendingStatusUpdates.InstanceKey=StepStatus.InstanceKey

delete from PendingStatusUpdates;
END

GO
/****** Object:  StoredProcedure [dbo].[StartWFInstance]    Script Date: 8/12/2014 12:11:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[StartWFInstance] (
	-- Add the parameters for the stored procedure here
	@WFInstanceID int)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

UPDATE WFInstances SET [Status]='Active' WHERE WFInstanceID=@WFInstanceID;

END

GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Value passed to StepRunner to be used to call external scripts on execute' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'WFTemplateSteps', @level2type=N'COLUMN',@level2name=N'OnExecuteCall'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Logical or semantic type of step' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'WFTemplateSteps', @level2type=N'COLUMN',@level2name=N'StepType'
GO
USE [master]
GO
ALTER DATABASE [ValkyrieWF] SET  READ_WRITE 
GO
