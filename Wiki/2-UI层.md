# 2. UI 层（WorkLogApp.UI）

## 组件结构

- `Program`：入口初始化样式、注册全局异常、加载模板后启动 `MainForm`。参考：`WorkLogApp.UI/Program.cs:1–76`
- `MainForm`：列表展示、月/日切换、拖拽排序、双击编辑、每日总结、保存与打开文件位置。参考：`WorkLogApp.UI/Forms/MainForm.cs:1–451`
- `ItemCreateForm`：模板驱动的动态表单，渲染文本后进入编辑保存。参考：`WorkLogApp.UI/Forms/ItemCreateForm.cs:22–118`
- `ItemEditForm`：纯文本编辑与保存（Excel 与文本备份）。参考：`WorkLogApp.UI/Forms/ItemEditForm.cs:75–139`, `:141–171`
- `CategoryManageForm`：分类与模板管理（新增、子分类、占位符插入、保存）。参考：`WorkLogApp.UI/Forms/CategoryManageForm.cs:136–200`, `:202–216`, `:279–324`
- `ImportWizardForm`：选择文件预览与批量导入。参考：`WorkLogApp.UI/Forms/ImportWizardForm.cs:44–55`, `:57–84`, `:86–119`
- `Controls`：
  - `CategoryTreeComboBox`：树形下拉选择分类（按 `-` 路径表示层级）。参考：`WorkLogApp.UI/Controls/CategoryTreeComboBox.cs:51–85`
  - `DynamicFormPanel`：根据占位符动态生成表单控件，支持 `text/textarea/select/checkbox/datetime`。参考：`WorkLogApp.UI/Controls/DynamicFormPanel.cs:19–79`
- `UIStyleManager`：统一字体、缩放、主题与抗锯齿，富文本行距控制。参考：`WorkLogApp.UI/UI/UIStyleManager.cs:45–61`, `:83–161`, `:342–381`

## 典型交互代码片段

双击编辑并重写当月 Excel：`WorkLogApp.UI/Forms/MainForm.cs:259–287`

```csharp
private void OnListViewDoubleClick(object sender, EventArgs e)
{
    if (_listView.SelectedItems.Count == 0) return;
    var lv = _listView.SelectedItems[0];
    var item = lv.Tag as WorkLogItem;
    using (var form = new ItemEditForm(item, null))
    {
        var result = form.ShowDialog(this);
        if (result == DialogResult.OK)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(dataDir);
            var month = _monthPicker.Value;
            IImportExportService svc = new ImportExportService();
            svc.RewriteMonth(month, _allMonthItems, dataDir);
            RefreshItems();
        }
    }
}
```

每日总结生成与写入：`WorkLogApp.UI/Forms/MainForm.cs:296–314`

```csharp
private void OnDailySummaryClick(object sender, EventArgs e)
{
    var selectedDate = _dayPicker.Value.Date;
    var day = _allMonthItems.FirstOrDefault(d => d.LogDate.Date == selectedDate);
    var existing = day?.DailySummary;
    var fallback = string.Join("；", _currentItems.Select(it => it.ItemTitle).Where(s => !string.IsNullOrWhiteSpace(s)));
    using (var form = new DailySummaryForm(day?.Items ?? new System.Collections.Generic.List<WorkLogItem>(), existing))
    {
        var result = form.ShowDialog(this);
        if (result == DialogResult.OK)
        {
            var text = form.SummaryText ?? string.Empty;
            ApplyDailySummary(selectedDate, text);
            OnSaveClick(null, EventArgs.Empty);
        }
    }
}
```

拖拽排序与排序值更新：`WorkLogApp.UI/Forms/MainForm.cs:329–371`, `:382–412`

```csharp
private void OnListViewDragDrop(object sender, DragEventArgs e)
{
    var dragged = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
    var p = _listView.PointToClient(new Point(e.X, e.Y));
    var target = _listView.GetItemAt(p.X, p.Y);
    int fromIndex = dragged.Index;
    int toIndex = target != null ? target.Index : _listView.Items.Count - 1;
    _listView.Items.RemoveAt(fromIndex);
    if (toIndex > fromIndex) toIndex--;
    _listView.Items.Insert(toIndex, dragged);
}

private void UpdateSortOrderByCurrentView()
{
    if (!_chkShowByMonth.Checked)
    {
        int order = 1; foreach (var it in _currentItems) it.SortOrder = order++;
        return;
    }
    var counters = new System.Collections.Generic.Dictionary<DateTime, int>();
    foreach (var it in _currentItems)
    {
        var key = it.LogDate.Date;
        if (!counters.TryGetValue(key, out var cnt)) cnt = 0; cnt += 1; counters[key] = cnt;
        it.SortOrder = cnt;
    }
}
```

打开当月文件位置：`WorkLogApp.UI/Forms/MainForm.cs:414–449`

