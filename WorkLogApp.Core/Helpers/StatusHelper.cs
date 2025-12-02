using System;
using System.Collections.Generic;
using WorkLogApp.Core.Enums;

namespace WorkLogApp.Core.Helpers
{
    public static class StatusHelper
    {
        /// <summary>
        /// 获取状态的中文描述
        /// </summary>
        public static string ToChinese(this StatusEnum status)
        {
            switch (status)
            {
                case StatusEnum.Todo: return "待办";
                case StatusEnum.Doing: return "进行中";
                case StatusEnum.Done: return "已完成";
                case StatusEnum.Blocked: return "已阻塞";
                case StatusEnum.Cancelled: return "已取消";
                default: return status.ToString();
            }
        }

        /// <summary>
        /// 从字符串（中文、英文、数字）解析状态
        /// </summary>
        public static StatusEnum Parse(string s)
        {
            var text = (s ?? string.Empty).Trim();
            if (int.TryParse(text, out var iv))
            {
                if (Enum.IsDefined(typeof(StatusEnum), iv))
                    return (StatusEnum)iv;
            }
            if (Enum.TryParse<StatusEnum>(text, true, out var status)) return status;
            
            switch (text)
            {
                case "待办": return StatusEnum.Todo;
                case "进行中": return StatusEnum.Doing;
                case "已完成": return StatusEnum.Done;
                case "阻塞":
                case "已阻塞":
                case "受阻": return StatusEnum.Blocked;
                case "已取消":
                case "取消": return StatusEnum.Cancelled;
                default: return StatusEnum.Todo;
            }
        }

        /// <summary>
        /// 获取用于下拉列表绑定的数据源
        /// </summary>
        public static List<KeyValuePair<StatusEnum, string>> GetList()
        {
            return new List<KeyValuePair<StatusEnum, string>>
            {
                new KeyValuePair<StatusEnum, string>(StatusEnum.Todo, StatusEnum.Todo.ToChinese()),
                new KeyValuePair<StatusEnum, string>(StatusEnum.Doing, StatusEnum.Doing.ToChinese()),
                new KeyValuePair<StatusEnum, string>(StatusEnum.Done, StatusEnum.Done.ToChinese()),
                new KeyValuePair<StatusEnum, string>(StatusEnum.Blocked, StatusEnum.Blocked.ToChinese()),
                new KeyValuePair<StatusEnum, string>(StatusEnum.Cancelled, StatusEnum.Cancelled.ToChinese())
            };
        }
    }
}
