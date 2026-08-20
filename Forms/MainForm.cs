using LayerGenForDotNet4.Database;
using LayerGenForDotNet4.Generator;
using LayerGenForDotNet4.Models;
using Microsoft.Data.SqlClient;
using System.ComponentModel;

namespace LayerGenForDotNet4.Forms
{
    public partial class MainForm : Form
    {
        private SqlSchemaReader? _reader;
        private List<TableInfo> _tables = new();
        private List<TableInfo> _filteredTables = new();
        private string _connectionString = "";

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        // ─── Load / Save Settings ───────────────────────────────────────────────

        private void LoadSettings()
        {
            try
            {
                txtServer.Text = Settings.Get("Server", ".");
                cboDatabase.Text = Settings.Get("Database", "");
                txtSchema.Text = Settings.Get("Schema", "dbo");
                txtUser.Text = Settings.Get("User", "");
                txtPassword.Text = Settings.Get("Password", "");

                bool winAuth = Settings.GetBool("WindowsAuth", true);
                rbWindows.Checked = winAuth;
                rbSql.Checked = !winAuth;

                txtOutput.Text = Settings.Get("OutputDir", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                txtNamespace.Text = Settings.Get("Namespace", "");
                chkSuppressComments.Checked = Settings.GetBool("SuppressComments", false);
                chkFalseErase.Checked = Settings.GetBool("FalseErase", false);
                txtFalseEraseField.Text = Settings.Get("FalseEraseField", "IsDeleted");

                chkGenCS.Checked = Settings.GetBool("GenCS", false);
                chkGenVB.Checked = Settings.GetBool("GenVB", true);
                chkGenSP.Checked = Settings.GetBool("GenSP", true);
                chkGenCustom.Checked = Settings.GetBool("GenCustom", false);
                chkGenInit.Checked = Settings.GetBool("GenInit", true);
                chkRunSP.Checked = Settings.GetBool("RunSP", false);
            }
            catch { }
        }

        private void SaveSettings()
        {
            Settings.Set("Server", txtServer.Text);
            Settings.Set("Database", cboDatabase.Text);
            Settings.Set("Schema", txtSchema.Text);
            Settings.Set("User", txtUser.Text);
            Settings.Set("Password", txtPassword.Text);
            Settings.SetBool("WindowsAuth", rbWindows.Checked);
            Settings.Set("OutputDir", txtOutput.Text);
            Settings.Set("Namespace", txtNamespace.Text);
            Settings.SetBool("SuppressComments", chkSuppressComments.Checked);
            Settings.SetBool("FalseErase", chkFalseErase.Checked);
            Settings.Set("FalseEraseField", txtFalseEraseField.Text);
            Settings.SetBool("GenCS", chkGenCS.Checked);
            Settings.SetBool("GenVB", chkGenVB.Checked);
            Settings.SetBool("GenSP", chkGenSP.Checked);
            Settings.SetBool("GenCustom", chkGenCustom.Checked);
            Settings.SetBool("GenInit", chkGenInit.Checked);
            Settings.SetBool("RunSP", chkRunSP.Checked);
        }

        // ─── Connection ─────────────────────────────────────────────────────────

        private string BuildConnectionString() =>
            SqlSchemaReader.BuildConnectionString(
                server: txtServer.Text.Trim(),
                database: cboDatabase.Text.Trim(),
                windowsAuth: rbWindows.Checked,
                user: txtUser.Text.Trim(),
                password: txtPassword.Text.Trim());

        /// <summary>Fills the database combo from the server; keeps the current selection if still present</summary>
        private void LoadDatabaseList(bool showErrors)
        {
            string server = txtServer.Text.Trim();
            if (string.IsNullOrEmpty(server))
            {
                if (showErrors)
                    MessageBox.Show("أدخل اسم السيرفر أولاً.", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string current = cboDatabase.Text;
            Cursor = Cursors.WaitCursor;
            try
            {
                var dbs = SqlSchemaReader.GetDatabases(
                    server: server,
                    windowsAuth: rbWindows.Checked,
                    user: txtUser.Text.Trim(),
                    password: txtPassword.Text.Trim());

                cboDatabase.Items.Clear();
                cboDatabase.Items.AddRange(dbs.ToArray());
                cboDatabase.Text = current;   // preserve what the user already typed/chose

                if (showErrors && dbs.Count == 0)
                    MessageBox.Show("لا توجد قواعد بيانات متاحة على هذا السيرفر.", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (showErrors)
                    MessageBox.Show($"لا يمكن قراءة قائمة قواعد البيانات:\n{ex.Message}",
                        "خطأ في الاتصال", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnRefreshDbs_Click(object sender, EventArgs e) => LoadDatabaseList(true);

        private void cboDatabase_DropDown(object sender, EventArgs e)
        {
            // fill on first drop-down so the user does not have to press refresh
            if (cboDatabase.Items.Count == 0)
                LoadDatabaseList(false);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _connectionString = BuildConnectionString();
            _reader = new SqlSchemaReader(_connectionString);

            if (!_reader.TestConnection(out string err))
            {
                MessageBox.Show($"لا يمكن الاتصال بقاعدة البيانات:\n{err}",
                    "خطأ في الاتصال", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _tables = _reader.GetTables(txtSchema.Text.Trim());
                txtSearch.Enabled = true;
                txtSearch.Text = "";
                PopulateTableList();
                lblStatus.Text = $"✅ تم الاتصال – {_tables.Count} جدول/view";
                lblStatus.ForeColor = Color.DarkGreen;
                btnCreateLayers.Enabled = true;
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateTableList()
        {
            string filter = txtSearch?.Text?.Trim() ?? "";
            _filteredTables = string.IsNullOrEmpty(filter)
                ? new List<TableInfo>(_tables)
                : _tables.Where(t => t.TableName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            clbTables.Items.Clear();
            foreach (var t in _filteredTables)
            {
                string display = t.IsView ? $"[VIEW]  {t.TableName}" : t.TableName;
                clbTables.Items.Add(display, false);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            PopulateTableList();
        }

        // ─── Select / Deselect All ──────────────────────────────────────────────

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbTables.Items.Count; i++)
                clbTables.SetItemChecked(i, true);
        }

        private void btnDeselectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbTables.Items.Count; i++)
                clbTables.SetItemChecked(i, false);
        }

        // ─── Browse Output ──────────────────────────────────────────────────────

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog { SelectedPath = txtOutput.Text };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtOutput.Text = dlg.SelectedPath;
        }

        // ─── Create Layers ──────────────────────────────────────────────────────

        private void btnCreateLayers_Click(object sender, EventArgs e)
        {
            if (_reader == null) return;

            if (!chkGenCS.Checked && !chkGenVB.Checked && !chkGenSP.Checked && !chkRunSP.Checked)
            {
                MessageBox.Show("الرجاء اختيار نوع المخرجات (C# أو VB.NET أو Stored Procedures).",
                    "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selected = new List<TableInfo>();
            for (int i = 0; i < clbTables.Items.Count; i++)
                if (clbTables.GetItemChecked(i))
                    selected.Add(_filteredTables[i]);

            if (selected.Count == 0)
            {
                MessageBox.Show("الرجاء اختيار جدول واحد على الأقل.",
                    "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string outputDir = txtOutput.Text.Trim();
            if (!Directory.Exists(outputDir))
            {
                MessageBox.Show("المجلد المحدد غير موجود.", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Namespace – empty string is valid (no namespace wrapper)
            string ns = txtNamespace.Text.Trim();
            bool suppress = chkSuppressComments.Checked;
            bool falseErase = chkFalseErase.Checked;
            string falseEraseFld = txtFalseEraseField.Text.Trim();
            bool genCS = chkGenCS.Checked;
            bool genVB = chkGenVB.Checked;
            bool runSP = chkRunSP.Checked;
            bool genSP = chkGenSP.Checked || runSP;  // RunSP يستلزم توليد SP
            bool genCustom = chkGenCustom.Checked;
            bool genInit = chkGenInit.Checked;

            SaveSettings();

            var progress = new ProgressDialog(selected.Count);
            progress.Show(this);

            var worker = new BackgroundWorker { WorkerReportsProgress = true };

            worker.DoWork += (s, ev) =>
            {
                // ── All files go directly into outputDir – no subfolders ──────────
                // Accumulated SP script (one file for all tables)
                var spScript = new System.Text.StringBuilder();
                spScript.AppendLine("-- ===========================================================================");
                spScript.AppendLine("-- This file was generated by LayerGenForDotNet4 application");
                spScript.AppendLine($"-- Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                spScript.AppendLine("-- Naser Almadi");
                spScript.AppendLine("-- nasmn77@gmail.com");
                spScript.AppendLine("-- ===========================================================================");
                spScript.AppendLine();

                // ── Init files: Universal + Interfaces ────────────────────────────
                if (genInit)
                {
                    if (genVB)
                    {
                        string univPath = Path.Combine(outputDir, "Universal.VB");
                        if (!File.Exists(univPath))
                            File.WriteAllText(univPath,
                                CodeGeneratorVB.GenerateUniversalFile(ns, _connectionString, suppress),
                                System.Text.Encoding.UTF8);

                        string ifacePath = Path.Combine(outputDir, "Interfaces.VB");
                        if (!File.Exists(ifacePath))
                            File.WriteAllText(ifacePath,
                                CodeGeneratorVB.GenerateInterfacesFile(ns, suppress),
                                System.Text.Encoding.UTF8);
                    }
                    if (genCS)
                    {
                        string univPath = Path.Combine(outputDir, "Universal.cs");
                        if (!File.Exists(univPath))
                            File.WriteAllText(univPath,
                                CodeGeneratorCS.GenerateUniversalFile(ns, _connectionString, suppress),
                                System.Text.Encoding.UTF8);

                        string ifacePathCS = Path.Combine(outputDir, "Interfaces.cs");
                        if (!File.Exists(ifacePathCS))
                            File.WriteAllText(ifacePathCS,
                                CodeGeneratorCS.GenerateInterfacesFile(ns, suppress),
                                System.Text.Encoding.UTF8);
                    }
                }

                // ── Per-table generation ──────────────────────────────────────────
                int idx = 0;
                foreach (var table in selected)
                {
                    idx++;
                    worker.ReportProgress((idx * 100) / selected.Count, table.TableName);

                    try
                    {
                        _reader!.LoadTableDetails(table);

                        // VB.NET
                        if (genVB)
                        {
                            File.WriteAllText(
                                Path.Combine(outputDir, $"{table.TableName}Data.VB"),
                                CodeGeneratorVB.GenerateDataLayer(table, ns, suppress, falseErase, falseEraseFld),
                                System.Text.Encoding.UTF8);

                            File.WriteAllText(
                                Path.Combine(outputDir, $"{table.TableName}Business.VB"),
                                CodeGeneratorVB.GenerateBusinessLayer(table, ns, suppress),
                                System.Text.Encoding.UTF8);

                            // Custom files – only create if they don't exist yet
                            if (genCustom)
                            {
                                string dataCustomPath = Path.Combine(outputDir, $"{table.TableName}DataCustom.VB");
                                if (!File.Exists(dataCustomPath))
                                    File.WriteAllText(dataCustomPath,
                                        CodeGeneratorVB.GenerateDataCustomFile(table, ns, suppress),
                                        System.Text.Encoding.UTF8);

                                string bizCustomPath = Path.Combine(outputDir, $"{table.TableName}BusinessCustom.VB");
                                if (!File.Exists(bizCustomPath))
                                    File.WriteAllText(bizCustomPath,
                                        CodeGeneratorVB.GenerateBusinessCustomFile(table, ns, suppress),
                                        System.Text.Encoding.UTF8);
                            }
                        }

                        // C#
                        if (genCS)
                        {
                            File.WriteAllText(
                                Path.Combine(outputDir, $"{table.TableName}Data.cs"),
                                CodeGeneratorCS.GenerateDataLayer(table, ns, suppress, falseErase, falseEraseFld),
                                System.Text.Encoding.UTF8);

                            File.WriteAllText(
                                Path.Combine(outputDir, $"{table.TableName}Business.cs"),
                                CodeGeneratorCS.GenerateBusinessLayer(table, ns, suppress),
                                System.Text.Encoding.UTF8);

                            if (genCustom)
                            {
                                string dataCustomPathCS = Path.Combine(outputDir, $"{table.TableName}DataCustom.cs");
                                if (!File.Exists(dataCustomPathCS))
                                    File.WriteAllText(dataCustomPathCS,
                                        CodeGeneratorCS.GenerateDataCustomFile(table, ns, suppress),
                                        System.Text.Encoding.UTF8);

                                string bizCustomPathCS = Path.Combine(outputDir, $"{table.TableName}BusinessCustom.cs");
                                if (!File.Exists(bizCustomPathCS))
                                    File.WriteAllText(bizCustomPathCS,
                                        CodeGeneratorCS.GenerateBusinessCustomFile(table, ns, suppress),
                                        System.Text.Encoding.UTF8);
                            }
                        }

                        // Stored Procedures – append to one combined script
                        if (genSP)
                        {
                            spScript.Append(
                                CodeGeneratorCS.GenerateStoredProcedures(table, suppress, falseErase, falseEraseFld, txtSchema.Text.Trim()));
                        }
                    }
                    catch (Exception ex)
                    {
                        string errFile = Path.Combine(outputDir, "_errors.txt");
                        File.AppendAllText(errFile, $"[{table.TableName}] {ex.Message}\n");
                    }
                }

                // ── Write single combined SQL file ────────────────────────────────
                if (genSP && spScript.Length > 0)
                {
                    // كتابة الملف فقط إذا كان chkGenSP محدداً
                    if (chkGenSP.Checked)
                    {
                        string sqlPath = Path.Combine(outputDir, "Procedures.SQL");
                        File.WriteAllText(sqlPath, spScript.ToString(), System.Text.Encoding.UTF8);
                    }

                    // ── Optionally execute SP script against the database ─────────
                    if (runSP)
                    {
                        try
                        {
                            ExecuteScript(_connectionString, spScript.ToString());
                        }
                        catch (Exception ex)
                        {
                            string errFile = Path.Combine(outputDir, "_errors.txt");
                            File.AppendAllText(errFile, $"[RunSP] {ex.Message}\n");
                        }
                    }
                }
            };

            worker.ProgressChanged += (s, ev) =>
                progress.SetProgress(ev.ProgressPercentage, ev.UserState?.ToString() ?? "");

            worker.RunWorkerCompleted += (s, ev) =>
            {
                progress.Close();

                if (ev.Error != null)
                {
                    MessageBox.Show(ev.Error.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var parts = new List<string>();
                if (genVB) parts.Add("VB.NET");
                if (genCS) parts.Add("C#");
                if (genSP) parts.Add("Stored Procedures");

                lblStatus.Text = $"✅ تم توليد [{string.Join(" + ", parts)}] لـ {selected.Count} جدول بنجاح!";
                lblStatus.ForeColor = Color.DarkGreen;

                var result = MessageBox.Show(
                    $"تم توليد الكود بنجاح في:\n{outputDir}\n\nهل تريد فتح المجلد؟",
                    "تم بنجاح", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                    System.Diagnostics.Process.Start("explorer.exe", outputDir);
            };

            worker.RunWorkerAsync();
        }

        // ─── Execute SQL script against the database ────────────────────────────

        private static void ExecuteScript(string connectionString, string script)
        {
            // Split on GO statements (case-insensitive, alone on a line)
            var batches = System.Text.RegularExpressions.Regex.Split(
                script, @"^\s*GO\s*$",
                System.Text.RegularExpressions.RegexOptions.Multiline |
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            using var conn = new SqlConnection(connectionString);
            conn.Open();
            foreach (var batch in batches)
            {
                string sql = batch.Trim();
                if (string.IsNullOrEmpty(sql)) continue;
                using var cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 120;
                cmd.ExecuteNonQuery();
            }
        }

        // ─── UI events ──────────────────────────────────────────────────────────

        private void rbWindows_CheckedChanged(object sender, EventArgs e)
        {
            bool sqlAuth = rbSql.Checked;
            txtUser.Enabled = sqlAuth;
            txtPassword.Enabled = sqlAuth;
            lblUser.Enabled = sqlAuth;
            lblPassword.Enabled = sqlAuth;
        }

        private void chkFalseErase_CheckedChanged(object sender, EventArgs e)
            => txtFalseEraseField.Enabled = chkFalseErase.Checked;

        private void pnlRight_SizeChanged(object sender, EventArgs e)
        {
            int w = pnlRight.ClientSize.Width - 20;
            int h = pnlRight.ClientSize.Height - 140;
            lblTablesTitle.Width = w;
            txtSearch.Width = w;
            clbTables.Width = w;
            clbTables.Height = Math.Max(80, h);
            btnCreateLayers.Width = w;
            btnCreateLayers.Top = clbTables.Bottom + 8;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            pnlLeft.Height = this.ClientSize.Height - 50;
            pnlRight.Left = 360;
            pnlRight.Top = 10;
            pnlRight.Width = this.ClientSize.Width - 370;
            pnlRight.Height = this.ClientSize.Height - 50;
            lblStatus.Top = this.ClientSize.Height - 30;
            lblStatus.Width = this.ClientSize.Width - 20;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveSettings();
            base.OnFormClosing(e);
        }

        private void chkGenCS_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
