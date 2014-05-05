USE [NetMeter]
GO

/****** Object:  StoredProcedure [dbo].[CreateAnalyticsDataMartDB]    Script Date: 4/29/2014 12:11:16 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateAnalyticsDataMartDB] @v_dt_now datetime, @v_id_run int
AS
BEGIN

DECLARE @l_count int;

if (@v_id_run is not null)
begin
       INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'Starting Subscription DataMart');
end;

if not exists (select 1 from master..sysdatabases where name='AnalyticsDataMart')
begin
       if (@v_id_run is not null)
       begin
              INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Info', 'Creating database for AnalyticsDataMart');
       end;
       create database AnalyticsDataMart;
       INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Info', 'Finished Creating empty database for AnalyticsDataMart');
end;


if (@v_id_run is not null)
begin
       INSERT INTO [dbo].[t_recevent_run_details] ([id_run], [dt_crt], [tx_type], [tx_detail]) VALUES (@v_id_run, GETUTCDATE(), 'Debug', 'Finished Proc DataMart');
end;

end;
GO


