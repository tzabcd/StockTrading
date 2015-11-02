
IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CreateHistoryRankByDate')
	BEGIN
		DROP  Procedure  CreateHistoryRankByDate
	END

GO

CREATE PROCEDURE [dbo].[CreateHistoryRankByDate]
/*
  **Description : Create table of current historyrank
  **para:
  **return:
  **author£ºtotem
  **create date£º2009-09-12
  **update memo:
attention£º
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