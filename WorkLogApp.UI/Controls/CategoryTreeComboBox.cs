using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.UI.Controls
{
    public class CategoryTreeComboBox : ComboBox
    {
        private readonly ITemplateService _templateService;

        public CategoryTreeComboBox(ITemplateService templateService)
        {
            _templateService = templateService;
            DropDownStyle = ComboBoxStyle.DropDownList;
            ReloadCategories();
        }

        public void ReloadCategories()
        {
            Items.Clear();
            var names = _templateService.GetCategoryNames() ?? Enumerable.Empty<string>();
            foreach (var n in names)
            {
                Items.Add(n);
            }
            if (Items.Count > 0) SelectedIndex = 0;
        }

        public string SelectedCategoryName => SelectedItem?.ToString();
    }
}