using System.Collections.Generic;
using WorkLogApp.Core.Enums;

namespace WorkLogApp.Core.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CategoryTypeEnum Type { get; set; }
        public int? ParentId { get; set; }
        public List<Category> Children { get; set; } = new List<Category>();
    }
}