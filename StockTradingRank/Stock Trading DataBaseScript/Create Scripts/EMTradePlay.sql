IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'EMTradePlay')
	DROP DATABASE [EMTradePlay]
GO

CREATE DATABASE [EMTradePlay]  ON (NAME = N'EMTradePlay_Data', FILENAME = N'd:\Program Files\Microsoft SQL Server\MSSQL\data\EMTradePlay_Data.MDF' , SIZE = 2, FILEGROWTH = 10%) LOG ON (NAME = N'EMTradePlay_Log', FILENAME = N'd:\Program Files\Microsoft SQL Server\MSSQL\data\EMTradePlay_Log.LDF' , SIZE = 1, FILEGROWTH = 10%)
 COLLATE Chinese_PRC_CI_AS
GO

exec sp_dboption N'EMTradePlay', N'autoclose', N'true'
GO

exec sp_dboption N'EMTradePlay', N'bulkcopy', N'false'
GO

exec sp_dboption N'EMTradePlay', N'trunc. log', N'true'
GO

exec sp_dboption N'EMTradePlay', N'torn page detection', N'true'
GO

exec sp_dboption N'EMTradePlay', N'read only', N'false'
GO

exec sp_dboption N'EMTradePlay', N'dbo use', N'false'
GO

exec sp_dboption N'EMTradePlay', N'single', N'false'
GO

exec sp_dboption N'EMTradePlay', N'autoshrink', N'true'
GO

exec sp_dboption N'EMTradePlay', N'ANSI null default', N'false'
GO

exec sp_dboption N'EMTradePlay', N'recursive triggers', N'false'
GO

exec sp_dboption N'EMTradePlay', N'ANSI nulls', N'false'
GO

exec sp_dboption N'EMTradePlay', N'concat null yields null', N'false'
GO

exec sp_dboption N'EMTradePlay', N'cursor close on commit', N'false'
GO

exec sp_dboption N'EMTradePlay', N'default to local cursor', N'false'
GO

exec sp_dboption N'EMTradePlay', N'quoted identifier', N'false'
GO

exec sp_dboption N'EMTradePlay', N'ANSI warnings', N'false'
GO

exec sp_dboption N'EMTradePlay', N'auto create statistics', N'true'
GO

exec sp_dboption N'EMTradePlay', N'auto update statistics', N'true'
GO

if( (@@microsoftversion / power(2, 24) = 8) and (@@microsoftversion & 0xffff >= 724) )
	exec sp_dboption N'EMTradePlay', N'db chaining', N'false'
GO

use [EMTradePlay]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetTradeUserListByPlayId]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetTradeUserListByPlayId]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetUserByPara]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetUserByPara]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Area]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[Area]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Award]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[Award]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Game]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[Game]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[HolidayList]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[HolidayList]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Manager]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[Manager]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Play]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[Play]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UserLevel]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[UserLevel]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UserList]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[UserList]
GO