```csharp
private void OnOpenFileLocationClick(object sender, EventArgs e)
{
    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
    var dataDir = Path.Combine(baseDir, "Data");
    Directory.CreateDirectory(dataDir);
    var month = _monthPicker.Value;
    var fileName = "worklog_" + new DateTime(month.Year, month.Month, 1).ToString("yyyyMM") + ".xlsx";
    var filePath = Path.Combine(dataDir, fileName);
    if (File.Exists(filePath))
    {
        Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = "/e,/select,\"" + filePath + "\"", UseShellExecute = true });
    }
    else
    {
        Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = dataDir, UseShellExecute = true });
    }
}
```

模板驱动创建与渲染保存：`WorkLogApp.UI/Forms/ItemCreateForm.cs:66–76`, `:78–118`

```csharp
private void BuildFormForCategory()
{
    var categoryName = _categoryCombo.SelectedCategoryName;
    var catTpl = _templateService.GetMergedCategoryTemplate(categoryName);
    _formPanel.BuildForm(catTpl?.Placeholders ?? new System.Collections.Generic.Dictionary<string, string>(), catTpl?.Options);
}

private void OnGenerateAndSave(object sender, EventArgs e)
{
    var categoryName = _categoryCombo.SelectedCategoryName;
    var catTpl = _templateService.GetMergedCategoryTemplate(categoryName);
    var values = _formPanel.GetFieldValues();
    values["CategoryPath"] = categoryName ?? string.Empty;
    var item = new WorkLogItem { LogDate = _datePicker.Value.Date, ItemTitle = _titleBox.Text?.Trim(), CategoryId = StableIdFromName(categoryName), Tags = categoryName };
    var content = _templateService.Render(catTpl.FormatTemplate, values, item);
    item.ItemContent = content;
    using (var editor = new ItemEditForm(item, content)) { var result = editor.ShowDialog(this); if (result == DialogResult.OK) Close(); }
}
```

动态表单生成与取值：`WorkLogApp.UI/Controls/DynamicFormPanel.cs:19–79`, `:82–101`

```csharp
public void BuildForm(Dictionary<string, string> placeholders, Dictionary<string, List<string>> options)
{
    Controls.Clear();
    var y = 10;
    foreach (var kv in placeholders)
    {
        var name = kv.Key; var type = kv.Value?.ToLowerInvariant() ?? "text";
        Control input = type == "textarea" ? (Control)new RichTextBox() : type == "datetime" ? new DateTimePicker() : type == "select" ? new ComboBox() : type == "checkbox" ? new CheckedListBox() : new TextBox();
        input.Location = new Point(120, y);
        Controls.Add(input);
        y += input.Height + 15;
    }
}

public Dictionary<string, object> GetFieldValues()
{
    var dict = new Dictionary<string, object>();
    foreach (var kv in _controls)
    {
        var ctrl = kv.Value;
        object val = ctrl is TextBox tb ? (object)tb.Text : ctrl is RichTextBox rtb ? rtb.Text : ctrl is DateTimePicker dt ? (object)dt.Value : ctrl is ComboBox cb ? cb.SelectedItem?.ToString() : ctrl is CheckedListBox clb ? string.Join("、", clb.CheckedItems.Cast<object>().Select(x => x.ToString())) : null;
        dict[kv.Key] = val;
    }
    return dict;
}
```

树形分类选择：`WorkLogApp.UI/Controls/CategoryTreeComboBox.cs:51–85`

```csharp
public void ReloadCategories()
{
    _treeView.BeginUpdate(); _treeView.Nodes.Clear();
    var map = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);
    var names = _templateService.GetCategoryNames() ?? Enumerable.Empty<string>();
    foreach (var name in names.OrderBy(n => n))
    {
        var parts = name.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        string path = string.Empty; TreeNode parent = null;
        for (int i = 0; i < parts.Length; i++)
        {
            path = i == 0 ? parts[0] : path + "-" + parts[i];
            if (!map.TryGetValue(path, out var node))
            {
                node = new TreeNode(parts[i]) { Name = path };
                if (parent == null) _treeView.Nodes.Add(node); else parent.Nodes.Add(node);
                map[path] = node;
            }
            parent = node;
        }
    }
    _treeView.ExpandAll(); _treeView.EndUpdate();
}
```

统一样式与主题：`WorkLogApp.UI/UI/UIStyleManager.cs:69–81`, `:88–161`

```csharp
public static void ApplyVisualEnhancements(Form form, float scaleFactor = 1.5f)
{
    form.AutoScaleMode = AutoScaleMode.Dpi; form.Font = BodyFont; TryEnableDoubleBuffer(form); ApplyToControlTree(form);
}

private static void ApplyLightThemeRecursive(Control c)
{
    c.ForeColor = Color.FromArgb(32, 32, 32);
    if (c is Form || c is Panel || c is TableLayoutPanel || c is FlowLayoutPanel) c.BackColor = Color.FromArgb(245, 247, 250);
    foreach (Control child in c.Controls) ApplyLightThemeRecursive(child);
}
```

## 设计期支持要点

- 多数窗体在设计期填充示例数据，便于在设计器中预览布局与样式；运行时通过 `UIStyleManager.IsDesignMode` 规避外部依赖。

