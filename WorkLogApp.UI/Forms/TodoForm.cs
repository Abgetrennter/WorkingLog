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
                    _txtContent.Text = File.ReadAllText(_filePath, Encoding.UTF8);
                    // Reset dirty flag after loading
                    _isDirty = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载待办事项失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // If there are unsaved changes (that haven't been auto-saved yet), we might want to save them or discard?
            // Requirement says "Save button" and "Cancel button". 
            // "Cancel" usually implies discarding changes since last save, but with auto-save, it's tricky.
            // However, the requirement says "Text content real-time auto-save draft (prevent loss)".
            // So "Cancel" might just close the window. 
            // But if the user typed something and hit Cancel, they might expect it to NOT be saved permanently if it wasn't auto-saved?
            // Actually, if auto-save is "real-time", then the file is already updated.
            // So "Cancel" just closes. "Save" also saves and closes.
            
            // To be safe and follow standard "Cancel" behavior, we might want to revert? 
            // But "Auto-save draft" implies we WANT to keep it.
            // So I will just close. The auto-save ensures data isn't lost. 
            // If the user explicitly wants to "Save", we ensure it's saved.
            
            // Wait, if I open, delete everything, and hit Cancel, it should probably revert?
            // But "Auto-save" contradicts "Cancel reverts".
            // "Draft" implies it's saved somewhere.
            // Given the requirement "Prevent loss", persistence is key.
            // I'll stick to: Auto-save persists to file. Cancel just closes (and maybe stops pending save).
            // But if I want to be very strict: 
            // Maybe "Save" is just "Close with explicit save" and "Cancel" is "Close without explicit save (but auto-save might have run)".
            
            // Let's assume "Cancel" just closes the form.
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
