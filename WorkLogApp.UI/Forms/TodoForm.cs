using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WorkLogApp.UI.UI;

namespace WorkLogApp.UI.Forms
{
    public partial class TodoForm : Form
    {
        private readonly string _filePath;
        private bool _isDirty = false;
        private Timer _autoSaveTimer;
        private const string PlaceholderText = "【草稿本 - 个人便签】\r\n\r\n此处内容仅作为个人临时记录，自动保存到本地 todo.txt 文件。\r\n此处的待办事项不会自动结转到工作日志中，也不会出现在正式的工作日志列表里。\r\n\r\n如需创建正式的工作日志条目，请在主界面点击「创建」按钮。";

        public TodoForm()
        {
            InitializeComponent();
            UIStyleManager.ApplyVisualEnhancements(this);
            UIStyleManager.ApplyLightTheme(this);

            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "todo.txt");
            LoadContent();

            // Setup auto-save timer
            _autoSaveTimer = new Timer();
            _autoSaveTimer.Interval = 2000; // Auto-save every 2 seconds if changes occur
            _autoSaveTimer.Tick += OnAutoSaveTick;
            _autoSaveTimer.Start();

            _txtContent.TextChanged += OnContentChanged;
        }

        private void LoadContent()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var content = File.ReadAllText(_filePath, Encoding.UTF8);
                    _txtContent.Text = string.IsNullOrWhiteSpace(content) ? PlaceholderText : content;
                    // Reset dirty flag after loading
                    _isDirty = false;
                }
                else
                {
                    // 首次打开，显示提示文字
                    _txtContent.Text = PlaceholderText;
                    _isDirty = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载待办事项失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveContent()
        {
            try
            {
                File.WriteAllText(_filePath, _txtContent.Text, Encoding.UTF8);
                _isDirty = false;
            }
            catch (Exception ex)
            {
                // In auto-save, we might not want to show a popup every time, but for now let's log or ignore repeated errors
                // Ideally, we could show a status label
                System.Diagnostics.Debug.WriteLine($"Auto-save failed: {ex.Message}");
            }
        }

        private void OnContentChanged(object sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void OnAutoSaveTick(object sender, EventArgs e)
        {
            if (_isDirty)
            {
                SaveContent();
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            SaveContent();
            this.Close();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            // Cancel just closes the form. Auto-save ensures data isn't lost.
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _autoSaveTimer.Stop();
            if (_isDirty)
            {
                SaveContent();
            }
            base.OnFormClosing(e);
        }
    }
}
