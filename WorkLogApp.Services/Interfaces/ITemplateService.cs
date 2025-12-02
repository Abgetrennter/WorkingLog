using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    public interface ITemplateService
    {
        // Initialization
        bool LoadTemplates(string templatesJsonPath);
        bool SaveTemplates();

        // Category Operations
        List<Category> GetAllCategories();
        Category GetCategory(string id);
        Category CreateCategory(string name, string parentId);
        bool UpdateCategory(Category category);
        bool DeleteCategory(string id);
        bool MoveCategory(string id, string newParentId);

        // Template Operations
        List<WorkTemplate> GetTemplatesByCategory(string categoryId);
        WorkTemplate GetTemplate(string id);
        WorkTemplate CreateTemplate(WorkTemplate template);
        bool UpdateTemplate(WorkTemplate template);
        bool DeleteTemplate(string id);

        // Rendering
        string Render(string content, Dictionary<string, object> values, WorkLogItem item);
    }
}
