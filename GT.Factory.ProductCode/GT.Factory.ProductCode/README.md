# GT.Factory.ProductCode V2

本版本基于原有 ProductCode WinForms UI 和现有 SQL 表重新整理。

## 核心变化

产品码不再由 `sp_GetProductCode` 生成。

新的生成链路：

```text
CodeRule
  -> ProductCodeGenerator
  -> ProcessorRegistry
  -> Segment Processors
  -> ProductCode
```

SQL Server 只负责数据持久化、流水号并发安全、班次读取和历史记录。

## 复用的现有 SQL 表

- `GTCodeRule`
- `GTCodeRuleDetail`
- `GTShiftConfig`
- `GTCodeSerialNumber`
- `GTProductCodeHistory`

不增加 ProductCode 专用表。

## Segment

当前支持：

- FixedText
- Date
- Shift
- ProductModel
- SerialNumber
- Variable

新增 Segment 时实现 `ICodeSegmentProcessor` 并注册到 `ProcessorRegistry`，无需修改 `ProductCodeGenerator`。

## Date

Date 使用新的组合配置：

```json
{
  "Parts": [
    {
      "Part": "Year",
      "Mode": "Mapping",
      "Mapping": {
        "Values": {
          "2026": "A",
          "2027": "B"
        }
      }
    },
    {
      "Part": "Month",
      "Mode": "Mapping",
      "Mapping": {
        "Values": {
          "1": "1",
          "10": "A",
          "11": "B",
          "12": "C"
        }
      }
    },
    {
      "Part": "DayOfYear",
      "Mode": "ThreeDigit"
    }
  ]
}
```

## Serial

```json
{
  "Length": 6,
  "PaddingChar": "0",
  "ResetMode": "None",
  "Direction": "Left"
}
```

## SQL

`sql/01_RemoveLegacyProductCodeProcedure.sql` 用于正式切换时删除旧 `sp_GetProductCode`。

该脚本不会删除 ProductCode 业务表。
