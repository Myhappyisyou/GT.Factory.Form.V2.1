USE [GT_Template];
GO

/*
    ProductCode V2 已经不再通过 sp_GetProductCode 生成产品码。

    现有业务表继续复用：
      GTCodeRule
      GTCodeRuleDetail
      GTShiftConfig
      GTCodeSerialNumber
      GTProductCodeHistory

    本脚本只负责移除旧的“万能产品码生成存储过程”。
    不删除、不修改上述业务表。
*/

IF OBJECT_ID(N'dbo.sp_GetProductCode', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_GetProductCode;
END
GO
