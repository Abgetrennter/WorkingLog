using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    public interface ICategoryService
    {
        bool LoadCategories(string configPath);
        Category GetCategoryById(int id);
        string GetCategoryPath(int id);
        IEnumerable<Category> GetAllCategories();
    }
}