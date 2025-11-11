using System.Collections.Generic;
using System.Linq;
using WorkLogApp.Core.Models;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly List<Category> _categories = new List<Category>();

        public bool LoadCategories(string configPath)
        {
            _categories.Clear();
            // TODO: 后续从 JSON 加载分类树
            return true;
        }

        public Category GetCategoryById(int id) => _categories.FirstOrDefault(c => c.Id == id);

        public string GetCategoryPath(int id)
        {
            var cat = GetCategoryById(id);
            return cat?.Name ?? string.Empty;
        }

        public IEnumerable<Category> GetAllCategories() => _categories;
    }
}