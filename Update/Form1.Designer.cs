
namespace Update
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pro_bar = new System.Windows.Forms.ProgressBar();
            this.lb_bytes = new System.Windows.Forms.Label();
            this.lb_tip = new System.Windows.Forms.Label();
            this.lb_rate = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pro_bar
            // 
            this.pro_bar.Location = new System.Drawing.Point(39, 24);
            this.pro_bar.Name = "pro_bar";
            this.pro_bar.Size = new System.Drawing.Size(353, 23);
            this.pro_bar.TabIndex = 0;
            // 
            // lb_bytes
            // 
            this.lb_bytes.AutoSize = true;
            this.lb_bytes.Location = new System.Drawing.Point(354, 9);
            this.lb_bytes.Name = "lb_bytes";
            this.lb_bytes.Size = new System.Drawing.Size(29, 12);
            this.lb_bytes.TabIndex = 2;
            this.lb_bytes.Text = "kb/s";
            // 
            // lb_tip
            // 
            this.lb_tip.AutoSize = true;
            this.lb_tip.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_tip.ForeColor = System.Drawing.Color.DarkRed;
            this.lb_tip.Location = new System.Drawing.Point(41, 56);
            this.lb_tip.Name = "lb_tip";
            this.lb_tip.Size = new System.Drawing.Size(53, 12);
            this.lb_tip.TabIndex = 3;
            this.lb_tip.Text = "更新完成";
            // 
            // lb_rate
            // 
            this.lb_rate.AutoSize = true;
            this.lb_rate.Location = new System.Drawing.Point(42, 9);
            this.lb_rate.Name = "lb_rate";
            this.lb_rate.Size = new System.Drawing.Size(17, 12);
            this.lb_rate.TabIndex = 4;
            this.lb_rate.Text = "0%";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 88);
            this.Controls.Add(this.lb_rate);
            this.Controls.Add(this.lb_tip);
            this.Controls.Add(this.lb_bytes);
            this.Controls.Add(this.pro_bar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "游戏更新";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pro_bar;
        private System.Windows.Forms.Label lb_bytes;
        private System.Windows.Forms.Label lb_tip;
        private System.Windows.Forms.Label lb_rate;
    }
}

