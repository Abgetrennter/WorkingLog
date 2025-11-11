using System.Collections.Generic;
using WorkLogApp.Core.Models;

namespace WorkLogApp.Services.Interfaces
{
    public interface ITemplateService
    {
        bool LoadTemplates(string templatesJsonPath);
        string Render(string formatTemplate, Dictionary<string, object> fieldValues, WorkLogItem item);
    }
}