using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WorkLogApp.Core.Models
{
    public class Category
    {
        public string Id { get; set; }          // 唯一标识 (GUID)
        public string Name { get; set; }        // 分类名称
        public string ParentId { get; set; }    // 父节点ID (根节点为null)
        public int SortOrder { get; set; }      // 同级排序权重

        // 运行时属性
        [JsonIgnore]
        public List<Category> Children { get; set; } = new List<Category>();
    }
}
