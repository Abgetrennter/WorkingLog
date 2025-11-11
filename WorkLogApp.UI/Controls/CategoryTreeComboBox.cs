using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WorkLogApp.Services.Interfaces;

namespace WorkLogApp.UI.Controls
{
    public class CategoryTreeComboBox : ComboBox
    {
        private ITemplateService _templateService;

        public CategoryTreeComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public CategoryTreeComboBox(ITemplateService templateService) : this()
        {
            TemplateService = templateService;
        }

        public ITemplateService TemplateService
        {
            get => _templateService;
            set
            {
                _templateService = value;
                if (_templateService != null)
                {
                    ReloadCategories();
                }
            }
        }

        public void ReloadCategories()
        {
            Items.Clear();
            if (_templateService == null) return;
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