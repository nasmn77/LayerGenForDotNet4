namespace LayerGenForDotNet4.Forms
{
    public class ProgressDialog : Form
    {
        private ProgressBar _bar = null!;
        private Label _lblTable = null!;
        private Label _lblPct = null!;

        public ProgressDialog(int total)
        {
            this.Text = "جاري توليد الكود...";
            this.Size = new Size(420, 140);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
            this.BackColor = Color.White;

            _lblTable = new Label
            {
                Text = "جاري العمل...",
                Left = 12, Top = 12, Width = 390, Height = 22,
                Font = new Font("Segoe UI", 9.5f)
            };

            _bar = new ProgressBar
            {
                Left = 12, Top = 40, Width = 390, Height = 24,
                Minimum = 0, Maximum = 100, Style = ProgressBarStyle.Continuous
            };

            _lblPct = new Label
            {
                Text = "0%",
                Left = 12, Top = 72, Width = 390, Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray
            };

            this.Controls.AddRange(new Control[] { _lblTable, _bar, _lblPct });
        }

        public void SetProgress(int percent, string tableName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => SetProgress(percent, tableName));
                return;
            }
            _bar.Value = Math.Min(percent, 100);
            _lblTable.Text = $"⚙  جاري معالجة: {tableName}";
            _lblPct.Text = $"{percent}%";
        }
    }
}
