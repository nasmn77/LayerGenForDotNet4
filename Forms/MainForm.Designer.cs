namespace LayerGenForDotNet4.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlLeft = new Panel();
            lblTitle = new Label();
            lblServer = new Label();
            txtServer = new TextBox();
            lblDb = new Label();
            txtDatabase = new TextBox();
            grpAuth = new GroupBox();
            rbWindows = new RadioButton();
            rbSql = new RadioButton();
            lblUser = new Label();
            txtUser = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnConnect = new Button();
            sep1 = new Label();
            lblNs = new Label();
            txtNamespace = new TextBox();
            lblNsHint = new Label();
            lblOut = new Label();
            txtOutput = new TextBox();
            btnBrowse = new Button();
            grpLang = new GroupBox();
            chkGenVB = new CheckBox();
            chkGenCS = new CheckBox();
            chkGenSP = new CheckBox();
            grpExtra = new GroupBox();
            chkGenInit = new CheckBox();
            chkGenCustom = new CheckBox();
            chkRunSP = new CheckBox();
            grpFE = new GroupBox();
            chkFalseErase = new CheckBox();
            txtFalseEraseField = new TextBox();
            lblFEField = new Label();
            chkSuppressComments = new CheckBox();
            pnlRight = new Panel();
            lblTablesTitle = new Label();
            btnSelectAll = new Button();
            btnDeselectAll = new Button();
            clbTables = new CheckedListBox();
            btnCreateLayers = new Button();
            lblStatus = new Label();
            pnlLeft.SuspendLayout();
            grpAuth.SuspendLayout();
            grpLang.SuspendLayout();
            grpExtra.SuspendLayout();
            grpFE.SuspendLayout();
            pnlRight.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLeft
            // 
            pnlLeft.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pnlLeft.BackColor = Color.White;
            pnlLeft.BorderStyle = BorderStyle.FixedSingle;
            pnlLeft.Controls.Add(lblTitle);
            pnlLeft.Controls.Add(lblServer);
            pnlLeft.Controls.Add(txtServer);
            pnlLeft.Controls.Add(lblDb);
            pnlLeft.Controls.Add(txtDatabase);
            pnlLeft.Controls.Add(grpAuth);
            pnlLeft.Controls.Add(lblUser);
            pnlLeft.Controls.Add(txtUser);
            pnlLeft.Controls.Add(lblPassword);
            pnlLeft.Controls.Add(txtPassword);
            pnlLeft.Controls.Add(btnConnect);
            pnlLeft.Controls.Add(sep1);
            pnlLeft.Controls.Add(lblNs);
            pnlLeft.Controls.Add(txtNamespace);
            pnlLeft.Controls.Add(lblNsHint);
            pnlLeft.Controls.Add(lblOut);
            pnlLeft.Controls.Add(txtOutput);
            pnlLeft.Controls.Add(btnBrowse);
            pnlLeft.Controls.Add(grpLang);
            pnlLeft.Controls.Add(grpExtra);
            pnlLeft.Controls.Add(grpFE);
            pnlLeft.Controls.Add(chkSuppressComments);
            pnlLeft.Location = new Point(10, 10);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Padding = new Padding(10);
            pnlLeft.Size = new Size(340, 700);
            pnlLeft.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(25, 118, 210);
            lblTitle.Location = new Point(10, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(318, 28);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⚙  اتصال SQL Server";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblServer
            // 
            lblServer.Location = new Point(10, 46);
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(95, 22);
            lblServer.TabIndex = 1;
            lblServer.Text = "السيرفر:";
            lblServer.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtServer
            // 
            txtServer.Location = new Point(112, 44);
            txtServer.Name = "txtServer";
            txtServer.RightToLeft = RightToLeft.No;
            txtServer.Size = new Size(206, 24);
            txtServer.TabIndex = 2;
            // 
            // lblDb
            // 
            lblDb.Location = new Point(10, 76);
            lblDb.Name = "lblDb";
            lblDb.Size = new Size(95, 22);
            lblDb.TabIndex = 3;
            lblDb.Text = "قاعدة البيانات:";
            lblDb.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtDatabase
            // 
            txtDatabase.Location = new Point(112, 74);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.RightToLeft = RightToLeft.No;
            txtDatabase.Size = new Size(206, 24);
            txtDatabase.TabIndex = 4;
            // 
            // grpAuth
            // 
            grpAuth.Controls.Add(rbWindows);
            grpAuth.Controls.Add(rbSql);
            grpAuth.Location = new Point(10, 104);
            grpAuth.Name = "grpAuth";
            grpAuth.Size = new Size(318, 54);
            grpAuth.TabIndex = 5;
            grpAuth.TabStop = false;
            grpAuth.Text = "نوع المصادقة";
            // 
            // rbWindows
            // 
            rbWindows.Checked = true;
            rbWindows.Location = new Point(12, 20);
            rbWindows.Name = "rbWindows";
            rbWindows.Size = new Size(130, 22);
            rbWindows.TabIndex = 0;
            rbWindows.TabStop = true;
            rbWindows.Text = "Windows Auth";
            rbWindows.CheckedChanged += rbWindows_CheckedChanged;
            // 
            // rbSql
            // 
            rbSql.Location = new Point(152, 20);
            rbSql.Name = "rbSql";
            rbSql.Size = new Size(100, 22);
            rbSql.TabIndex = 1;
            rbSql.Text = "SQL Auth";
            rbSql.CheckedChanged += rbWindows_CheckedChanged;
            // 
            // lblUser
            // 
            lblUser.Enabled = false;
            lblUser.Location = new Point(10, 170);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(95, 22);
            lblUser.TabIndex = 6;
            lblUser.Text = "المستخدم:";
            lblUser.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtUser
            // 
            txtUser.Enabled = false;
            txtUser.Location = new Point(112, 168);
            txtUser.Name = "txtUser";
            txtUser.RightToLeft = RightToLeft.No;
            txtUser.Size = new Size(206, 24);
            txtUser.TabIndex = 7;
            // 
            // lblPassword
            // 
            lblPassword.Enabled = false;
            lblPassword.Location = new Point(10, 200);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(95, 22);
            lblPassword.TabIndex = 8;
            lblPassword.Text = "كلمة المرور:";
            lblPassword.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtPassword
            // 
            txtPassword.Enabled = false;
            txtPassword.Location = new Point(112, 198);
            txtPassword.Name = "txtPassword";
            txtPassword.RightToLeft = RightToLeft.No;
            txtPassword.Size = new Size(206, 24);
            txtPassword.TabIndex = 9;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.FromArgb(25, 118, 210);
            btnConnect.Cursor = Cursors.Hand;
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.FlatStyle = FlatStyle.Flat;
            btnConnect.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConnect.ForeColor = Color.White;
            btnConnect.Location = new Point(10, 232);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(318, 36);
            btnConnect.TabIndex = 10;
            btnConnect.Text = "🔗  اتصال";
            btnConnect.UseVisualStyleBackColor = false;
            btnConnect.Click += btnConnect_Click;
            // 
            // sep1
            // 
            sep1.ForeColor = Color.Gray;
            sep1.Location = new Point(10, 280);
            sep1.Name = "sep1";
            sep1.Size = new Size(318, 18);
            sep1.TabIndex = 11;
            sep1.Text = "────────── خيارات التوليد ──────────";
            sep1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblNs
            // 
            lblNs.Location = new Point(10, 306);
            lblNs.Name = "lblNs";
            lblNs.Size = new Size(95, 22);
            lblNs.TabIndex = 12;
            lblNs.Text = "Namespace:";
            lblNs.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtNamespace
            // 
            txtNamespace.Location = new Point(112, 304);
            txtNamespace.Name = "txtNamespace";
            txtNamespace.RightToLeft = RightToLeft.No;
            txtNamespace.Size = new Size(206, 24);
            txtNamespace.TabIndex = 13;
            // 
            // lblNsHint
            // 
            lblNsHint.Font = new Font("Segoe UI", 7.5F);
            lblNsHint.ForeColor = Color.Gray;
            lblNsHint.Location = new Point(112, 326);
            lblNsHint.Name = "lblNsHint";
            lblNsHint.Size = new Size(206, 16);
            lblNsHint.TabIndex = 14;
            lblNsHint.Text = "(اتركه فارغاً إذا لم تُرد Namespace)";
            // 
            // lblOut
            // 
            lblOut.Location = new Point(10, 350);
            lblOut.Name = "lblOut";
            lblOut.Size = new Size(95, 22);
            lblOut.TabIndex = 15;
            lblOut.Text = "مجلد الإخراج:";
            lblOut.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtOutput
            // 
            txtOutput.Location = new Point(112, 348);
            txtOutput.Name = "txtOutput";
            txtOutput.RightToLeft = RightToLeft.No;
            txtOutput.Size = new Size(170, 24);
            txtOutput.TabIndex = 16;
            // 
            // btnBrowse
            // 
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.Location = new Point(286, 346);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(32, 26);
            btnBrowse.TabIndex = 17;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // grpLang
            // 
            grpLang.Controls.Add(chkGenVB);
            grpLang.Controls.Add(chkGenCS);
            grpLang.Controls.Add(chkGenSP);
            grpLang.Location = new Point(10, 380);
            grpLang.Name = "grpLang";
            grpLang.Size = new Size(318, 92);
            grpLang.TabIndex = 18;
            grpLang.TabStop = false;
            grpLang.Text = "الكود المُولَّد";
            // 
            // chkGenVB
            // 
            chkGenVB.Checked = true;
            chkGenVB.CheckState = CheckState.Checked;
            chkGenVB.Location = new Point(12, 18);
            chkGenVB.Name = "chkGenVB";
            chkGenVB.RightToLeft = RightToLeft.Yes;
            chkGenVB.Size = new Size(290, 20);
            chkGenVB.TabIndex = 0;
            chkGenVB.Text = "VB.NET";
            // 
            // chkGenCS
            // 
            chkGenCS.Location = new Point(12, 40);
            chkGenCS.Name = "chkGenCS";
            chkGenCS.RightToLeft = RightToLeft.Yes;
            chkGenCS.Size = new Size(290, 20);
            chkGenCS.TabIndex = 1;
            chkGenCS.Text = "C#";
            // 
            // chkGenSP
            // 
            chkGenSP.Checked = true;
            chkGenSP.CheckState = CheckState.Checked;
            chkGenSP.Location = new Point(12, 62);
            chkGenSP.Name = "chkGenSP";
            chkGenSP.RightToLeft = RightToLeft.Yes;
            chkGenSP.Size = new Size(290, 20);
            chkGenSP.TabIndex = 2;
            chkGenSP.Text = "Stored Procedures  (Procedures.SQL)";
            // 
            // grpExtra
            // 
            grpExtra.Controls.Add(chkGenInit);
            grpExtra.Controls.Add(chkGenCustom);
            grpExtra.Controls.Add(chkRunSP);
            grpExtra.Location = new Point(10, 480);
            grpExtra.Name = "grpExtra";
            grpExtra.Size = new Size(318, 92);
            grpExtra.TabIndex = 19;
            grpExtra.TabStop = false;
            grpExtra.Text = "ملفات إضافية";
            // 
            // chkGenInit
            // 
            chkGenInit.Checked = true;
            chkGenInit.CheckState = CheckState.Checked;
            chkGenInit.Location = new Point(12, 18);
            chkGenInit.Name = "chkGenInit";
            chkGenInit.Size = new Size(290, 20);
            chkGenInit.TabIndex = 0;
            chkGenInit.Text = "إنشاء Universal + Interfaces";
            // 
            // chkGenCustom
            // 
            chkGenCustom.Location = new Point(12, 40);
            chkGenCustom.Name = "chkGenCustom";
            chkGenCustom.Size = new Size(290, 20);
            chkGenCustom.TabIndex = 1;
            chkGenCustom.Text = "إنشاء ملفات Custom (DataCustom/BusinessCustom)";
            // 
            // chkRunSP
            // 
            chkRunSP.Location = new Point(12, 62);
            chkRunSP.Name = "chkRunSP";
            chkRunSP.Size = new Size(290, 20);
            chkRunSP.TabIndex = 2;
            chkRunSP.Text = "تطبيق SP على قاعدة البيانات مباشرةً";
            // 
            // grpFE
            // 
            grpFE.Controls.Add(chkFalseErase);
            grpFE.Controls.Add(txtFalseEraseField);
            grpFE.Controls.Add(lblFEField);
            grpFE.Location = new Point(10, 580);
            grpFE.Name = "grpFE";
            grpFE.Size = new Size(318, 56);
            grpFE.TabIndex = 20;
            grpFE.TabStop = false;
            grpFE.Text = "False Erase (حذف منطقي)";
            // 
            // chkFalseErase
            // 
            chkFalseErase.Location = new Point(248, 27);
            chkFalseErase.Name = "chkFalseErase";
            chkFalseErase.Size = new Size(60, 20);
            chkFalseErase.TabIndex = 0;
            chkFalseErase.Text = "تفعيل";
            chkFalseErase.CheckedChanged += chkFalseErase_CheckedChanged;
            // 
            // txtFalseEraseField
            // 
            txtFalseEraseField.Enabled = false;
            txtFalseEraseField.Location = new Point(12, 23);
            txtFalseEraseField.Name = "txtFalseEraseField";
            txtFalseEraseField.RightToLeft = RightToLeft.No;
            txtFalseEraseField.Size = new Size(182, 24);
            txtFalseEraseField.TabIndex = 2;
            txtFalseEraseField.Text = "IsDeleted";
            // 
            // lblFEField
            // 
            lblFEField.Location = new Point(200, 27);
            lblFEField.Name = "lblFEField";
            lblFEField.Size = new Size(44, 20);
            lblFEField.TabIndex = 1;
            lblFEField.Text = "الحقل:";
            lblFEField.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkSuppressComments
            // 
            chkSuppressComments.Location = new Point(10, 646);
            chkSuppressComments.Name = "chkSuppressComments";
            chkSuppressComments.Size = new Size(318, 28);
            chkSuppressComments.TabIndex = 21;
            chkSuppressComments.Text = "حذف التعليقات من الكود";
            // 
            // pnlRight
            // 
            pnlRight.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlRight.BackColor = Color.White;
            pnlRight.BorderStyle = BorderStyle.FixedSingle;
            pnlRight.Controls.Add(lblTablesTitle);
            pnlRight.Controls.Add(btnSelectAll);
            pnlRight.Controls.Add(btnDeselectAll);
            pnlRight.Controls.Add(clbTables);
            pnlRight.Controls.Add(btnCreateLayers);
            pnlRight.Location = new Point(360, 10);
            pnlRight.Name = "pnlRight";
            pnlRight.Padding = new Padding(10);
            pnlRight.Size = new Size(630, 700);
            pnlRight.TabIndex = 1;
            pnlRight.SizeChanged += pnlRight_SizeChanged;
            // 
            // lblTablesTitle
            // 
            lblTablesTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTablesTitle.ForeColor = Color.FromArgb(25, 118, 210);
            lblTablesTitle.Location = new Point(10, 8);
            lblTablesTitle.Name = "lblTablesTitle";
            lblTablesTitle.Size = new Size(600, 28);
            lblTablesTitle.TabIndex = 0;
            lblTablesTitle.Text = "📋  الجداول والـ Views";
            lblTablesTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnSelectAll
            // 
            btnSelectAll.BackColor = Color.FromArgb(232, 245, 232);
            btnSelectAll.Cursor = Cursors.Hand;
            btnSelectAll.FlatStyle = FlatStyle.Flat;
            btnSelectAll.Location = new Point(10, 42);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(110, 28);
            btnSelectAll.TabIndex = 1;
            btnSelectAll.Text = "تحديد الكل";
            btnSelectAll.UseVisualStyleBackColor = false;
            btnSelectAll.Click += btnSelectAll_Click;
            // 
            // btnDeselectAll
            // 
            btnDeselectAll.BackColor = Color.FromArgb(255, 235, 235);
            btnDeselectAll.Cursor = Cursors.Hand;
            btnDeselectAll.FlatStyle = FlatStyle.Flat;
            btnDeselectAll.Location = new Point(130, 42);
            btnDeselectAll.Name = "btnDeselectAll";
            btnDeselectAll.Size = new Size(110, 28);
            btnDeselectAll.TabIndex = 2;
            btnDeselectAll.Text = "إلغاء الكل";
            btnDeselectAll.UseVisualStyleBackColor = false;
            btnDeselectAll.Click += btnDeselectAll_Click;
            // 
            // clbTables
            // 
            clbTables.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            clbTables.BorderStyle = BorderStyle.FixedSingle;
            clbTables.CheckOnClick = true;
            clbTables.Font = new Font("Consolas", 9.5F);
            clbTables.Location = new Point(10, 78);
            clbTables.Name = "clbTables";
            clbTables.Size = new Size(600, 529);
            clbTables.TabIndex = 3;
            // 
            // btnCreateLayers
            // 
            btnCreateLayers.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnCreateLayers.BackColor = Color.FromArgb(76, 175, 80);
            btnCreateLayers.Cursor = Cursors.Hand;
            btnCreateLayers.Enabled = false;
            btnCreateLayers.FlatAppearance.BorderSize = 0;
            btnCreateLayers.FlatStyle = FlatStyle.Flat;
            btnCreateLayers.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnCreateLayers.ForeColor = Color.White;
            btnCreateLayers.Location = new Point(10, 616);
            btnCreateLayers.Name = "btnCreateLayers";
            btnCreateLayers.Size = new Size(600, 46);
            btnCreateLayers.TabIndex = 4;
            btnCreateLayers.Text = "🚀  إنشاء الكود";
            btnCreateLayers.UseVisualStyleBackColor = false;
            btnCreateLayers.Click += btnCreateLayers_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Location = new Point(10, 720);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(980, 24);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "قم بالاتصال بقاعدة البيانات أولاً...";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 242, 245);
            ClientSize = new Size(1004, 755);
            Controls.Add(pnlLeft);
            Controls.Add(pnlRight);
            Controls.Add(lblStatus);
            Font = new Font("Segoe UI", 9.5F);
            MinimumSize = new Size(900, 700);
            Name = "MainForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LayerGen X1  –  VB.NET 4 / ASP.NET 4 Code Generator";
            SizeChanged += MainForm_SizeChanged;
            pnlLeft.ResumeLayout(false);
            pnlLeft.PerformLayout();
            grpAuth.ResumeLayout(false);
            grpLang.ResumeLayout(false);
            grpExtra.ResumeLayout(false);
            grpFE.ResumeLayout(false);
            grpFE.PerformLayout();
            pnlRight.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // ── Control declarations ─────────────────────────────────────────────────
        private System.Windows.Forms.Panel         pnlLeft             = null!;
        private System.Windows.Forms.Panel         pnlRight            = null!;
        private System.Windows.Forms.Label         lblTitle            = null!;
        private System.Windows.Forms.Label         lblServer           = null!;
        private System.Windows.Forms.TextBox       txtServer           = null!;
        private System.Windows.Forms.Label         lblDb               = null!;
        private System.Windows.Forms.TextBox       txtDatabase         = null!;
        private System.Windows.Forms.GroupBox      grpAuth             = null!;
        private System.Windows.Forms.RadioButton   rbWindows           = null!;
        private System.Windows.Forms.RadioButton   rbSql               = null!;
        private System.Windows.Forms.Label         lblUser             = null!;
        private System.Windows.Forms.TextBox       txtUser             = null!;
        private System.Windows.Forms.Label         lblPassword         = null!;
        private System.Windows.Forms.TextBox       txtPassword         = null!;
        private System.Windows.Forms.Button        btnConnect          = null!;
        private System.Windows.Forms.Label         sep1                = null!;
        private System.Windows.Forms.Label         lblNs               = null!;
        private System.Windows.Forms.TextBox       txtNamespace        = null!;
        private System.Windows.Forms.Label         lblNsHint           = null!;
        private System.Windows.Forms.Label         lblOut              = null!;
        private System.Windows.Forms.TextBox       txtOutput           = null!;
        private System.Windows.Forms.Button        btnBrowse           = null!;
        private System.Windows.Forms.GroupBox      grpLang             = null!;
        private System.Windows.Forms.CheckBox      chkGenCS            = null!;
        private System.Windows.Forms.CheckBox      chkGenVB            = null!;
        private System.Windows.Forms.CheckBox      chkGenSP            = null!;
        private System.Windows.Forms.GroupBox      grpExtra            = null!;
        private System.Windows.Forms.CheckBox      chkGenCustom        = null!;
        private System.Windows.Forms.CheckBox      chkGenInit          = null!;
        private System.Windows.Forms.CheckBox      chkRunSP            = null!;
        private System.Windows.Forms.GroupBox      grpFE               = null!;
        private System.Windows.Forms.CheckBox      chkFalseErase       = null!;
        private System.Windows.Forms.Label         lblFEField          = null!;
        private System.Windows.Forms.TextBox       txtFalseEraseField  = null!;
        private System.Windows.Forms.CheckBox      chkSuppressComments = null!;
        private System.Windows.Forms.CheckedListBox clbTables          = null!;
        private System.Windows.Forms.Button        btnSelectAll        = null!;
        private System.Windows.Forms.Button        btnDeselectAll      = null!;
        private System.Windows.Forms.Button        btnCreateLayers     = null!;
        private System.Windows.Forms.Label         lblStatus           = null!;
        private System.Windows.Forms.Label         lblTablesTitle      = null!;
    }
}
