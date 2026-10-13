/*
    ProductCode V2 新配置示例。

    本文件只展示新的 ConfigJson 结构，不自动修改生产规则。
    可以先 SELECT 查看，再按实际 RuleId/SegmentId 生成 UPDATE。
*/

SELECT
    1 AS SegmentType,
    N'{"Text":"GT"}' AS ConfigJson
UNION ALL
SELECT
    2,
    N'{"Parts":[{"Part":"Year","Mode":"LastTwo"},{"Part":"Month","Mode":"Mapping","Mapping":{"Values":{"1":"1","2":"2","3":"3","4":"4","5":"5","6":"6","7":"7","8":"8","9":"9","10":"A","11":"B","12":"C"}}},{"Part":"DayOfYear","Mode":"ThreeDigit"}]}'
UNION ALL
SELECT
    4,
    N'{}'
UNION ALL
SELECT
    7,
    N'{"Length":6,"PaddingChar":"0","ResetMode":"None","Direction":"Left"}';
