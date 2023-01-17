using CommonTool;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace AutoFrame
{
    partial class Form_MsgTest
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
            bool flag = disposing && this.components != null;
            if (flag)
            {
                this.components.Dispose();
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
            this.button_yes = new System.Windows.Forms.Button();
            this.button_no = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.groupBox_Warn = new System.Windows.Forms.GroupBox();
            this.richTextBox_warn = new System.Windows.Forms.RichTextBox();
            this.groupBox_Tips = new System.Windows.Forms.GroupBox();
            this.checkedListBox_Tips = new System.Windows.Forms.CheckedListBox();
            this.groupBox_Warn.SuspendLayout();
            this.groupBox_Tips.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_yes
            // 
            this.button_yes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.button_yes.Location = new System.Drawing.Point(33, 323);
            this.button_yes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_yes.Name = "button_yes";
            this.button_yes.Size = new System.Drawing.Size(109, 60);
            this.button_yes.TabIndex = 0;
            this.button_yes.Text = "确定";
            this.button_yes.UseVisualStyleBackColor = true;
            this.button_yes.Click += new System.EventHandler(this.button_yes_Click);
            // 
            // button_no
            // 
            this.button_no.DialogResult = System.Windows.Forms.DialogResult.No;
            this.button_no.Location = new System.Drawing.Point(161, 323);
            this.button_no.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_no.Name = "button_no";
            this.button_no.Size = new System.Drawing.Size(109, 60);
            this.button_no.TabIndex = 1;
            this.button_no.Text = "停止";
            this.button_no.UseVisualStyleBackColor = true;
            this.button_no.Click += new System.EventHandler(this.button_no_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(290, 323);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(109, 60);
            this.button_cancel.TabIndex = 1;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // groupBox_Warn
            // 
            this.groupBox_Warn.Controls.Add(this.richTextBox_warn);
            this.groupBox_Warn.Location = new System.Drawing.Point(15, 12);
            this.groupBox_Warn.Name = "groupBox_Warn";
            this.groupBox_Warn.Size = new System.Drawing.Size(448, 148);
            this.groupBox_Warn.TabIndex = 5;
            this.groupBox_Warn.TabStop = false;
            this.groupBox_Warn.Text = "警告";
            // 
            // richTextBox_warn
            // 
            this.richTextBox_warn.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox_warn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_warn.Font = new System.Drawing.Font("宋体", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richTextBox_warn.Location = new System.Drawing.Point(3, 27);
            this.richTextBox_warn.Name = "richTextBox_warn";
            this.richTextBox_warn.ReadOnly = true;
            this.richTextBox_warn.Size = new System.Drawing.Size(442, 118);
            this.richTextBox_warn.TabIndex = 0;
            this.richTextBox_warn.Text = "";
            // 
            // groupBox_Tips
            // 
            this.groupBox_Tips.Controls.Add(this.checkedListBox_Tips);
            this.groupBox_Tips.Location = new System.Drawing.Point(15, 166);
            this.groupBox_Tips.Name = "groupBox_Tips";
            this.groupBox_Tips.Size = new System.Drawing.Size(448, 150);
            this.groupBox_Tips.TabIndex = 6;
            this.groupBox_Tips.TabStop = false;
            this.groupBox_Tips.Text = "Tips";
            // 
            // checkedListBox_Tips
            // 
            this.checkedListBox_Tips.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBox_Tips.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBox_Tips.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBox_Tips.FormattingEnabled = true;
            this.checkedListBox_Tips.HorizontalScrollbar = true;
            this.checkedListBox_Tips.Location = new System.Drawing.Point(3, 27);
            this.checkedListBox_Tips.Name = "checkedListBox_Tips";
            this.checkedListBox_Tips.Size = new System.Drawing.Size(442, 120);
            this.checkedListBox_Tips.TabIndex = 0;
            // 
            // Form_MsgTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(476, 400);
            this.Controls.Add(this.groupBox_Tips);
            this.Controls.Add(this.groupBox_Warn);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_no);
            this.Controls.Add(this.button_yes);
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_MsgTest";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "消息提示";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_MsgTest_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Message_FormClosed);
            this.Load += new System.EventHandler(this.Form_Message_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_Message_KeyDown);
            this.groupBox_Warn.ResumeLayout(false);
            this.groupBox_Tips.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// Required designer variable.
        /// </summary>
       // private IContainer components = null;

        private Button button_yes;

        private Button button_no;

        private Button button_cancel;
        private GroupBox groupBox_Warn;
        private GroupBox groupBox_Tips;
        private CheckedListBox checkedListBox_Tips;
        private RichTextBox richTextBox_warn;
        //public Timer timer1;
    }
}