namespace Forte7000C.Reports
{
    partial class CrystalReport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.gbFields = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.CBShiftName = new System.Windows.Forms.CheckBox();
            this.CBWtMes = new System.Windows.Forms.CheckBox();
            this.CBMoistMes = new System.Windows.Forms.CheckBox();
            this.CBSpareSngFld2 = new System.Windows.Forms.CheckBox();
            this.CBSpareSngFld1 = new System.Windows.Forms.CheckBox();
            this.CBSerialNumber = new System.Windows.Forms.CheckBox();
            this.CBNetWeight = new System.Windows.Forms.CheckBox();
            this.CBMoisture = new System.Windows.Forms.CheckBox();
            this.CBTareWeight = new System.Windows.Forms.CheckBox();
            this.CBTimeComplete = new System.Windows.Forms.CheckBox();
            this.CBTimeStart = new System.Windows.Forms.CheckBox();
            this.CBDownCount = new System.Windows.Forms.CheckBox();
            this.CBUpCount = new System.Windows.Forms.CheckBox();
            this.CBForte2 = new System.Windows.Forms.CheckBox();
            this.CBForte1 = new System.Windows.Forms.CheckBox();
            this.CBForte = new System.Windows.Forms.CheckBox();
            this.CBWeight = new System.Windows.Forms.CheckBox();
            this.CBCalibrationName = new System.Windows.Forms.CheckBox();
            this.CBStockName = new System.Windows.Forms.CheckBox();
            this.crystalReportViewer2 = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.CrtDayReport1 = new Forte7000C.Reports.CrtDayReport();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.gbFields.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.gbFields);
            this.splitContainer1.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.SplitContainer1_Panel1_Paint);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.crystalReportViewer2);
            this.splitContainer1.Size = new System.Drawing.Size(1123, 775);
            this.splitContainer1.SplitterDistance = 202;
            this.splitContainer1.TabIndex = 1;
            // 
            // gbFields
            // 
            this.gbFields.Controls.Add(this.button1);
            this.gbFields.Controls.Add(this.CBShiftName);
            this.gbFields.Controls.Add(this.CBWtMes);
            this.gbFields.Controls.Add(this.CBMoistMes);
            this.gbFields.Controls.Add(this.CBSpareSngFld2);
            this.gbFields.Controls.Add(this.CBSpareSngFld1);
            this.gbFields.Controls.Add(this.CBSerialNumber);
            this.gbFields.Controls.Add(this.CBNetWeight);
            this.gbFields.Controls.Add(this.CBMoisture);
            this.gbFields.Controls.Add(this.CBTareWeight);
            this.gbFields.Controls.Add(this.CBTimeComplete);
            this.gbFields.Controls.Add(this.CBTimeStart);
            this.gbFields.Controls.Add(this.CBDownCount);
            this.gbFields.Controls.Add(this.CBUpCount);
            this.gbFields.Controls.Add(this.CBForte2);
            this.gbFields.Controls.Add(this.CBForte1);
            this.gbFields.Controls.Add(this.CBForte);
            this.gbFields.Controls.Add(this.CBWeight);
            this.gbFields.Controls.Add(this.CBCalibrationName);
            this.gbFields.Controls.Add(this.CBStockName);
            this.gbFields.Location = new System.Drawing.Point(22, 83);
            this.gbFields.Name = "gbFields";
            this.gbFields.Size = new System.Drawing.Size(159, 599);
            this.gbFields.TabIndex = 0;
            this.gbFields.TabStop = false;
            this.gbFields.Text = "Custom Fields";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(50, 555);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 22;
            this.button1.Text = "View Report";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // CBShiftName
            // 
            this.CBShiftName.AutoSize = true;
            this.CBShiftName.Location = new System.Drawing.Point(36, 118);
            this.CBShiftName.Name = "CBShiftName";
            this.CBShiftName.Size = new System.Drawing.Size(75, 17);
            this.CBShiftName.TabIndex = 21;
            this.CBShiftName.Text = "ShiftName";
            this.CBShiftName.UseVisualStyleBackColor = true;
            // 
            // CBWtMes
            // 
            this.CBWtMes.AutoSize = true;
            this.CBWtMes.Enabled = false;
            this.CBWtMes.Location = new System.Drawing.Point(36, 251);
            this.CBWtMes.Name = "CBWtMes";
            this.CBWtMes.Size = new System.Drawing.Size(60, 17);
            this.CBWtMes.TabIndex = 20;
            this.CBWtMes.Text = "WtMes";
            this.CBWtMes.UseVisualStyleBackColor = true;
            // 
            // CBMoistMes
            // 
            this.CBMoistMes.AutoSize = true;
            this.CBMoistMes.Enabled = false;
            this.CBMoistMes.Location = new System.Drawing.Point(36, 297);
            this.CBMoistMes.Name = "CBMoistMes";
            this.CBMoistMes.Size = new System.Drawing.Size(71, 17);
            this.CBMoistMes.TabIndex = 19;
            this.CBMoistMes.Text = "MoistMes";
            this.CBMoistMes.UseVisualStyleBackColor = true;
            // 
            // CBSpareSngFld2
            // 
            this.CBSpareSngFld2.AutoSize = true;
            this.CBSpareSngFld2.Enabled = false;
            this.CBSpareSngFld2.Location = new System.Drawing.Point(36, 498);
            this.CBSpareSngFld2.Name = "CBSpareSngFld2";
            this.CBSpareSngFld2.Size = new System.Drawing.Size(93, 17);
            this.CBSpareSngFld2.TabIndex = 18;
            this.CBSpareSngFld2.Text = "SpareSngFld2";
            this.CBSpareSngFld2.UseVisualStyleBackColor = true;
            // 
            // CBSpareSngFld1
            // 
            this.CBSpareSngFld1.AutoSize = true;
            this.CBSpareSngFld1.Enabled = false;
            this.CBSpareSngFld1.Location = new System.Drawing.Point(36, 476);
            this.CBSpareSngFld1.Name = "CBSpareSngFld1";
            this.CBSpareSngFld1.Size = new System.Drawing.Size(93, 17);
            this.CBSpareSngFld1.TabIndex = 17;
            this.CBSpareSngFld1.Text = "SpareSngFld1";
            this.CBSpareSngFld1.UseVisualStyleBackColor = true;
            // 
            // CBSerialNumber
            // 
            this.CBSerialNumber.AutoSize = true;
            this.CBSerialNumber.Location = new System.Drawing.Point(36, 56);
            this.CBSerialNumber.Name = "CBSerialNumber";
            this.CBSerialNumber.Size = new System.Drawing.Size(89, 17);
            this.CBSerialNumber.TabIndex = 16;
            this.CBSerialNumber.Text = "SerialNumber";
            this.CBSerialNumber.UseVisualStyleBackColor = true;
            // 
            // CBNetWeight
            // 
            this.CBNetWeight.AutoSize = true;
            this.CBNetWeight.Enabled = false;
            this.CBNetWeight.Location = new System.Drawing.Point(36, 182);
            this.CBNetWeight.Name = "CBNetWeight";
            this.CBNetWeight.Size = new System.Drawing.Size(77, 17);
            this.CBNetWeight.TabIndex = 15;
            this.CBNetWeight.Text = "NetWeight";
            this.CBNetWeight.UseVisualStyleBackColor = true;
            // 
            // CBMoisture
            // 
            this.CBMoisture.AutoSize = true;
            this.CBMoisture.Location = new System.Drawing.Point(36, 274);
            this.CBMoisture.Name = "CBMoisture";
            this.CBMoisture.Size = new System.Drawing.Size(66, 17);
            this.CBMoisture.TabIndex = 14;
            this.CBMoisture.Text = "Moisture";
            this.CBMoisture.UseVisualStyleBackColor = true;
            // 
            // CBTareWeight
            // 
            this.CBTareWeight.AutoSize = true;
            this.CBTareWeight.Enabled = false;
            this.CBTareWeight.Location = new System.Drawing.Point(36, 205);
            this.CBTareWeight.Name = "CBTareWeight";
            this.CBTareWeight.Size = new System.Drawing.Size(82, 17);
            this.CBTareWeight.TabIndex = 13;
            this.CBTareWeight.Text = "TareWeight";
            this.CBTareWeight.UseVisualStyleBackColor = true;
            // 
            // CBTimeComplete
            // 
            this.CBTimeComplete.AutoSize = true;
            this.CBTimeComplete.Location = new System.Drawing.Point(36, 95);
            this.CBTimeComplete.Name = "CBTimeComplete";
            this.CBTimeComplete.Size = new System.Drawing.Size(93, 17);
            this.CBTimeComplete.TabIndex = 11;
            this.CBTimeComplete.Text = "TimeComplete";
            this.CBTimeComplete.UseVisualStyleBackColor = true;
            // 
            // CBTimeStart
            // 
            this.CBTimeStart.AutoSize = true;
            this.CBTimeStart.Enabled = false;
            this.CBTimeStart.Location = new System.Drawing.Point(36, 75);
            this.CBTimeStart.Name = "CBTimeStart";
            this.CBTimeStart.Size = new System.Drawing.Size(71, 17);
            this.CBTimeStart.TabIndex = 10;
            this.CBTimeStart.Text = "TimeStart";
            this.CBTimeStart.UseVisualStyleBackColor = true;
            // 
            // CBDownCount
            // 
            this.CBDownCount.AutoSize = true;
            this.CBDownCount.Enabled = false;
            this.CBDownCount.Location = new System.Drawing.Point(36, 434);
            this.CBDownCount.Name = "CBDownCount";
            this.CBDownCount.Size = new System.Drawing.Size(82, 17);
            this.CBDownCount.TabIndex = 9;
            this.CBDownCount.Text = "DownCount";
            this.CBDownCount.UseVisualStyleBackColor = true;
            // 
            // CBUpCount
            // 
            this.CBUpCount.AutoSize = true;
            this.CBUpCount.Enabled = false;
            this.CBUpCount.Location = new System.Drawing.Point(36, 455);
            this.CBUpCount.Name = "CBUpCount";
            this.CBUpCount.Size = new System.Drawing.Size(68, 17);
            this.CBUpCount.TabIndex = 8;
            this.CBUpCount.Text = "UpCount";
            this.CBUpCount.UseVisualStyleBackColor = true;
            // 
            // CBForte2
            // 
            this.CBForte2.AutoSize = true;
            this.CBForte2.Enabled = false;
            this.CBForte2.Location = new System.Drawing.Point(36, 414);
            this.CBForte2.Name = "CBForte2";
            this.CBForte2.Size = new System.Drawing.Size(56, 17);
            this.CBForte2.TabIndex = 7;
            this.CBForte2.Text = "Forte2";
            this.CBForte2.UseVisualStyleBackColor = true;
            // 
            // CBForte1
            // 
            this.CBForte1.AutoSize = true;
            this.CBForte1.Enabled = false;
            this.CBForte1.Location = new System.Drawing.Point(36, 393);
            this.CBForte1.Name = "CBForte1";
            this.CBForte1.Size = new System.Drawing.Size(56, 17);
            this.CBForte1.TabIndex = 6;
            this.CBForte1.Text = "Forte1";
            this.CBForte1.UseVisualStyleBackColor = true;
            // 
            // CBForte
            // 
            this.CBForte.AutoSize = true;
            this.CBForte.Enabled = false;
            this.CBForte.Location = new System.Drawing.Point(36, 373);
            this.CBForte.Name = "CBForte";
            this.CBForte.Size = new System.Drawing.Size(50, 17);
            this.CBForte.TabIndex = 5;
            this.CBForte.Text = "Forte";
            this.CBForte.UseVisualStyleBackColor = true;
            // 
            // CBWeight
            // 
            this.CBWeight.AutoSize = true;
            this.CBWeight.Location = new System.Drawing.Point(36, 228);
            this.CBWeight.Name = "CBWeight";
            this.CBWeight.Size = new System.Drawing.Size(60, 17);
            this.CBWeight.TabIndex = 4;
            this.CBWeight.Text = "Weight";
            this.CBWeight.UseVisualStyleBackColor = true;
            // 
            // CBCalibrationName
            // 
            this.CBCalibrationName.AutoSize = true;
            this.CBCalibrationName.Location = new System.Drawing.Point(36, 161);
            this.CBCalibrationName.Name = "CBCalibrationName";
            this.CBCalibrationName.Size = new System.Drawing.Size(103, 17);
            this.CBCalibrationName.TabIndex = 3;
            this.CBCalibrationName.Text = "CalibrationName";
            this.CBCalibrationName.UseVisualStyleBackColor = true;
            // 
            // CBStockName
            // 
            this.CBStockName.AutoSize = true;
            this.CBStockName.Location = new System.Drawing.Point(36, 139);
            this.CBStockName.Name = "CBStockName";
            this.CBStockName.Size = new System.Drawing.Size(82, 17);
            this.CBStockName.TabIndex = 2;
            this.CBStockName.Text = "StockName";
            this.CBStockName.UseVisualStyleBackColor = true;
            // 
            // crystalReportViewer2
            // 
            this.crystalReportViewer2.ActiveViewIndex = 0;
            this.crystalReportViewer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer2.Cursor = System.Windows.Forms.Cursors.Default;
            this.crystalReportViewer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crystalReportViewer2.Location = new System.Drawing.Point(0, 0);
            this.crystalReportViewer2.Name = "crystalReportViewer2";
            this.crystalReportViewer2.ReportSource = this.CrtDayReport1;
            this.crystalReportViewer2.Size = new System.Drawing.Size(917, 775);
            this.crystalReportViewer2.TabIndex = 0;
            this.crystalReportViewer2.ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.None;
            this.crystalReportViewer2.Load += new System.EventHandler(this.CrystalReportViewer2_Load);
            // 
            // CrtDayReport1
            // 
            this.CrtDayReport1.InitReport += new System.EventHandler(this.CrtDayReport1_InitReport);
            // 
            // CrystalReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1123, 775);
            this.Controls.Add(this.splitContainer1);
            this.Name = "CrystalReport";
            this.Text = "CrystalReport";
            this.Load += new System.EventHandler(this.CrystalReport_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.gbFields.ResumeLayout(false);
            this.gbFields.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private CrystalDecisions.Windows.Forms.CrystalReportViewer crystalReportViewer2;
        private CrtDayReport CrtDayReport1;
        private System.Windows.Forms.GroupBox gbFields;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox CBShiftName;
        private System.Windows.Forms.CheckBox CBWtMes;
        private System.Windows.Forms.CheckBox CBMoistMes;
        private System.Windows.Forms.CheckBox CBSpareSngFld2;
        private System.Windows.Forms.CheckBox CBSpareSngFld1;
        private System.Windows.Forms.CheckBox CBSerialNumber;
        private System.Windows.Forms.CheckBox CBNetWeight;
        private System.Windows.Forms.CheckBox CBMoisture;
        private System.Windows.Forms.CheckBox CBTareWeight;
        private System.Windows.Forms.CheckBox CBTimeComplete;
        private System.Windows.Forms.CheckBox CBTimeStart;
        private System.Windows.Forms.CheckBox CBDownCount;
        private System.Windows.Forms.CheckBox CBUpCount;
        private System.Windows.Forms.CheckBox CBForte2;
        private System.Windows.Forms.CheckBox CBForte1;
        private System.Windows.Forms.CheckBox CBForte;
        private System.Windows.Forms.CheckBox CBWeight;
        private System.Windows.Forms.CheckBox CBCalibrationName;
        private System.Windows.Forms.CheckBox CBStockName;
    }
}