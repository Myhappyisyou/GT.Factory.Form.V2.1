using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRule
{
    public class CodeRuleService
    {
        private readonly string _connectionString;

        public CodeRuleService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 获取下一个编码
        public string GetNextCode(string ruleName, Dictionary<string, string> variables = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var command = new SqlCommand("sp_GetNextCode", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@RuleName", ruleName);

                if (variables != null && variables.Count > 0)
                {
                    string jsonVariables = JsonConvert.SerializeObject(variables);
                    command.Parameters.AddWithValue("@Variables", jsonVariables);
                }
                else
                {
                    command.Parameters.AddWithValue("@Variables", DBNull.Value);
                }

                var outputParam = new SqlParameter("@ResultCode", SqlDbType.NVarChar, 500)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                command.ExecuteNonQuery();

                return outputParam.Value != DBNull.Value ? outputParam.Value.ToString() : null;
            }
        }

        // 添加或更新编码规则
        public int SaveCodeRule(CodeRule rule)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 保存主表
                        string sql;
                        if (rule.RuleId == 0)
                        {
                            sql = @"INSERT INTO GTCodeRule (RuleName, RuleDescription, IsActive, CreatedBy, ModifiedBy,CodeRegex)
                                VALUES (@RuleName, @RuleDescription, @IsActive, @CreatedBy, @ModifiedBy,@CodeRegex);
                                SELECT SCOPE_IDENTITY();";
                        }
                        else
                        {
                            sql = @"UPDATE GTCodeRule SET 
                                RuleName = @RuleName,
                                RuleDescription = @RuleDescription,
                                IsActive = @IsActive,
                                ModifiedTime = GETDATE(),
                                ModifiedBy = @ModifiedBy,
                                CodeRegex=@CodeRegex
                                WHERE RuleId = @RuleId;
                                SELECT @RuleId;";
                        }

                        var command = new SqlCommand(sql, connection, transaction);
                        command.Parameters.AddWithValue("@RuleName", rule.RuleName);
                        command.Parameters.AddWithValue("@RuleDescription", rule.RuleDescription ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IsActive", rule.IsActive);
                        command.Parameters.AddWithValue("@CreatedBy", rule.CreatedBy ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ModifiedBy", rule.ModifiedBy ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CodeRegex", rule.CodeRegex ?? (object)DBNull.Value);

                        if (rule.RuleId > 0)
                        {
                            command.Parameters.AddWithValue("@RuleId", rule.RuleId);
                        }

                        int ruleId = Convert.ToInt32(command.ExecuteScalar());

                        // 删除旧的明细
                        if (rule.RuleId > 0)
                        {
                            command = new SqlCommand("DELETE FROM GTCodeRuleDetail WHERE RuleId = @RuleId", connection, transaction);
                            command.Parameters.AddWithValue("@RuleId", ruleId);
                            command.ExecuteNonQuery();
                        }

                        // 插入新的明细
                        foreach (var detail in rule.Details)
                        {
                            command = new SqlCommand(@"
                            INSERT INTO GTCodeRuleDetail (RuleId, SegmentType, SegmentValue, SegmentFormat, SegmentLength, SegmentOrder, PaddingChar, PaddingSide)
                            VALUES (@RuleId, @SegmentType, @SegmentValue, @SegmentFormat, @SegmentLength, @SegmentOrder, @PaddingChar, @PaddingSide)",
                                connection, transaction);

                            command.Parameters.AddWithValue("@RuleId", ruleId);
                            command.Parameters.AddWithValue("@SegmentType", detail.SegmentType);
                            command.Parameters.AddWithValue("@SegmentValue", detail.SegmentValue ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@SegmentFormat", detail.SegmentFormat ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@SegmentLength", detail.SegmentLength ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@SegmentOrder", detail.SegmentOrder);
                            command.Parameters.AddWithValue("@PaddingChar", detail.PaddingChar);
                            command.Parameters.AddWithValue("@PaddingSide", detail.PaddingSide ?? (object)DBNull.Value);

                            command.ExecuteNonQuery();
                        }

                        // 处理序列号配置
                        if (rule.Details.Any(d => d.SegmentType == "SerialNumber"))
                        {
                            command = new SqlCommand(@"
                            IF EXISTS (SELECT 1 FROM GTCodeSerialNumber WHERE RuleId = @RuleId)
                                UPDATE GTCodeSerialNumber SET 
                                    ResetMode = @ResetMode,
                                    LastResetDate = @LastResetDate
                                WHERE RuleId = @RuleId
                            ELSE
                                INSERT INTO GTCodeSerialNumber (RuleId, Prefix, ResetMode, LastResetDate)
                                VALUES (@RuleId, @Prefix, @ResetMode, @LastResetDate)",
                                connection, transaction);

                            command.Parameters.AddWithValue("@RuleId", ruleId);
                            command.Parameters.AddWithValue("@Prefix", $"{rule.RuleName}_SN");
                            command.Parameters.AddWithValue("@ResetMode", "Never"); // 默认不重置
                            command.Parameters.AddWithValue("@LastResetDate", DateTime.Now);

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return ruleId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // 获取编码规则
        public CodeRule GetCodeRule(int ruleId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var rule = new CodeRule();

                // 获取主表信息
                var command = new SqlCommand("SELECT * FROM GTCodeRule WHERE RuleId = @RuleId", connection);
                command.Parameters.AddWithValue("@RuleId", ruleId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rule.RuleId = Convert.ToInt32(reader["RuleId"]);
                        rule.RuleName = reader["RuleName"].ToString();
                        rule.RuleDescription = reader["RuleDescription"] != DBNull.Value ? reader["RuleDescription"].ToString() : null;
                        rule.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        rule.CreatedTime = Convert.ToDateTime(reader["CreatedTime"]);
                        rule.ModifiedTime = Convert.ToDateTime(reader["ModifiedTime"]);
                        rule.CreatedBy = reader["CreatedBy"] != DBNull.Value ? reader["CreatedBy"].ToString() : null;
                        rule.ModifiedBy = reader["ModifiedBy"] != DBNull.Value ? reader["ModifiedBy"].ToString() : null;
                        rule.CodeRegex = reader["CodeRegex"] != DBNull.Value ? reader["ModifiedBy"].ToString() : null;

                    }
                    else
                    {
                        return null;
                    }
                }

                // 获取明细
                command = new SqlCommand("SELECT * FROM GTCodeRuleDetail WHERE RuleId = @RuleId ORDER BY SegmentOrder", connection);
                command.Parameters.AddWithValue("@RuleId", ruleId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var detail = new CodeRuleDetail
                        {
                            DetailId = Convert.ToInt32(reader["DetailId"]),
                            RuleId = Convert.ToInt32(reader["RuleId"]),
                            SegmentType = reader["SegmentType"].ToString(),
                            SegmentValue = reader["SegmentValue"].ToString(),
                            SegmentFormat = reader["SegmentFormat"] != DBNull.Value ? reader["SegmentFormat"].ToString() : null,
                            SegmentLength = reader["SegmentLength"] != DBNull.Value ? Convert.ToInt32(reader["SegmentLength"]) : (int?)null,
                            SegmentOrder = Convert.ToInt32(reader["SegmentOrder"]),
                            PaddingChar = reader["PaddingChar"].ToString()[0],
                            PaddingSide = reader["PaddingSide"].ToString()
                        };

                        rule.Details.Add(detail);
                    }
                }

                return rule;
            }
        }


        public List<CodeRule> GetAllRules()
        {
            var rules = new List<CodeRule>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // 获取所有规则主表信息
                var command = new SqlCommand("SELECT * FROM GTCodeRule ORDER BY RuleName", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rule = new CodeRule
                        {
                            RuleId = Convert.ToInt32(reader["RuleId"]),
                            RuleName = reader["RuleName"].ToString(),
                            RuleDescription = reader["RuleDescription"] != DBNull.Value ? reader["RuleDescription"].ToString() : null,
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            CreatedTime = Convert.ToDateTime(reader["CreatedTime"]),
                            ModifiedTime = Convert.ToDateTime(reader["ModifiedTime"]),
                            CreatedBy = reader["CreatedBy"] != DBNull.Value ? reader["CreatedBy"].ToString() : null,
                            ModifiedBy = reader["ModifiedBy"] != DBNull.Value ? reader["ModifiedBy"].ToString() : null,
                            CodeRegex = reader["CodeRegex"] != DBNull.Value ? reader["CodeRegex"].ToString() : null,
                        };

                        rules.Add(rule);
                    }
                }

                // 为每个规则加载明细
                foreach (var rule in rules)
                {
                    command = new SqlCommand("SELECT * FROM GTCodeRuleDetail WHERE RuleId = @RuleId ORDER BY SegmentOrder", connection);
                    command.Parameters.AddWithValue("@RuleId", rule.RuleId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detail = new CodeRuleDetail
                            {
                                DetailId = Convert.ToInt32(reader["DetailId"]),
                                RuleId = Convert.ToInt32(reader["RuleId"]),
                                SegmentType = reader["SegmentType"].ToString(),
                                SegmentValue = reader["SegmentValue"].ToString(),
                                SegmentFormat = reader["SegmentFormat"] != DBNull.Value ? reader["SegmentFormat"].ToString() : null,
                                SegmentLength = reader["SegmentLength"] != DBNull.Value ? Convert.ToInt32(reader["SegmentLength"]) : (int?)null,
                                SegmentOrder = Convert.ToInt32(reader["SegmentOrder"]),
                                PaddingChar = reader["PaddingChar"].ToString()[0],
                                PaddingSide = reader["PaddingSide"].ToString()
                            };

                            rule.Details.Add(detail);
                        }
                    }
                }
            }

            return rules;
        }

    }
}