CREATE TABLE [dbo].[Area] (
	[AreaId] [int] IDENTITY (1, 1) NOT NULL ,
	[AreaName] [nvarchar] (64) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaDescription] [nvarchar] (1024) COLLATE Chinese_PRC_CI_AS NULL ,
	[CreateDate] [datetime] NULL ,
	[AreaState] [tinyint] NULL ,
	[GameId] [int] NULL ,
	[AreaRegDateStart] [datetime] NULL ,
	[AreaRegDateEnd] [datetime] NULL ,
	[AreaDateStart] [datetime] NULL ,
	[AreaDateEnd] [datetime] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Award] (
	[AwardId] [int] IDENTITY (1, 1) NOT NULL ,
	[GameId] [int] NULL ,
	[AwardLevel] [int] NULL ,
	[AwardName] [varchar] (64) COLLATE Chinese_PRC_CI_AS NULL ,
	[AwardNum] [int] NULL ,
	[AwardState] [tinyint] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Game] (
	[GameId] [int] IDENTITY (1, 1) NOT NULL ,
	[PlayId] [int] NULL ,
	[GameName] [nvarchar] (128) COLLATE Chinese_PRC_CI_AS NULL ,
	[GameType] [tinyint] NULL ,
	[RegDateStart] [datetime] NULL ,
	[RegDateEnd] [datetime] NULL ,
	[GameDateStart] [datetime] NULL ,
	[GameDateEnd] [datetime] NULL ,
	[Description] [nvarchar] (1024) COLLATE Chinese_PRC_CI_AS NULL ,
	[GameLevel] [tinyint] NULL ,
	[GameState] [tinyint] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[HolidayList] (
	[HolidayId] [int] IDENTITY (1, 1) NOT NULL ,
	[HolidayDate] [smalldatetime] NULL ,
	[HolidayName] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Manager] (
	[ManagerId] [int] IDENTITY (1, 1) NOT NULL ,
	[ManagerName] [nvarchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[Password] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[ManagerState] [tinyint] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Play] (
	[PlayId] [int] IDENTITY (1, 1) NOT NULL ,
	[PlayName] [nvarchar] (128) COLLATE Chinese_PRC_CI_AS NULL ,
	[PlayState] [tinyint] NULL ,
	[Description] [nvarchar] (1024) COLLATE Chinese_PRC_CI_AS NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[UserLevel] (
	[LevelID] [int] NOT NULL ,
	[Percentage] [money] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[UserList] (
	[UserID] [int] IDENTITY (1, 1) NOT NULL ,
	[UserName] [varchar] (32) COLLATE Chinese_PRC_CI_AS NOT NULL ,
	[UserType] [tinyint] NULL ,
	[LevelId] [int] NULL ,
	[AreaID] [int] NOT NULL ,
	[TeamID] [int] NULL ,
	[Rtime] [datetime] NULL ,
	[RIP] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[Public] [bit] NULL ,
	[Validity] [tinyint] NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[TradeFlag] [bit] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Area] WITH NOCHECK ADD 
	CONSTRAINT [PK_AREA] PRIMARY KEY  CLUSTERED 
	(
		[AreaId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[Award] WITH NOCHECK ADD 
	CONSTRAINT [PK_AWARD] PRIMARY KEY  CLUSTERED 
	(
		[AwardId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[Game] WITH NOCHECK ADD 
	CONSTRAINT [PK_GAME] PRIMARY KEY  CLUSTERED 
	(
		[GameId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[Manager] WITH NOCHECK ADD 
	CONSTRAINT [PK_MANAGER] PRIMARY KEY  CLUSTERED 
	(
		[ManagerId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[Play] WITH NOCHECK ADD 
	CONSTRAINT [PK_PLAY] PRIMARY KEY  CLUSTERED 
	(
		[PlayId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[UserLevel] WITH NOCHECK ADD 
	CONSTRAINT [PK_USERLEVEL] PRIMARY KEY  CLUSTERED 
	(
		[LevelID]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[UserList] WITH NOCHECK ADD 
	CONSTRAINT [PK_USERLIST] PRIMARY KEY  CLUSTERED 
	(
		[UserID],
		[UserName],
		[AreaID]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[Area] ADD 
	CONSTRAINT [DF_Area_AreaState] DEFAULT (0) FOR [AreaState]
GO

ALTER TABLE [dbo].[UserList] ADD 
	CONSTRAINT [DF_UserList_UserName] DEFAULT ('') FOR [UserName],
	CONSTRAINT [DF_UserList_UserType] DEFAULT (0) FOR [UserType],
	CONSTRAINT [DF_UserList_LevelId] DEFAULT (0) FOR [LevelId],
	CONSTRAINT [DF_UserList_AreaID] DEFAULT (0) FOR [AreaID],
	CONSTRAINT [DF_UserList_TeamID] DEFAULT (0) FOR [TeamID],
	CONSTRAINT [DF_UserList_RIP] DEFAULT ('') FOR [RIP],
	CONSTRAINT [DF__UserList__Public__24927208] DEFAULT (1) FOR [Public],
	CONSTRAINT [DF__UserList__Validi__25869641] DEFAULT (1) FOR [Validity],
	CONSTRAINT [DF_UserList_UserDataBase] DEFAULT ('A') FOR [UserDataBase],
	CONSTRAINT [DF_UserList_TradeFlag] DEFAULT (0) FOR [TradeFlag],
	CONSTRAINT [DF_UserList_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE GetTradeUserListByPlayId 
@playId int
AS
/*
description:	获取指定活动中参与交易的有效用户列表
author:		totem
para:		playId 活动ID
return:		
create date:	2009-09-24
modify date:	
modify reason:	
*/
select  p.playid, g.gameid, a.areaid, u.* 
from userlist u,play p,game g,area a
where u.areaid = a.areaid
and a.gameid=g.gameid
and g.playid = p.playid
and u.validity = 1
and u.tradeflag = 1
and p.playid = @playId
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE GetUserByPara
@playId int,
@userId int
AS
/*
description:	获取指定活动中指定的用户
author:		totem
para:		playId 活动ID, userId 用户ID
return:		
create date:	2009-09-24
modify date:	
modify reason:	
*/
if @playId is null or (@playId <=0 or @playId >255)
begin
    print '无效的活动ID'
    return
end
select  p.playid, g.gameid, a.areaid, u.*
from userlist u,play p,game g,area a
where u.areaid = a.areaid
and a.gameid=g.gameid
and g.playid = p.playid
and u.validity = 1
and p.playid = @playId
and u.userid = @userId
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


exec sp_addextendedproperty N'MS_Description', '区域', N'user', N'dbo', N'table', N'Area'

GO

exec sp_addextendedproperty N'MS_Description', '区域结束时间', N'user', N'dbo', N'table', N'Area', N'column', N'AreaDateEnd'
GO
exec sp_addextendedproperty N'MS_Description', '区域开始时间', N'user', N'dbo', N'table', N'Area', N'column', N'AreaDateStart'
GO
exec sp_addextendedproperty N'MS_Description', '区域描述', N'user', N'dbo', N'table', N'Area', N'column', N'AreaDescription'
GO
exec sp_addextendedproperty N'MS_Description', '区域ID', N'user', N'dbo', N'table', N'Area', N'column', N'AreaId'
GO
exec sp_addextendedproperty N'MS_Description', '区域名称', N'user', N'dbo', N'table', N'Area', N'column', N'AreaName'
GO
exec sp_addextendedproperty N'MS_Description', '区域报名结束时间', N'user', N'dbo', N'table', N'Area', N'column', N'AreaRegDateEnd'
GO
exec sp_addextendedproperty N'MS_Description', '区域报名开始时间', N'user', N'dbo', N'table', N'Area', N'column', N'AreaRegDateStart'
GO
exec sp_addextendedproperty N'MS_Description', '区域状态(未开始/开始/结束)', N'user', N'dbo', N'table', N'Area', N'column', N'AreaState'
GO
exec sp_addextendedproperty N'MS_Description', '区域创建日期', N'user', N'dbo', N'table', N'Area', N'column', N'CreateDate'
GO
exec sp_addextendedproperty N'MS_Description', '所属比赛', N'user', N'dbo', N'table', N'Area', N'column', N'GameId'


GO


exec sp_addextendedproperty N'MS_Description', '奖品', N'user', N'dbo', N'table', N'Award'

GO

exec sp_addextendedproperty N'MS_Description', '奖品ID', N'user', N'dbo', N'table', N'Award', N'column', N'AwardId'
GO
exec sp_addextendedproperty N'MS_Description', '奖品等级', N'user', N'dbo', N'table', N'Award', N'column', N'AwardLevel'
GO
exec sp_addextendedproperty N'MS_Description', '奖品名称', N'user', N'dbo', N'table', N'Award', N'column', N'AwardName'
GO
exec sp_addextendedproperty N'MS_Description', '奖品数量', N'user', N'dbo', N'table', N'Award', N'column', N'AwardNum'
GO
exec sp_addextendedproperty N'MS_Description', '奖品状态', N'user', N'dbo', N'table', N'Award', N'column', N'AwardState'
GO
exec sp_addextendedproperty N'MS_Description', '活动ID', N'user', N'dbo', N'table', N'Award', N'column', N'GameId'


GO


exec sp_addextendedproperty N'MS_Description', '比赛', N'user', N'dbo', N'table', N'Game'

GO

exec sp_addextendedproperty N'MS_Description', '比赛说明', N'user', N'dbo', N'table', N'Game', N'column', N'Description'
GO
exec sp_addextendedproperty N'MS_Description', '比赛结束时间', N'user', N'dbo', N'table', N'Game', N'column', N'GameDateEnd'
GO
exec sp_addextendedproperty N'MS_Description', '比赛开始时间', N'user', N'dbo', N'table', N'Game', N'column', N'GameDateStart'
GO
exec sp_addextendedproperty N'MS_Description', '比赛具体ID', N'user', N'dbo', N'table', N'Game', N'column', N'GameId'
GO
exec sp_addextendedproperty N'MS_Description', '比赛等级(初赛/复赛/决赛)', N'user', N'dbo', N'table', N'Game', N'column', N'GameLevel'
GO
exec sp_addextendedproperty N'MS_Description', '比赛名称', N'user', N'dbo', N'table', N'Game', N'column', N'GameName'
GO
exec sp_addextendedproperty N'MS_Description', '比赛状态(未开始/开始/结束)', N'user', N'dbo', N'table', N'Game', N'column', N'GameState'
GO
exec sp_addextendedproperty N'MS_Description', '比赛类型(常年/季度/月度/特殊)', N'user', N'dbo', N'table', N'Game', N'column', N'GameType'
GO
exec sp_addextendedproperty N'MS_Description', '活动ID', N'user', N'dbo', N'table', N'Game', N'column', N'PlayId'
GO
exec sp_addextendedproperty N'MS_Description', '报名结束时间', N'user', N'dbo', N'table', N'Game', N'column', N'RegDateEnd'
GO
exec sp_addextendedproperty N'MS_Description', '报名开始时间', N'user', N'dbo', N'table', N'Game', N'column', N'RegDateStart'


GO


exec sp_addextendedproperty N'MS_Description', '活动', N'user', N'dbo', N'table', N'Manager'

GO

exec sp_addextendedproperty N'MS_Description', '管理员ID', N'user', N'dbo', N'table', N'Manager', N'column', N'ManagerId'
GO
exec sp_addextendedproperty N'MS_Description', '管理员用户名', N'user', N'dbo', N'table', N'Manager', N'column', N'ManagerName'
GO
exec sp_addextendedproperty N'MS_Description', '管理员状态', N'user', N'dbo', N'table', N'Manager', N'column', N'ManagerState'
GO
exec sp_addextendedproperty N'MS_Description', '密码)', N'user', N'dbo', N'table', N'Manager', N'column', N'Password'


GO


exec sp_addextendedproperty N'MS_Description', '活动', N'user', N'dbo', N'table', N'Play'

GO

exec sp_addextendedproperty N'MS_Description', '活动描述', N'user', N'dbo', N'table', N'Play', N'column', N'Description'
GO
exec sp_addextendedproperty N'MS_Description', '活动ID', N'user', N'dbo', N'table', N'Play', N'column', N'PlayId'
GO
exec sp_addextendedproperty N'MS_Description', '活动名称', N'user', N'dbo', N'table', N'Play', N'column', N'PlayName'
GO
exec sp_addextendedproperty N'MS_Description', '活动状态(未开始/开始/结束)', N'user', N'dbo', N'table', N'Play', N'column', N'PlayState'


GO


exec sp_addextendedproperty N'MS_Description', '用户等级', N'user', N'dbo', N'table', N'UserLevel'

GO

exec sp_addextendedproperty N'MS_Description', '等级ID', N'user', N'dbo', N'table', N'UserLevel', N'column', N'LevelID'
GO
exec sp_addextendedproperty N'MS_Description', '收益率', N'user', N'dbo', N'table', N'UserLevel', N'column', N'Percentage'
GO
exec sp_addextendedproperty N'MS_Description', '等级名称', N'user', N'dbo', N'table', N'UserLevel', N'column', N'Title'


GO


exec sp_addextendedproperty N'MS_Description', '用户', N'user', N'dbo', N'table', N'UserList'

GO

exec sp_addextendedproperty N'MS_Description', '区域ID', N'user', N'dbo', N'table', N'UserList', N'column', N'AreaID'
GO
exec sp_addextendedproperty N'MS_Description', '用户等级', N'user', N'dbo', N'table', N'UserList', N'column', N'LevelId'
GO
exec sp_addextendedproperty N'MS_Description', '是否公开委托信息', N'user', N'dbo', N'table', N'UserList', N'column', N'Public'
GO
exec sp_addextendedproperty N'MS_Description', N'低于持仓标准的天数', N'user', N'dbo', N'table', N'UserList', N'column', N'RatioUnderDays'
GO
exec sp_addextendedproperty N'MS_Description', '报名IP', N'user', N'dbo', N'table', N'UserList', N'column', N'RIP'
GO
exec sp_addextendedproperty N'MS_Description', '报名时间', N'user', N'dbo', N'table', N'UserList', N'column', N'Rtime'
GO
exec sp_addextendedproperty N'MS_Description', '小组ID', N'user', N'dbo', N'table', N'UserList', N'column', N'TeamID'
GO
exec sp_addextendedproperty N'MS_Description', N'是否交易标志', N'user', N'dbo', N'table', N'UserList', N'column', N'TradeFlag'
GO
exec sp_addextendedproperty N'MS_Description', '用户订单数据库标志', N'user', N'dbo', N'table', N'UserList', N'column', N'UserDataBase'
GO
exec sp_addextendedproperty N'MS_Description', '用户名', N'user', N'dbo', N'table', N'UserList', N'column', N'UserName'
GO
exec sp_addextendedproperty N'MS_Description', '用户类别(用以区别测试账号)', N'user', N'dbo', N'table', N'UserList', N'column', N'UserType'
GO
exec sp_addextendedproperty N'MS_Description', '标志（控制参赛资格）', N'user', N'dbo', N'table', N'UserList', N'column', N'Validity'


GO

 