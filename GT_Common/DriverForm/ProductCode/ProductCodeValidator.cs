using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.ProductCode
{
    public static class ProductCodeValidator
    {
        public static List<string> Validate(List<ProductCodeRule> rules)
        {
            var errors = new List<string>();

            if (rules == null || rules.Count == 0)
            {
                errors.Add("没有任何规则");
                return errors;
            }

            // 1. 必填
            foreach (var r in rules)
            {
                if (string.IsNullOrWhiteSpace(r.Model))
                    errors.Add("存在空型号");

                if (string.IsNullOrWhiteSpace(r.PartName))
                    errors.Add($"[{r.Model}] 部件名称不能为空");

                if (r.Length <= 0)
                    errors.Add($"[{r.Model}-{r.PartName}] 长度必须大于0");
            }

            // 2. 唯一性（Model + CodeType + PartName）
            var dup1 = rules
                .GroupBy(x => new { x.Model, x.CodeType, x.PartName })
                .Where(g => g.Count() > 1);

            foreach (var g in dup1)
            {
                errors.Add($"重复规则：{g.Key.Model}-{g.Key.CodeType}-{g.Key.PartName}");
            }

            // 3. CodeMark 唯一
            var dup2 = rules
                .GroupBy(x => new { x.Model, x.CodeMark })
                .Where(g => g.Count() > 1);

            foreach (var g in dup2)
            {
                errors.Add($"CodeMark重复：{g.Key.Model}-{g.Key.CodeMark}");
            }

            // 4. 每个型号至少一个启用
            var modelGroups = rules.GroupBy(x => x.Model);
            foreach (var g in modelGroups)
            {
                if (!g.Any(x => x.Enable))
                    errors.Add($"型号[{g.Key}]没有启用规则");
            }

            return errors;
        }
    }
}
