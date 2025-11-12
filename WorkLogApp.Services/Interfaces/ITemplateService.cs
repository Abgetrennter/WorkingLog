using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    public interface ITemplateService
    {
        bool LoadTemplates(string templatesJsonPath);
        bool SaveTemplates();
        string Render(string formatTemplate, Dictionary<string, object> fieldValues, WorkLogItem item);
        CategoryTemplate GetCategoryTemplate(string categoryName);
        CategoryTemplate GetMergedCategoryTemplate(string categoryName);
        IEnumerable<string> GetCategoryNames();
        bool AddOrUpdateCategoryTemplate(string categoryName, CategoryTemplate template);
        bool RemoveCategory(string categoryName);
    }
}