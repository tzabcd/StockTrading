IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'EMTradeRank2')
	DROP DATABASE [EMTradeRank2]
GO

CREATE DATABASE [EMTradeRank2]  ON (NAME = N'EMTradeRank2_Data', FILENAME = N'd:\Program Files\Microsoft SQL Server\MSSQL\data\EMTradeRank2_Data.MDF' , SIZE = 7, FILEGROWTH = 10%) LOG ON (NAME = N'EMTradeRank2_Log', FILENAME = N'd:\Program Files\Microsoft SQL Server\MSSQL\data\EMTradeRank2_Log.LDF' , SIZE = 146, FILEGROWTH = 10%)
 COLLATE Chinese_PRC_CI_AS
GO

exec sp_dboption N'EMTradeRank2', N'autoclose', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'bulkcopy', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'trunc. log', N'true'
GO

exec sp_dboption N'EMTradeRank2', N'torn page detection', N'true'
GO

exec sp_dboption N'EMTradeRank2', N'read only', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'dbo use', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'single', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'autoshrink', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'ANSI null default', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'recursive triggers', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'ANSI nulls', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'concat null yields null', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'cursor close on commit', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'default to local cursor', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'quoted identifier', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'ANSI warnings', N'false'
GO

exec sp_dboption N'EMTradeRank2', N'auto create statistics', N'true'
GO

exec sp_dboption N'EMTradeRank2', N'auto update statistics', N'true'
GO

if( (@@microsoftversion / power(2, 24) = 8) and (@@microsoftversion & 0xffff >= 724) )
	exec sp_dboption N'EMTradeRank2', N'db chaining', N'false'
GO

use [EMTradeRank2]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetLastWorkDayOfCurrentDay]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[GetLastWorkDayOfCurrentDay]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetLastWorkDayOfLastMonth]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[GetLastWorkDayOfLastMonth]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetLastWorkDayOfLastWeek]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[GetLastWorkDayOfLastWeek]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_addtosourcecontrol_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_addtosourcecontrol_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_checkinobject_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_checkinobject_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_checkoutobject_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_checkoutobject_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_isundersourcecontrol_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_isundersourcecontrol_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_validateloginparams_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_validateloginparams_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_whocheckedout_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_whocheckedout_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_addtosourcecontrol]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_addtosourcecontrol]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_checkinobject]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_checkinobject]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_checkoutobject]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_checkoutobject]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_getpropertiesbyid_vcs_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_getpropertiesbyid_vcs_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_isundersourcecontrol]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_isundersourcecontrol]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_removefromsourcecontrol]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_removefromsourcecontrol]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_setpropertybyid_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_setpropertybyid_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_validateloginparams]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_validateloginparams]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_whocheckedout]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_whocheckedout]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[CompHistoryRank]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[CompHistoryRank]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetLastDayRank]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetLastDayRank]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetLastDayWealth]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetLastDayWealth]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetLastDayWealthByAreaId]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetLastDayWealthByAreaId]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetLastMonthWealth]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetLastMonthWealth]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetLastWeekWealth]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetLastWeekWealth]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[ImportHistoryRank]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[ImportHistoryRank]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[IsQuoteDate]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[IsQuoteDate]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_adduserobject]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_adduserobject]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_adduserobject_vcs]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_adduserobject_vcs]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_displayoaerror_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_displayoaerror_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_droppropertiesbyid]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_droppropertiesbyid]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_dropuserobjectbyid]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_dropuserobjectbyid]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_generateansiname]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_generateansiname]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_getobjwithprop]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_getobjwithprop]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_getobjwithprop_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_getobjwithprop_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_getpropertiesbyid]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_getpropertiesbyid]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_getpropertiesbyid_u]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_getpropertiesbyid_u]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_getpropertiesbyid_vcs]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_getpropertiesbyid_vcs]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_setpropertybyid]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_setpropertybyid]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[CreateHistoryRankByDate]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[CreateHistoryRankByDate]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetLastQuoteDate]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetLastQuoteDate]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetUserWealth2]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetUserWealth2]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RestoreLostUser]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[RestoreLostUser]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[ShowAllRegUserCount]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[ShowAllRegUserCount]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[ShowAllSimUserCount]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[ShowAllSimUserCount]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[ShowLostUser]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[ShowLostUser]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[ShowSimNoTradeUserFund]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[ShowSimNoTradeUserFund]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[ShowSimUserFund]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[ShowSimUserFund]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[ShowSimUserOrders]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[ShowSimUserOrders]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[ShowSimUserStocks]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[ShowSimUserStocks]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_displayoaerror]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_displayoaerror]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_vcsenabled]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_vcsenabled]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_verstamp006]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_verstamp006]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dt_verstamp007]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[dt_verstamp007]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DailyRank]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[DailyRank]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[emtradeplay].[HistoryRank_090914]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [emtradeplay].[HistoryRank_090914]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[emtradeplay].[HistoryRank_090915]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [emtradeplay].[HistoryRank_090915]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[emtradeplay].[HistoryRank_090916]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [emtradeplay].[HistoryRank_090916]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[emtradeplay].[HistoryRank_090917]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [emtradeplay].[HistoryRank_090917]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[emtradeplay].[HistoryRank_090918]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [emtradeplay].[HistoryRank_090918]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[emtradeplay].[HistoryRank_090921]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [emtradeplay].[HistoryRank_090921]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[emtradeplay].[HistoryRank_090922]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [emtradeplay].[HistoryRank_090922]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[emtradeplay].[HistoryRank_090923]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [emtradeplay].[HistoryRank_090923]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[HolidayList]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[HolidayList]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UserLevel]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[UserLevel]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UserRankHistory]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[UserRankHistory]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[dtproperties]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[dtproperties]
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE FUNCTION dbo.GetLastWorkDayOfCurrentDay (@CurrentDay datetime)  
RETURNS datetime AS  
BEGIN 
     DECLARE @dLastDay datetime
     SELECT @dLastDay = 
     case datepart(w,@CurrentDay)
        WHEN 1 THEN convert(char(10),dateadd(d,5-datepart(w,@CurrentDay+1)-7+1,@CurrentDay+1),121)
        WHEN 2 THEN convert(char(10),dateadd(d,5-datepart(w,@CurrentDay+1)-7+1,@CurrentDay+1),121)
        ELSE convert(char(10),@CurrentDay-1,121)
     END 
     RETURN @dLastDay
END


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE FUNCTION dbo.GetLastWorkDayOfLastMonth (@CurrentDay datetime)  
RETURNS datetime AS  
BEGIN 
    DECLARE @retDay datetime
    SELECT @retDay = 
    CONVERT(char(10),dateadd(d,5-datepart(w,@CurrentDay-day(@CurrentDay)+1)-7+1,@CurrentDay-day(@CurrentDay)+1),121)
    RETURN @retDay
END

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE FUNCTION dbo.GetLastWorkDayOfLastWeek (@CurrentDay datetime)  
RETURNS datetime AS  
BEGIN 
     DECLARE @retDay datetime
      SELECT @retDay = 
      CONVERT(char(10),dateadd(d,5-datepart(w,@CurrentDay+1)-7+1,@CurrentDay+1),121)
     RETURN @retDay
END



GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

