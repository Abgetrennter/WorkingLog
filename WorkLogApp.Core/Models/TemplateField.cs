using System.Collections.Generic;

namespace WorkLogApp.Core.Models
{
    /// <summary>
    /// 模板字段定义 - 结构化字段配置
    /// </summary>
    public class TemplateField
    {
        /// <summary>
        /// 字段标识（英文，用于内容替换）
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 字段显示名称（中文）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 字段类型：text, textarea, datetime, date, time, select, multiselect, 
        /// checkbox, radio, number, duration, rangeDateTime, autocomplete
        /// </summary>
        public string Type { get; set; } = "text";

        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// 验证规则（正则表达式）
        /// </summary>
        public string ValidationRule { get; set; }

        /// <summary>
        /// 输入提示
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// 显示顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 选项列表（用于 select/checkbox/radio 类型）
        /// </summary>
        public List<string> Options { get; set; } = new List<string>();

        /// <summary>
        /// 字段显示条件
        /// </summary>
        public FieldCondition Condition { get; set; }

        /// <summary>
        /// 帮助文本/说明
        /// </summary>
        public string HelpText { get; set; }
    }

    /// <summary>
    /// 字段显示条件
    /// </summary>
    public class FieldCondition
    {
        /// <summary>
        /// 依赖的字段Key
        /// </summary>
        public string DependsOn { get; set; }

        /// <summary>
        /// 操作符：eq(等于), ne(不等于), in(包含), contains(包含文本)
        /// </summary>
        public string Operator { get; set; } = "eq";

        /// <summary>
        /// 比较值
        /// </summary>
        public string Value { get; set; }
    }
}