CREATE TABLE [dbo].[DailyRank] (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [emtradeplay].[HistoryRank_090914] (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [emtradeplay].[HistoryRank_090915] (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [emtradeplay].[HistoryRank_090916] (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [emtradeplay].[HistoryRank_090917] (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [emtradeplay].[HistoryRank_090918] (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [emtradeplay].[HistoryRank_090921] (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [emtradeplay].[HistoryRank_090922] (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [emtradeplay].[HistoryRank_090923] (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[HolidayList] (
	[HolidayId] [int] IDENTITY (1, 1) NOT NULL ,
	[HolidayDate] [smalldatetime] NULL ,
	[HolidayName] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[UserLevel] (
	[LevelID] [int] IDENTITY (1, 1) NOT NULL ,
	[Percentage] [money] NOT NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NOT NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[UserRankHistory] (
	[UserID] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[RankID] [int] NULL ,
	[RankChanged] [int] NULL ,
	[AreaRankId] [int] NULL ,
	[AreaRankChanged] [int] NULL ,
	[RankDate] [datetime] NOT NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[dtproperties] (
	[id] [int] IDENTITY (1, 1) NOT NULL ,
	[objectid] [int] NULL ,
	[property] [varchar] (64) COLLATE Chinese_PRC_CI_AS NOT NULL ,
	[value] [varchar] (255) COLLATE Chinese_PRC_CI_AS NULL ,
	[uvalue] [nvarchar] (255) COLLATE Chinese_PRC_CI_AS NULL ,
	[lvalue] [image] NULL ,
	[version] [int] NOT NULL 
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[DailyRank] WITH NOCHECK ADD 
	CONSTRAINT [PK_DailyRank] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [emtradeplay].[HistoryRank_090914] WITH NOCHECK ADD 
	CONSTRAINT [PK_HistoryRank_090914] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [emtradeplay].[HistoryRank_090915] WITH NOCHECK ADD 
	CONSTRAINT [PK_HistoryRank_090915] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [emtradeplay].[HistoryRank_090916] WITH NOCHECK ADD 
	CONSTRAINT [PK_HistoryRank_090916] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [emtradeplay].[HistoryRank_090917] WITH NOCHECK ADD 
	CONSTRAINT [PK_HistoryRank_090917] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [emtradeplay].[HistoryRank_090918] WITH NOCHECK ADD 
	CONSTRAINT [PK_HistoryRank_090918] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [emtradeplay].[HistoryRank_090921] WITH NOCHECK ADD 
	CONSTRAINT [PK_HistoryRank_090921] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [emtradeplay].[HistoryRank_090922] WITH NOCHECK ADD 
	CONSTRAINT [PK_HistoryRank_090922] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [emtradeplay].[HistoryRank_090923] WITH NOCHECK ADD 
	CONSTRAINT [PK_HistoryRank_090923] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[UserLevel] WITH NOCHECK ADD 
	CONSTRAINT [PK_UserLevel] PRIMARY KEY  CLUSTERED 
	(
		[LevelID]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[UserRankHistory] WITH NOCHECK ADD 
	CONSTRAINT [PK_RANKHISTORY] PRIMARY KEY  CLUSTERED 
	(
		[UserID],
		[RankDate]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[dtproperties] WITH NOCHECK ADD 
	CONSTRAINT [pk_dtproperties] PRIMARY KEY  CLUSTERED 
	(
		[id],
		[property]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[DailyRank] ADD 
	CONSTRAINT [DF_DailyRank_RankId] DEFAULT (0) FOR [RankId],
	CONSTRAINT [DF_DailyRank_RankChanged] DEFAULT (0) FOR [RankChanged],
	CONSTRAINT [DF_DailyRank_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_DailyRank_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_DailyRank_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [emtradeplay].[HistoryRank_090914] ADD 
	CONSTRAINT [DF_HistoryRank_090914_RankId] DEFAULT (0) FOR [RankId],
	CONSTRAINT [DF_HistoryRank_090914_RankChanged] DEFAULT (0) FOR [RankChanged],
	CONSTRAINT [DF_HistoryRank_090914_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_HistoryRank_090914_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_HistoryRank_090914_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [emtradeplay].[HistoryRank_090915] ADD 
	CONSTRAINT [DF_HistoryRank_090915_RankId] DEFAULT (0) FOR [RankId],
	CONSTRAINT [DF_HistoryRank_090915_RankChanged] DEFAULT (0) FOR [RankChanged],
	CONSTRAINT [DF_HistoryRank_090915_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_HistoryRank_090915_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_HistoryRank_090915_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [emtradeplay].[HistoryRank_090916] ADD 
	CONSTRAINT [DF_HistoryRank_090916_RankId] DEFAULT (0) FOR [RankId],
	CONSTRAINT [DF_HistoryRank_090916_RankChanged] DEFAULT (0) FOR [RankChanged],
	CONSTRAINT [DF_HistoryRank_090916_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_HistoryRank_090916_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_HistoryRank_090916_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [emtradeplay].[HistoryRank_090917] ADD 
	CONSTRAINT [DF_HistoryRank_090917_RankId] DEFAULT (0) FOR [RankId],
	CONSTRAINT [DF_HistoryRank_090917_RankChanged] DEFAULT (0) FOR [RankChanged],
	CONSTRAINT [DF_HistoryRank_090917_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_HistoryRank_090917_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_HistoryRank_090917_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [emtradeplay].[HistoryRank_090918] ADD 
	CONSTRAINT [DF_HistoryRank_090918_RankId] DEFAULT (0) FOR [RankId],
	CONSTRAINT [DF_HistoryRank_090918_RankChanged] DEFAULT (0) FOR [RankChanged],
	CONSTRAINT [DF_HistoryRank_090918_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_HistoryRank_090918_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_HistoryRank_090918_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [emtradeplay].[HistoryRank_090921] ADD 
	CONSTRAINT [DF_HistoryRank_090921_RankId] DEFAULT (0) FOR [RankId],
	CONSTRAINT [DF_HistoryRank_090921_RankChanged] DEFAULT (0) FOR [RankChanged],
	CONSTRAINT [DF_HistoryRank_090921_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_HistoryRank_090921_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_HistoryRank_090921_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [emtradeplay].[HistoryRank_090922] ADD 
	CONSTRAINT [DF_HistoryRank_090922_RankId] DEFAULT (0) FOR [RankId],
	CONSTRAINT [DF_HistoryRank_090922_RankChanged] DEFAULT (0) FOR [RankChanged],
	CONSTRAINT [DF_HistoryRank_090922_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_HistoryRank_090922_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_HistoryRank_090922_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [emtradeplay].[HistoryRank_090923] ADD 
	CONSTRAINT [DF_HistoryRank_090923_RankId] DEFAULT (0) FOR [RankId],
	CONSTRAINT [DF_HistoryRank_090923_RankChanged] DEFAULT (0) FOR [RankChanged],
	CONSTRAINT [DF_HistoryRank_090923_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_HistoryRank_090923_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_HistoryRank_090923_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [dbo].[UserRankHistory] ADD 
	CONSTRAINT [DF_UserRankHistory_AreaRankId] DEFAULT (0) FOR [AreaRankId],
	CONSTRAINT [DF_UserRankHistory_AreaRankChanged] DEFAULT (0) FOR [AreaRankChanged],
	CONSTRAINT [DF_UserRankHistory_RatioUnderDays] DEFAULT (0) FOR [RatioUnderDays]
GO

ALTER TABLE [dbo].[dtproperties] ADD 
	CONSTRAINT [DF__dtpropert__versi__7D78A4E7] DEFAULT (0) FOR [version]
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO


CREATE PROCEDURE [dbo].[CreateHistoryRankByDate]
/*
  **Description : Create table of current historyrank
  **para:
  **return:
  **author：totem
  **create date：2009-09-12
  **update memo:
attention：
*/
AS

declare @sql varchar(8000) , @tbName varchar(32),@tbDate varchar(10) 

set @tbName = 'HistoryRank_'+convert(varchar(10),getdate(),12)

print '================================================='

 set @sql='if exists (select 1
            from sysobjects
            where id = object_id('''+@tbName+''')
            and   type = ''U'')
   drop table '+@tbName+''
exec(@sql)

set @sql='CREATE TABLE '+@tbName+' (
	[UserId] [int] NOT NULL ,
	[AreaId] [int] NULL ,
	[UserName] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[UserDataBase] [varchar] (16) COLLATE Chinese_PRC_CI_AS NULL ,
	[RankId] [int] NULL CONSTRAINT [DF_'+@tbName+'_RankId] DEFAULT (0),
	[RankChanged] [int] NULL CONSTRAINT [DF_'+@tbName+'_RankChanged] DEFAULT (0),
	[AreaRankId] [int] NULL CONSTRAINT [DF_'+@tbName+'_AreaRankId] DEFAULT (0),
	[AreaRankChanged] [int] NULL CONSTRAINT [DF_'+@tbName+'_AreaRankChanged] DEFAULT (0),
	[RankDate] [datetime] NULL ,
	[Title] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[AreaTitle] [nvarchar] (32) COLLATE Chinese_PRC_CI_AS NULL ,
	[Wealth] [money] NULL ,
	[WealthRMB] [money] NULL ,
	[WealthUSD] [money] NULL ,
	[WealthHKD] [money] NULL ,
	[StockWealth] [money] NULL ,
	[Profit] [money] NULL ,
	[DailyProfit] [money] NULL ,
	[WeeklyProfit] [money] NULL ,
	[MonthlyProfit] [money] NULL ,
	[RatioRMB] [money] NULL ,
	[RatioUSD] [money] NULL ,
	[RatioHKD] [money] NULL ,
	[RatioUnderDays] [int] NULL CONSTRAINT [DF_'+@tbName+'_RatioUnderDays] DEFAULT (0),
	CONSTRAINT [PK_'+@tbName+'] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
) ON [PRIMARY]
'
exec(@sql)
print 'create the '+@tbName+':ok'


 set @sql='insert '+@tbName+' select * from dailyrank'
 exec(@sql)
 print 'backup current data into '+@tbName+' successed : :ok'
print '================================================='

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO



CREATE PROC GetLastQuoteDate
@CurrDay smalldatetime ,
@LastQuoteDate smalldatetime output
AS
/*
description :    获取指定日期的上一有行情的日期
author :    totem
create date :   2009-09-16
*/
declare @HolidayName varchar(16)
declare @bSuccessed bit

set @LastQuoteDate = dateadd(d,-1,@CurrDay)
set @bSuccessed = 0

while @bSuccessed = 0
begin
    if datepart(w,@LastQuoteDate)=7 or datepart(w,@LastQuoteDate)=1
    begin
        set @LastQuoteDate = dateadd(d,-1,@LastQuoteDate)
        continue
    end
    set @HolidayName = null
    select @HolidayName = holidayName from holidaylist where holidaydate = convert(varchar(10),@LastQuoteDate,121)
    if @HolidayName is null 
        break
    else
        set @LastQuoteDate = dateadd(d,-1,@LastQuoteDate)
end
 


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE PROC GetUserWealth2
AS
declare @tbCurr varchar(32), @currDate smalldatetime, @tbExists varchar(32), @sqlSelect varchar(2048)
set @currDate = getdate()
while 1 = 1
begin
	set @tbCurr ='HistoryRank_'+convert(varchar(6),@currDate,12)
	set @tbExists = null
	SELECT @tbExists = name FROM sysobjects WHERE  name = ''+@tbCurr+ '' AND  type = 'U'
	if @tbExists is null
	    set @currDate =  dateadd(d,-1,@currDate)
	else
	    break
end 
print @tbCurr
set @sqlSelect = '
SELECT TOP 100 PERCENT *
FROM (SELECT userid, 1000000 AS wealth, 1000000 AS wealthrmb, 0 AS wealthusd, 
              0 AS wealthhkd
        FROM emtradeplay.dbo.userlist
        WHERE areaid IN (4, 5, 6) AND userid NOT IN
                  (SELECT userid
                 FROM emtradeplay.'+@tbCurr+')
        UNION ALL
        SELECT userid, wealth, wealthrmb, wealthusd, wealthhkd
        FROM emtradeplay.'+@tbCurr+'
        WHERE areaid IN (4, 5, 6)) a
ORDER BY userid
'
exec(@sqlSelect)
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO



CREATE PROC RestoreLostUser AS

Set ANSI_NULLS ON
Set ANSI_WARNINGS ON

declare @tbName varchar(32), @sql varchar(8000)
declare @i int,@iMax int 
set @sql = ''
set @i = 52	set @iMax=58
while @i <= @iMax
begin
	set @tbName = 'userfund'
	set @sql = 'insert 
opendataSource(''SQLOLEDB'', ''server=192.168.1.'+convert(varchar(2),@i)+';uid=emtradeplay;pwd=eastmoney201'').stocktrading2.dbo.'+@tbName+''+char(13)+'
(userid,cash,usablecash,currency)
		      select userid,1000000.0000,1000000.0000,9 from emtradeplay.dbo.userlist 
		      where userdatabase = '''+char(65+@i-52)+'''
		      and userid not in 
		      (
		      select userid from 
	              opendataSource(''SQLOLEDB'', ''server=192.168.1.'+convert(varchar(2),@i)+';uid=emtradeplay;pwd=eastmoney201'').stocktrading2.dbo.'+@tbName+''+char(13)+'
		      )'
exec(@sql)
set @i = @i + 1
end
Set ANSI_NULLS OFF
Set ANSI_WARNINGS OFF

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE ShowAllRegUserCount AS
/*
description:	所有注册用户
author:		totem
create date:	2009-09-15
*/
Set ANSI_NULLS ON
Set ANSI_WARNINGS ON

select * from (
select userdatabase as DBCHAR,count(*) as UserCount from emtradeplay.dbo.userlist 
group by userdatabase
union all 
select 'Total = ',count(*)
from emtradeplay.dbo.userlist 
) a
order by dbchar 
Set ANSI_NULLS OFF
Set ANSI_WARNINGS OFF

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE dbo.ShowAllSimUserCount AS
/*
description:	所有撮合系统用户
author:		totem
create date:	2009-09-15
*/
  SET   ANSI_WARNINGS   ON   
  SET   ANSI_NULLS         ON  

declare @tbName varchar(32), @sql varchar(8000),@sqltem varchar(1024)
declare @i int,@iMax int 
set @sql = ''	set @sqltem= ''
set @i = 52	set @iMax=58
while @i <= @iMax
begin
	set @tbName = 'userfund'
	set @sqltem = 'select ''server_'+convert(varchar(2),@i)+''' as ServerName, count(*) as UserCount,'''+char(65+@i-52)+''' as DBCHAR from 
	opendataSource(''SQLOLEDB'', ''server=192.168.1.'+convert(varchar(2),@i)+';uid=emtradeplay;pwd=eastmoney201'').stocktrading2.dbo.'+@tbName+''+char(13)
	if @i < @iMax 
	  set @sql = @sql + @sqltem + ' union all '+char(13)
	else
	  set @sql = @sql + @sqltem +char(13)
	set @i = @i + 1
end 
set @sql =@sql +'union all select ''  Total = '', sum(unTb.userCount), '''' from ( '+@sql+') unTb '
exec(@sql)
  SET   ANSI_WARNINGS   OFF   
  SET   ANSI_NULLS         OFF

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO


CREATE PROCEDURE ShowLostUser AS
/*
description:	查找丢失的用户
author:		totem
create date:	2009-09-15
*/
  SET   ANSI_WARNINGS   ON   
  SET   ANSI_NULLS         ON  
begin
declare @tbName varchar(32), @sql varchar(8000),@sqltem varchar(1024)
declare @i int,@iMax int 
set @sql = ''	set @sqltem= ''
set @i = 52	set @iMax=58
while @i <= @iMax
begin
	set @tbName = 'userfund'
	set @sqltem = 'select userid,username,rtime,rip,userdatabase,tradeflag from emtradeplay.dbo.userlist 
		      where userdatabase = '''+char(65+@i-52)+'''
		      and userid not in 
		      (
		      select userid from 
	              opendataSource(''SQLOLEDB'', ''server=192.168.1.'+convert(varchar(2),@i)+';uid=emtradeplay;pwd=eastmoney201'').stocktrading2.dbo.'+@tbName+''+char(13)+'
		      )'
	if @i < @iMax 
	  set @sql = @sql + @sqltem + ' union all '+char(13)
	else
	  set @sql = @sql + @sqltem +char(13)
	set @i = @i + 1
end 
set @sql =@sql +' order by rtime desc '
exec(@sql)
end
  SET   ANSI_WARNINGS   OFF   
  SET   ANSI_NULLS         OFF

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE dbo.ShowSimNoTradeUserFund
@strWhere varchar(128)
AS
/*
description:	显示用户资金
author:		totem
create date:	2009-09-17
*/
  SET   ANSI_WARNINGS   ON   
  SET   ANSI_NULLS         ON  

declare @sql varchar(8000) ,@sqltem varchar(4000)
declare @i int,@iMax int, @IsExists int
set @sql = ''   set @sqltem = ''
set @i = 52	set @iMax=58
while @i <= @iMax
begin
	set @sqltem = ' select * from 
	opendataSource(''SQLOLEDB'', ''server=192.168.1.'+convert(varchar(2),@i)+';uid=emtradeplay;pwd=eastmoney201'').stocktrading2.dbo.userfund
	 '+ @strWhere

	if @i < @iMax 
	  set @sql = @sql + @sqltem + ' union all '+char(13)
	else
	  set @sql = @sql + @sqltem +char(13)
	set @i = @i + 1
end 
set @sql = 'select a.* from ('+@sql+') a ,emtradeplay.dbo.userlist b 
	      where b.tradeflag =0 and  a.userid=b.userid order by b.userid desc '
exec(@sql)
  SET   ANSI_WARNINGS   OFF   
  SET   ANSI_NULLS         OFF

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE dbo.ShowSimUserFund 
@strWhere varchar(128),
@TradeFlag int
AS
/*
description:	显示用户资金
author:		totem
create date:	2009-09-17
*/
  SET   ANSI_WARNINGS   ON   
  SET   ANSI_NULLS         ON  

declare @sql varchar(8000) ,@sqltem varchar(4000)
declare @i int,@iMax int, @IsExists int
set @sql = ''   set @sqltem = ''
set @i = 52	set @iMax=58
while @i <= @iMax
begin
	set @sqltem = ' select * from 
	opendataSource(''SQLOLEDB'', ''server=192.168.1.'+convert(varchar(2),@i)+';uid=emtradeplay;pwd=eastmoney201'').stocktrading2.dbo.userfund
	 '+ @strWhere

	if @i < @iMax 
	  set @sql = @sql + @sqltem + ' union all '+char(13)
	else
	  set @sql = @sql + @sqltem +char(13)
	set @i = @i + 1
end 
if @TradeFlag <= 1 
set @sql = 'select a.* from ('+@sql+') a ,emtradeplay.dbo.userlist b 
	      where b.tradeflag =0 and  a.userid=b.userid order by b.userid desc '
exec(@sql)
  SET   ANSI_WARNINGS   OFF   
  SET   ANSI_NULLS         OFF
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE dbo.ShowSimUserOrders
@UserId int
AS
/*
description:	显示用户资金
author:		totem
create date:	2009-09-17
*/
  SET   ANSI_WARNINGS   ON   
  SET   ANSI_NULLS         ON  

declare @sql varchar(8000) ,@sqltem varchar(4000)
declare @i int,@iMax int, @strUserId varchar(6) 
set @sql = ''   set @sqltem = ''
set @i = 52	set @iMax=58
while @i <= @iMax
begin
	set @strUserId = convert(int,@UserId)
	set @sqltem = ' select * from 
	opendataSource(''SQLOLEDB'', ''server=192.168.1.'+convert(varchar(2),@i)+';uid=emtradeplay;pwd=eastmoney201'').stocktrading2.dbo.UserOrders
	where userid ='+ @strUserId

	if @i < @iMax 
	  set @sql = @sql + @sqltem + ' union all '+char(13)
	else
	  set @sql = @sql + @sqltem +char(13)
	set @i = @i + 1
end 
exec(@sql)
  SET   ANSI_WARNINGS   OFF   
  SET   ANSI_NULLS         OFF

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE PROCEDURE dbo.ShowSimUserStocks
@sqlWhere varchar(1024)
AS
/*
description:	显示用户持股
author:		totem
create date:	2009-09-17
*/
  SET   ANSI_WARNINGS   ON   
  SET   ANSI_NULLS         ON  

declare @sql varchar(8000) ,@sqltem varchar(4000)
declare @i int,@iMax int, @IsExists int
set @sql = ''   set @sqltem = ''
set @i = 52	set @iMax=58
while @i <= @iMax
begin
	set @sqltem = ' select * from 
	opendataSource(''SQLOLEDB'', ''server=192.168.1.'+convert(varchar(2),@i)+';uid=emtradeplay;pwd=eastmoney201'').stocktrading2.dbo.userstocks
	  '+ @sqlWhere

	if @i < @iMax 
	  set @sql = @sql + @sqltem + ' union all '+char(13)
	else
	  set @sql = @sql + @sqltem +char(13)
	set @i = @i + 1
end 
exec(@sql)
  SET   ANSI_WARNINGS   OFF   
  SET   ANSI_NULLS         OFF
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


CREATE PROCEDURE dbo.dt_displayoaerror
    @iObject int,
    @iresult int
as

set nocount on

declare @vchOutput      varchar(255)
declare @hr             int
declare @vchSource      varchar(255)
declare @vchDescription varchar(255)

    exec @hr = master.dbo.sp_OAGetErrorInfo @iObject, @vchSource OUT, @vchDescription OUT

    select @vchOutput = @vchSource + ': ' + @vchDescription
    raiserror (@vchOutput,16,-1)

    return



GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_vcsenabled

as

set nocount on

declare @iObjectId int
select @iObjectId = 0

declare @VSSGUID varchar(100)
select @VSSGUID = 'SQLVersionControl.VCS_SQL'

    declare @iReturn int
    exec @iReturn = master.dbo.sp_OACreate @VSSGUID, @iObjectId OUT
    if @iReturn <> 0 raiserror('', 16, -1) /* Can't Load Helper DLLC */




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	This procedure returns the version number of the stored
**    procedures used by legacy versions of the Microsoft
**	Visual Database Tools.  Version is 7.0.00.
*/
create procedure dbo.dt_verstamp006
as
	select 7000


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	This procedure returns the version number of the stored
**    procedures used by the the Microsoft Visual Database Tools.
**	Version is 7.0.05.
*/
create procedure dbo.dt_verstamp007
as
	select 7005


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


/*
description:清理无用历史数据
清除除当天,昨天,上月末,上周末的其它日期排名数据
*/
CREATE procedure CompHistoryRank as 
delete from UserRankHistory 
where (
  rankdate <> convert(char(10),getdate(),121)
  and rankdate <> dbo.GetLastWorkDayOfCurrentDay(getdate())
  and rankdate <> dbo.GetLastWorkDayOfLastMonth(getdate())
  and rankdate <> dbo.GetLastWorkDayOfLastWeek(getdate())
)
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


/*
description:获取历上日排名(从DailyRank表中将新增用户信息附加到上日排名)
*/
CREATE procedure GetLastDayRank as
	select userid,areaid,rankid,arearankid,rankdate 
	from userrankhistory
	where rankdate = dbo.getlastworkdayofcurrentday(getdate())
	union 
	select userid,areaid,0 as rankid,0 as arearankid,rankdate from dailyrank a where a.userid not in
	(
	select userid
	from userrankhistory
	where rankdate = dbo.getlastworkdayofcurrentday(getdate())
	)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


/*
description:获取上一工作日资产信息
*/
CREATE procedure GetLastDayWealth 
as
	select userid,wealth,wealthrmb,wealthusd,wealthhkd from [userrankhistory] 
	where rankdate = dbo.getlastworkdayofcurrentday(getdate())
	order by userid

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


/*
description:获取上一工作日资产信息(UserWealth Webservice专用)
*/
CREATE procedure GetLastDayWealthByAreaId
 @areaid int
 as
	select userid,1000000 as wealth,1000000 as wealthrmb,0 as wealthusd,0 as wealthhkd
	 from emtradeplay.dbo.userlist where areaid = @areaid
	 and userid not in
	(
	select userid from emtradeplay.HistoryRank_090923
	)
	union all
	select userid,wealth,wealthrmb,wealthusd,wealthhkd from emtradeplay.HistoryRank_090923
	where areaid = @areaid
	order by userid
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


/*
description:获取上月资产信息
*/
CREATE procedure GetLastMonthWealth as
	  select userid,wealth,wealthrmb,wealthusd,wealthhkd from [userrankhistory] 
	  where rankdate = dbo.getlastworkdayoflastmonth(getdate())
	 order by userid

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


/*
description:获取上周资产信息
*/
CREATE procedure GetLastWeekWealth as
	  select userid,wealth,wealthrmb,wealthusd,wealthhkd from [userrankhistory] 
	  where rankdate = dbo.getlastworkdayoflastweek(getdate())
	  order by userid

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


/*
description:将当天排行数据导入历史排行榜（工作日盘后调用有效）
*/
CREATE procedure ImportHistoryRank as
declare @nHour datetime
set @nHour = Convert(int,datepart(hour,getdate()))
if(@nHour>=15 and  datepart(w,getdate())<>1 and  datepart(w,getdate()) <> 7)
begin
--日排行榜导入历史排行榜
	insert into userrankhistory(
	userid,areaid,rankid,rankchanged,arearankid,arearankchanged,rankdate,
	wealth,wealthrmb,wealthusd,wealthhkd,
	ratiormb,ratiousd,ratiohkd,ratiounderdays)
	(
	select userid,areaid,rankid,rankchanged,arearankid,arearankchanged,rankdate,
	wealth,wealthrmb,wealthusd,wealthhkd,
	ratiormb,ratiousd,ratiohkd,ratiounderdays
	 from dailyrank where not exists (select a.* from dailyrank a,userrankhistory b
	 where a.rankdate = b.rankdate and a.userid=b.userid )
	)
--累加持仓未达标的天数
	update userrankhistory 
	set ratiounderdays = lasttb.ratiounderdays + currtb.ratiounderdays
	from 
	(
	select userid,ratiounderdays from userrankhistory where rankdate =
	(
	select top 1 rankdate from userrankhistory 
	where rankdate < getdate()-1
	group by rankdate
	order by rankdate desc
	) )lasttb,userrankhistory currtb
	where 
	currtb.rankdate = 
	(
	select top 1 rankdate from userrankhistory 
	where rankdate < getdate()
	group by rankdate
	order by rankdate desc
	)  
	and lasttb.userid = currtb.userid
--更新用户表中的持仓标志
	update emtradeplay.dbo.userlist 
	set ratiounderdays = ratiotb.ratiounderdays
	from 
	(select userid,areaid,ratiounderdays from userrankhistory where rankdate =
	(
	select top 1 rankdate from userrankhistory 
	where rankdate < getdate()
	group by rankdate
	order by rankdate desc
	)) ratiotb, emtradeplay.dbo.userlist currtb 
	where ratiotb.userid = currtb.userid 
	and ratiotb.areaid = currtb.areaid 

end
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE PROC dbo.IsQuoteDate
@currDay smalldatetime,
@bQuoteDate bit output
AS  
/*
description :    判断当天是否有行情
return :    @bQuoteDate(0:无行情/1:有行情)
author :    totem
create date :   2009-09-16
*/
declare @thisDay varchar(10),@HolidayName varchar(16)
set @thisDay = convert(varchar(10),@currDay,121)
set @bQuoteDate = 0
if datepart(w,getdate())<>7 and datepart(w,getdate())<>1 --不为周六和周日
begin
  select @HolidayName = holidayName from holidaylist where holidaydate = @thisDay
  if @HolidayName is null
    set @bQuoteDate = 1
  else
    set @bQuoteDate = 0
end
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	Add an object to the dtproperties table
*/
create procedure dbo.dt_adduserobject
as
	set nocount on
	/*
	** Create the user object if it does not exist already
	*/
	begin transaction
		insert dbo.dtproperties (property) VALUES ('DtgSchemaOBJECT')
		update dbo.dtproperties set objectid=@@identity 
			where id=@@identity and property='DtgSchemaOBJECT'
	commit
	return @@identity


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create procedure dbo.dt_adduserobject_vcs
    @vchProperty varchar(64)

as

set nocount on

declare @iReturn int
    /*
    ** Create the user object if it does not exist already
    */
    begin transaction
        select @iReturn = objectid from dbo.dtproperties where property = @vchProperty
        if @iReturn IS NULL
        begin
            insert dbo.dtproperties (property) VALUES (@vchProperty)
            update dbo.dtproperties set objectid=@@identity
                    where id=@@identity and property=@vchProperty
            select @iReturn = @@identity
        end
    commit
    return @iReturn




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


CREATE PROCEDURE dbo.dt_displayoaerror_u
    @iObject int,
    @iresult int
as
	-- This procedure should no longer be called;  dt_displayoaerror should be called instead.
	-- Calls are forwarded to dt_displayoaerror to maintain backward compatibility.
	set nocount on
	exec dbo.dt_displayoaerror
		@iObject,
		@iresult




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	Drop one or all the associated properties of an object or an attribute 
**
**	dt_dropproperties objid, null or '' -- drop all properties of the object itself
**	dt_dropproperties objid, property -- drop the property
*/
create procedure dbo.dt_droppropertiesbyid
	@id int,
	@property varchar(64)
as
	set nocount on

	if (@property is null) or (@property = '')
		delete from dbo.dtproperties where objectid=@id
	else
		delete from dbo.dtproperties 
			where objectid=@id and property=@property



GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	Drop an object from the dbo.dtproperties table
*/
create procedure dbo.dt_dropuserobjectbyid
	@id int
as
	set nocount on
	delete from dbo.dtproperties where objectid=@id


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/* 
**	Generate an ansi name that is unique in the dtproperties.value column 
*/ 
create procedure dbo.dt_generateansiname(@name varchar(255) output) 
as 
	declare @prologue varchar(20) 
	declare @indexstring varchar(20) 
	declare @index integer 
 
	set @prologue = 'MSDT-A-' 
	set @index = 1 
 
	while 1 = 1 
	begin 
		set @indexstring = cast(@index as varchar(20)) 
		set @name = @prologue + @indexstring 
		if not exists (select value from dtproperties where value = @name) 
			break 
		 
		set @index = @index + 1 
 
		if (@index = 10000) 
			goto TooMany 
	end 
 
Leave: 
 
	return 
 
TooMany: 
 
	set @name = 'DIAGRAM' 
	goto Leave 


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	Retrieve the owner object(s) of a given property
*/
create procedure dbo.dt_getobjwithprop
	@property varchar(30),
	@value varchar(255)
as
	set nocount on

	if (@property is null) or (@property = '')
	begin
		raiserror('Must specify a property name.',-1,-1)
		return (1)
	end

	if (@value is null)
		select objectid id from dbo.dtproperties
			where property=@property

	else
		select objectid id from dbo.dtproperties
			where property=@property and value=@value


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	Retrieve the owner object(s) of a given property
*/
create procedure dbo.dt_getobjwithprop_u
	@property varchar(30),
	@uvalue nvarchar(255)
as
	set nocount on

	if (@property is null) or (@property = '')
	begin
		raiserror('Must specify a property name.',-1,-1)
		return (1)
	end

	if (@uvalue is null)
		select objectid id from dbo.dtproperties
			where property=@property

	else
		select objectid id from dbo.dtproperties
			where property=@property and uvalue=@uvalue


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	Retrieve properties by id's
**
**	dt_getproperties objid, null or '' -- retrieve all properties of the object itself
**	dt_getproperties objid, property -- retrieve the property specified
*/
create procedure dbo.dt_getpropertiesbyid
	@id int,
	@property varchar(64)
as
	set nocount on

	if (@property is null) or (@property = '')
		select property, version, value, lvalue
			from dbo.dtproperties
			where  @id=objectid
	else
		select property, version, value, lvalue
			from dbo.dtproperties
			where  @id=objectid and @property=property


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	Retrieve properties by id's
**
**	dt_getproperties objid, null or '' -- retrieve all properties of the object itself
**	dt_getproperties objid, property -- retrieve the property specified
*/
create procedure dbo.dt_getpropertiesbyid_u
	@id int,
	@property varchar(64)
as
	set nocount on

	if (@property is null) or (@property = '')
		select property, version, uvalue, lvalue
			from dbo.dtproperties
			where  @id=objectid
	else
		select property, version, uvalue, lvalue
			from dbo.dtproperties
			where  @id=objectid and @property=property


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create procedure dbo.dt_getpropertiesbyid_vcs
    @id       int,
    @property varchar(64),
    @value    varchar(255) = NULL OUT

as

    set nocount on

    select @value = (
        select value
                from dbo.dtproperties
                where @id=objectid and @property=property
                )



GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	If the property already exists, reset the value; otherwise add property
**		id -- the id in sysobjects of the object
**		property -- the name of the property
**		value -- the text value of the property
**		lvalue -- the binary value of the property (image)
*/
create procedure dbo.dt_setpropertybyid
	@id int,
	@property varchar(64),
	@value varchar(255),
	@lvalue image
as
	set nocount on
	declare @uvalue nvarchar(255) 
	set @uvalue = convert(nvarchar(255), @value) 
	if exists (select * from dbo.dtproperties 
			where objectid=@id and property=@property)
	begin
		--
		-- bump the version count for this row as we update it
		--
		update dbo.dtproperties set value=@value, uvalue=@uvalue, lvalue=@lvalue, version=version+1
			where objectid=@id and property=@property
	end
	else
	begin
		--
		-- version count is auto-set to 0 on initial insert
		--
		insert dbo.dtproperties (property, objectid, value, uvalue, lvalue)
			values (@property, @id, @value, @uvalue, @lvalue)
	end



GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_addtosourcecontrol
    @vchSourceSafeINI varchar(255) = '',
    @vchProjectName   varchar(255) ='',
    @vchComment       varchar(255) ='',
    @vchLoginName     varchar(255) ='',
    @vchPassword      varchar(255) =''

as

set nocount on

declare @iReturn int
declare @iObjectId int
select @iObjectId = 0

declare @iStreamObjectId int
select @iStreamObjectId = 0

declare @VSSGUID varchar(100)
select @VSSGUID = 'SQLVersionControl.VCS_SQL'

declare @vchDatabaseName varchar(255)
select @vchDatabaseName = db_name()

declare @iReturnValue int
select @iReturnValue = 0

declare @iPropertyObjectId int
declare @vchParentId varchar(255)

declare @iObjectCount int
select @iObjectCount = 0

    exec @iReturn = master.dbo.sp_OACreate @VSSGUID, @iObjectId OUT
    if @iReturn <> 0 GOTO E_OAError


    /* Create Project in SS */
    exec @iReturn = master.dbo.sp_OAMethod @iObjectId,
											'AddProjectToSourceSafe',
											NULL,
											@vchSourceSafeINI,
											@vchProjectName output,
											@@SERVERNAME,
											@vchDatabaseName,
											@vchLoginName,
											@vchPassword,
											@vchComment


    if @iReturn <> 0 GOTO E_OAError

    /* Set Database Properties */

    begin tran SetProperties

    /* add high level object */

    exec @iPropertyObjectId = dbo.dt_adduserobject_vcs 'VCSProjectID'

    select @vchParentId = CONVERT(varchar(255),@iPropertyObjectId)

    exec dbo.dt_setpropertybyid @iPropertyObjectId, 'VCSProjectID', @vchParentId , NULL
    exec dbo.dt_setpropertybyid @iPropertyObjectId, 'VCSProject' , @vchProjectName , NULL
    exec dbo.dt_setpropertybyid @iPropertyObjectId, 'VCSSourceSafeINI' , @vchSourceSafeINI , NULL
    exec dbo.dt_setpropertybyid @iPropertyObjectId, 'VCSSQLServer', @@SERVERNAME, NULL
    exec dbo.dt_setpropertybyid @iPropertyObjectId, 'VCSSQLDatabase', @vchDatabaseName, NULL

    if @@error <> 0 GOTO E_General_Error

    commit tran SetProperties
    
    select @iObjectCount = 0;

CleanUp:
    select @vchProjectName
    select @iObjectCount
    return

E_General_Error:
    /* this is an all or nothing.  No specific error messages */
    goto CleanUp

E_OAError:
    exec dbo.dt_displayoaerror @iObjectId, @iReturn
    goto CleanUp




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_checkinobject
    @chObjectType  char(4),
    @vchObjectName varchar(255),
    @vchComment    varchar(255)='',
    @vchLoginName  varchar(255),
    @vchPassword   varchar(255)='',
    @iVCSFlags     int = 0,
    @iActionFlag   int = 0,   /* 0 => AddFile, 1 => CheckIn */
    @txStream1     Text = '', /* drop stream   */ /* There is a bug that if items are NULL they do not pass to OLE servers */
    @txStream2     Text = '', /* create stream */
    @txStream3     Text = ''  /* grant stream  */


as

	set nocount on

	declare @iReturn int
	declare @iObjectId int
	select @iObjectId = 0
	declare @iStreamObjectId int

	declare @VSSGUID varchar(100)
	select @VSSGUID = 'SQLVersionControl.VCS_SQL'

	declare @iPropertyObjectId int
	select @iPropertyObjectId  = 0

    select @iPropertyObjectId = (select objectid from dbo.dtproperties where property = 'VCSProjectID')

    declare @vchProjectName   varchar(255)
    declare @vchSourceSafeINI varchar(255)
    declare @vchServerName    varchar(255)
    declare @vchDatabaseName  varchar(255)
    declare @iReturnValue	  int
    declare @pos			  int
    declare @vchProcLinePiece varchar(255)

    
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSProject',       @vchProjectName   OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSourceSafeINI', @vchSourceSafeINI OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSQLServer',     @vchServerName    OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSQLDatabase',   @vchDatabaseName  OUT

    if @chObjectType = 'PROC'
    begin
        if @iActionFlag = 1
        begin
            /* Procedure Can have up to three streams
            Drop Stream, Create Stream, GRANT stream */

            begin tran compile_all

            /* try to compile the streams */
            exec (@txStream1)
            if @@error <> 0 GOTO E_Compile_Fail

            exec (@txStream2)
            if @@error <> 0 GOTO E_Compile_Fail

            exec (@txStream3)
            if @@error <> 0 GOTO E_Compile_Fail
        end

        exec @iReturn = master.dbo.sp_OACreate @VSSGUID, @iObjectId OUT
        if @iReturn <> 0 GOTO E_OAError

        exec @iReturn = master.dbo.sp_OAGetProperty @iObjectId, 'GetStreamObject', @iStreamObjectId OUT
        if @iReturn <> 0 GOTO E_OAError
        
        if @iActionFlag = 1
        begin
            
            declare @iStreamLength int
			
			select @pos=1
			select @iStreamLength = datalength(@txStream2)
			
			if @iStreamLength > 0
			begin
			
				while @pos < @iStreamLength
				begin
						
					select @vchProcLinePiece = substring(@txStream2, @pos, 255)
					
					exec @iReturn = master.dbo.sp_OAMethod @iStreamObjectId, 'AddStream', @iReturnValue OUT, @vchProcLinePiece
            		if @iReturn <> 0 GOTO E_OAError
            		
					select @pos = @pos + 255
					
				end
            
				exec @iReturn = master.dbo.sp_OAMethod @iObjectId,
														'CheckIn_StoredProcedure',
														NULL,
														@sProjectName = @vchProjectName,
														@sSourceSafeINI = @vchSourceSafeINI,
														@sServerName = @vchServerName,
														@sDatabaseName = @vchDatabaseName,
														@sObjectName = @vchObjectName,
														@sComment = @vchComment,
														@sLoginName = @vchLoginName,
														@sPassword = @vchPassword,
														@iVCSFlags = @iVCSFlags,
														@iActionFlag = @iActionFlag,
														@sStream = ''
                                        
			end
        end
        else
        begin
        
            select colid, text into #ProcLines
            from syscomments
            where id = object_id(@vchObjectName)
            order by colid

            declare @iCurProcLine int
            declare @iProcLines int
            select @iCurProcLine = 1
            select @iProcLines = (select count(*) from #ProcLines)
            while @iCurProcLine <= @iProcLines
            begin
                select @pos = 1
                declare @iCurLineSize int
                select @iCurLineSize = len((select text from #ProcLines where colid = @iCurProcLine))
                while @pos <= @iCurLineSize
                begin                
                    select @vchProcLinePiece = convert(varchar(255),
                        substring((select text from #ProcLines where colid = @iCurProcLine),
                                  @pos, 255 ))
                    exec @iReturn = master.dbo.sp_OAMethod @iStreamObjectId, 'AddStream', @iReturnValue OUT, @vchProcLinePiece
                    if @iReturn <> 0 GOTO E_OAError
                    select @pos = @pos + 255                  
                end
                select @iCurProcLine = @iCurProcLine + 1
            end
            drop table #ProcLines

            exec @iReturn = master.dbo.sp_OAMethod @iObjectId,
													'CheckIn_StoredProcedure',
													NULL,
													@sProjectName = @vchProjectName,
													@sSourceSafeINI = @vchSourceSafeINI,
													@sServerName = @vchServerName,
													@sDatabaseName = @vchDatabaseName,
													@sObjectName = @vchObjectName,
													@sComment = @vchComment,
													@sLoginName = @vchLoginName,
													@sPassword = @vchPassword,
													@iVCSFlags = @iVCSFlags,
													@iActionFlag = @iActionFlag,
													@sStream = ''
        end

        if @iReturn <> 0 GOTO E_OAError

        if @iActionFlag = 1
        begin
            commit tran compile_all
            if @@error <> 0 GOTO E_Compile_Fail
        end

    end

CleanUp:
	return

E_Compile_Fail:
	declare @lerror int
	select @lerror = @@error
	rollback tran compile_all
	RAISERROR (@lerror,16,-1)
	goto CleanUp

E_OAError:
	if @iActionFlag = 1 rollback tran compile_all
	exec dbo.dt_displayoaerror @iObjectId, @iReturn
	goto CleanUp




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_checkoutobject
    @chObjectType  char(4),
    @vchObjectName varchar(255),
    @vchComment    varchar(255),
    @vchLoginName  varchar(255),
    @vchPassword   varchar(255),
    @iVCSFlags     int = 0,
    @iActionFlag   int = 0/* 0 => Checkout, 1 => GetLatest, 2 => UndoCheckOut */

as

	set nocount on

	declare @iReturn int
	declare @iObjectId int
	select @iObjectId =0

	declare @VSSGUID varchar(100)
	select @VSSGUID = 'SQLVersionControl.VCS_SQL'

	declare @iReturnValue int
	select @iReturnValue = 0

	declare @vchTempText varchar(255)

	/* this is for our strings */
	declare @iStreamObjectId int
	select @iStreamObjectId = 0

    declare @iPropertyObjectId int
    select @iPropertyObjectId = (select objectid from dbo.dtproperties where property = 'VCSProjectID')

    declare @vchProjectName   varchar(255)
    declare @vchSourceSafeINI varchar(255)
    declare @vchServerName    varchar(255)
    declare @vchDatabaseName  varchar(255)
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSProject',       @vchProjectName   OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSourceSafeINI', @vchSourceSafeINI OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSQLServer',     @vchServerName    OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSQLDatabase',   @vchDatabaseName  OUT

    if @chObjectType = 'PROC'
    begin
        /* Procedure Can have up to three streams
           Drop Stream, Create Stream, GRANT stream */

        exec @iReturn = master.dbo.sp_OACreate @VSSGUID, @iObjectId OUT

        if @iReturn <> 0 GOTO E_OAError

        exec @iReturn = master.dbo.sp_OAMethod @iObjectId,
												'CheckOut_StoredProcedure',
												NULL,
												@sProjectName = @vchProjectName,
												@sSourceSafeINI = @vchSourceSafeINI,
												@sObjectName = @vchObjectName,
												@sServerName = @vchServerName,
												@sDatabaseName = @vchDatabaseName,
												@sComment = @vchComment,
												@sLoginName = @vchLoginName,
												@sPassword = @vchPassword,
												@iVCSFlags = @iVCSFlags,
												@iActionFlag = @iActionFlag

        if @iReturn <> 0 GOTO E_OAError


        exec @iReturn = master.dbo.sp_OAGetProperty @iObjectId, 'GetStreamObject', @iStreamObjectId OUT

        if @iReturn <> 0 GOTO E_OAError

        create table #commenttext (id int identity, sourcecode varchar(255))


        select @vchTempText = 'STUB'
        while @vchTempText is not null
        begin
            exec @iReturn = master.dbo.sp_OAMethod @iStreamObjectId, 'GetStream', @iReturnValue OUT, @vchTempText OUT
            if @iReturn <> 0 GOTO E_OAError
            
            if (@vchTempText = '') set @vchTempText = null
            if (@vchTempText is not null) insert into #commenttext (sourcecode) select @vchTempText
        end

        select 'VCS'=sourcecode from #commenttext order by id
        select 'SQL'=text from syscomments where id = object_id(@vchObjectName) order by colid

    end

CleanUp:
    return

E_OAError:
    exec dbo.dt_displayoaerror @iObjectId, @iReturn
    GOTO CleanUp




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create procedure dbo.dt_getpropertiesbyid_vcs_u
    @id       int,
    @property varchar(64),
    @value    nvarchar(255) = NULL OUT

as

    -- This procedure should no longer be called;  dt_getpropertiesbyid_vcsshould be called instead.
	-- Calls are forwarded to dt_getpropertiesbyid_vcs to maintain backward compatibility.
	set nocount on
    exec dbo.dt_getpropertiesbyid_vcs
		@id,
		@property,
		@value output



GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_isundersourcecontrol
    @vchLoginName varchar(255) = '',
    @vchPassword  varchar(255) = '',
    @iWhoToo      int = 0 /* 0 => Just check project; 1 => get list of objs */

as

	set nocount on

	declare @iReturn int
	declare @iObjectId int
	select @iObjectId = 0

	declare @VSSGUID varchar(100)
	select @VSSGUID = 'SQLVersionControl.VCS_SQL'

	declare @iReturnValue int
	select @iReturnValue = 0

	declare @iStreamObjectId int
	select @iStreamObjectId   = 0

	declare @vchTempText varchar(255)

    declare @iPropertyObjectId int
    select @iPropertyObjectId = (select objectid from dbo.dtproperties where property = 'VCSProjectID')

    declare @vchProjectName   varchar(255)
    declare @vchSourceSafeINI varchar(255)
    declare @vchServerName    varchar(255)
    declare @vchDatabaseName  varchar(255)
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSProject',       @vchProjectName   OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSourceSafeINI', @vchSourceSafeINI OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSQLServer',     @vchServerName    OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSQLDatabase',   @vchDatabaseName  OUT

    if (@vchProjectName = '')	set @vchProjectName		= null
    if (@vchSourceSafeINI = '') set @vchSourceSafeINI	= null
    if (@vchServerName = '')	set @vchServerName		= null
    if (@vchDatabaseName = '')	set @vchDatabaseName	= null
    
    if (@vchProjectName is null) or (@vchSourceSafeINI is null) or (@vchServerName is null) or (@vchDatabaseName is null)
    begin
        RAISERROR('Not Under Source Control',16,-1)
        return
    end

    if @iWhoToo = 1
    begin

        /* Get List of Procs in the project */
        exec @iReturn = master.dbo.sp_OACreate @VSSGUID, @iObjectId OUT
        if @iReturn <> 0 GOTO E_OAError

        exec @iReturn = master.dbo.sp_OAMethod @iObjectId,
												'GetListOfObjects',
												NULL,
												@vchProjectName,
												@vchSourceSafeINI,
												@vchServerName,
												@vchDatabaseName,
												@vchLoginName,
												@vchPassword

        if @iReturn <> 0 GOTO E_OAError

        exec @iReturn = master.dbo.sp_OAGetProperty @iObjectId, 'GetStreamObject', @iStreamObjectId OUT

        if @iReturn <> 0 GOTO E_OAError

        create table #ObjectList (id int identity, vchObjectlist varchar(255))

        select @vchTempText = 'STUB'
        while @vchTempText is not null
        begin
            exec @iReturn = master.dbo.sp_OAMethod @iStreamObjectId, 'GetStream', @iReturnValue OUT, @vchTempText OUT
            if @iReturn <> 0 GOTO E_OAError
            
            if (@vchTempText = '') set @vchTempText = null
            if (@vchTempText is not null) insert into #ObjectList (vchObjectlist ) select @vchTempText
        end

        select vchObjectlist from #ObjectList order by id
    end

CleanUp:
    return

E_OAError:
    exec dbo.dt_displayoaerror @iObjectId, @iReturn
    goto CleanUp




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create procedure dbo.dt_removefromsourcecontrol

as

    set nocount on

    declare @iPropertyObjectId int
    select @iPropertyObjectId = (select objectid from dbo.dtproperties where property = 'VCSProjectID')

    exec dbo.dt_droppropertiesbyid @iPropertyObjectId, null

    /* -1 is returned by dt_droppopertiesbyid */
    if @@error <> 0 and @@error <> -1 return 1

    return 0




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


/*
**	If the property already exists, reset the value; otherwise add property
**		id -- the id in sysobjects of the object
**		property -- the name of the property
**		uvalue -- the text value of the property
**		lvalue -- the binary value of the property (image)
*/
create procedure dbo.dt_setpropertybyid_u
	@id int,
	@property varchar(64),
	@uvalue nvarchar(255),
	@lvalue image
as
	set nocount on
	-- 
	-- If we are writing the name property, find the ansi equivalent. 
	-- If there is no lossless translation, generate an ansi name. 
	-- 
	declare @avalue varchar(255) 
	set @avalue = null 
	if (@uvalue is not null) 
	begin 
		if (convert(nvarchar(255), convert(varchar(255), @uvalue)) = @uvalue) 
		begin 
			set @avalue = convert(varchar(255), @uvalue) 
		end 
		else 
		begin 
			if 'DtgSchemaNAME' = @property 
			begin 
				exec dbo.dt_generateansiname @avalue output 
			end 
		end 
	end 
	if exists (select * from dbo.dtproperties 
			where objectid=@id and property=@property)
	begin
		--
		-- bump the version count for this row as we update it
		--
		update dbo.dtproperties set value=@avalue, uvalue=@uvalue, lvalue=@lvalue, version=version+1
			where objectid=@id and property=@property
	end
	else
	begin
		--
		-- version count is auto-set to 0 on initial insert
		--
		insert dbo.dtproperties (property, objectid, value, uvalue, lvalue)
			values (@property, @id, @avalue, @uvalue, @lvalue)
	end


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_validateloginparams
    @vchLoginName  varchar(255),
    @vchPassword   varchar(255)
as

set nocount on

declare @iReturn int
declare @iObjectId int
select @iObjectId =0

declare @VSSGUID varchar(100)
select @VSSGUID = 'SQLVersionControl.VCS_SQL'

    declare @iPropertyObjectId int
    select @iPropertyObjectId = (select objectid from dbo.dtproperties where property = 'VCSProjectID')

    declare @vchSourceSafeINI varchar(255)
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSourceSafeINI', @vchSourceSafeINI OUT

    exec @iReturn = master.dbo.sp_OACreate @VSSGUID, @iObjectId OUT
    if @iReturn <> 0 GOTO E_OAError

    exec @iReturn = master.dbo.sp_OAMethod @iObjectId,
											'ValidateLoginParams',
											NULL,
											@sSourceSafeINI = @vchSourceSafeINI,
											@sLoginName = @vchLoginName,
											@sPassword = @vchPassword
    if @iReturn <> 0 GOTO E_OAError

CleanUp:
    return

E_OAError:
    exec dbo.dt_displayoaerror @iObjectId, @iReturn
    GOTO CleanUp




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_whocheckedout
        @chObjectType  char(4),
        @vchObjectName varchar(255),
        @vchLoginName  varchar(255),
        @vchPassword   varchar(255)

as

set nocount on

declare @iReturn int
declare @iObjectId int
select @iObjectId =0

declare @VSSGUID varchar(100)
select @VSSGUID = 'SQLVersionControl.VCS_SQL'

    declare @iPropertyObjectId int

    select @iPropertyObjectId = (select objectid from dbo.dtproperties where property = 'VCSProjectID')

    declare @vchProjectName   varchar(255)
    declare @vchSourceSafeINI varchar(255)
    declare @vchServerName    varchar(255)
    declare @vchDatabaseName  varchar(255)
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSProject',       @vchProjectName   OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSourceSafeINI', @vchSourceSafeINI OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSQLServer',     @vchServerName    OUT
    exec dbo.dt_getpropertiesbyid_vcs @iPropertyObjectId, 'VCSSQLDatabase',   @vchDatabaseName  OUT

    if @chObjectType = 'PROC'
    begin
        exec @iReturn = master.dbo.sp_OACreate @VSSGUID, @iObjectId OUT

        if @iReturn <> 0 GOTO E_OAError

        declare @vchReturnValue varchar(255)
        select @vchReturnValue = ''

        exec @iReturn = master.dbo.sp_OAMethod @iObjectId,
												'WhoCheckedOut',
												@vchReturnValue OUT,
												@sProjectName = @vchProjectName,
												@sSourceSafeINI = @vchSourceSafeINI,
												@sObjectName = @vchObjectName,
												@sServerName = @vchServerName,
												@sDatabaseName = @vchDatabaseName,
												@sLoginName = @vchLoginName,
												@sPassword = @vchPassword

        if @iReturn <> 0 GOTO E_OAError

        select @vchReturnValue

    end

CleanUp:
    return

E_OAError:
    exec dbo.dt_displayoaerror @iObjectId, @iReturn
    GOTO CleanUp




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_addtosourcecontrol_u
    @vchSourceSafeINI nvarchar(255) = '',
    @vchProjectName   nvarchar(255) ='',
    @vchComment       nvarchar(255) ='',
    @vchLoginName     nvarchar(255) ='',
    @vchPassword      nvarchar(255) =''

as
	-- This procedure should no longer be called;  dt_addtosourcecontrol should be called instead.
	-- Calls are forwarded to dt_addtosourcecontrol to maintain backward compatibility
	set nocount on
	exec dbo.dt_addtosourcecontrol 
		@vchSourceSafeINI, 
		@vchProjectName, 
		@vchComment, 
		@vchLoginName, 
		@vchPassword




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_checkinobject_u
    @chObjectType  char(4),
    @vchObjectName nvarchar(255),
    @vchComment    nvarchar(255)='',
    @vchLoginName  nvarchar(255),
    @vchPassword   nvarchar(255)='',
    @iVCSFlags     int = 0,
    @iActionFlag   int = 0,   /* 0 => AddFile, 1 => CheckIn */
    @txStream1     text = '',  /* drop stream   */ /* There is a bug that if items are NULL they do not pass to OLE servers */
    @txStream2     text = '',  /* create stream */
    @txStream3     text = ''   /* grant stream  */

as	
	-- This procedure should no longer be called;  dt_checkinobject should be called instead.
	-- Calls are forwarded to dt_checkinobject to maintain backward compatibility.
	set nocount on
	exec dbo.dt_checkinobject
		@chObjectType,
		@vchObjectName,
		@vchComment,
		@vchLoginName,
		@vchPassword,
		@iVCSFlags,
		@iActionFlag,   
		@txStream1,		
		@txStream2,		
		@txStream3		




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_checkoutobject_u
    @chObjectType  char(4),
    @vchObjectName nvarchar(255),
    @vchComment    nvarchar(255),
    @vchLoginName  nvarchar(255),
    @vchPassword   nvarchar(255),
    @iVCSFlags     int = 0,
    @iActionFlag   int = 0/* 0 => Checkout, 1 => GetLatest, 2 => UndoCheckOut */

as

	-- This procedure should no longer be called;  dt_checkoutobject should be called instead.
	-- Calls are forwarded to dt_checkoutobject to maintain backward compatibility.
	set nocount on
	exec dbo.dt_checkoutobject
		@chObjectType,  
		@vchObjectName, 
		@vchComment,    
		@vchLoginName,  
		@vchPassword,  
		@iVCSFlags,    
		@iActionFlag 




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_isundersourcecontrol_u
    @vchLoginName nvarchar(255) = '',
    @vchPassword  nvarchar(255) = '',
    @iWhoToo      int = 0 /* 0 => Just check project; 1 => get list of objs */

as
	-- This procedure should no longer be called;  dt_isundersourcecontrol should be called instead.
	-- Calls are forwarded to dt_isundersourcecontrol to maintain backward compatibility.
	set nocount on
	exec dbo.dt_isundersourcecontrol
		@vchLoginName,
		@vchPassword,
		@iWhoToo 




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_validateloginparams_u
    @vchLoginName  nvarchar(255),
    @vchPassword   nvarchar(255)
as

	-- This procedure should no longer be called;  dt_validateloginparams should be called instead.
	-- Calls are forwarded to dt_validateloginparams to maintain backward compatibility.
	set nocount on
	exec dbo.dt_validateloginparams
		@vchLoginName,
		@vchPassword 




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


create proc dbo.dt_whocheckedout_u
        @chObjectType  char(4),
        @vchObjectName nvarchar(255),
        @vchLoginName  nvarchar(255),
        @vchPassword   nvarchar(255)

as

	-- This procedure should no longer be called;  dt_whocheckedout should be called instead.
	-- Calls are forwarded to dt_whocheckedout to maintain backward compatibility.
	set nocount on
	exec dbo.dt_whocheckedout
		@chObjectType, 
		@vchObjectName,
		@vchLoginName, 
		@vchPassword  




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


exec sp_addextendedproperty N'MS_Description', N'低于持仓标准的天数', N'user', N'dbo', N'table', N'DailyRank', N'column', N'RatioUnderDays'


GO


exec sp_addextendedproperty N'MS_Description', '历史排行', N'user', N'dbo', N'table', N'UserRankHistory'

GO

exec sp_addextendedproperty N'MS_Description', '用户ID', N'user', N'dbo', N'table', N'UserRankHistory', N'column', N'UserID'
GO
exec sp_addextendedproperty N'MS_Description', '区域ID', N'user', N'dbo', N'table', N'UserRankHistory', N'column', N'AreaId'
GO
exec sp_addextendedproperty N'MS_Description', '排行ID', N'user', N'dbo', N'table', N'UserRankHistory', N'column', N'RankID'
GO
exec sp_addextendedproperty N'MS_Description', '排行变化数', N'user', N'dbo', N'table', N'UserRankHistory', N'column', N'RankChanged'
GO
exec sp_addextendedproperty N'MS_Description', '排行时间', N'user', N'dbo', N'table', N'UserRankHistory', N'column', N'RankDate'
GO
exec sp_addextendedproperty N'MS_Description', N'低于持仓标准的天数', N'user', N'dbo', N'table', N'UserRankHistory', N'column', N'RatioUnderDays'


GO

 