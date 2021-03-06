using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace eContract.DDP.UI
{
    public class MyDataGridViewNoRow : System.Windows.Forms.DataGridView
    {
     

       protected override void OnCellPainting(System.Windows.Forms.DataGridViewCellPaintingEventArgs e)
       {
           Color mLinearColor1 = Color.FromArgb(247, 251, 254);
           Color mLinearColor2 = Color.FromArgb(213, 231, 255);
           Color mGridColor = Color.FromArgb(120, 147, 191);  //网格线的颜色
           Color mHasFocusedColor = Color.DarkCyan;                          //控件的焦点框颜色 


           Rectangle Rect = new Rectangle(e.CellBounds.X - 1, e.CellBounds.Y, e.CellBounds.Width, e.CellBounds.Height - 1);
           LinearGradientBrush LinearGradientBrushs = new LinearGradientBrush(Rect, mLinearColor1, mLinearColor2, LinearGradientMode.Vertical);

           try
           {
               if (e.RowIndex == -1 ||e.ColumnIndex==-1)
               {
                   e.Graphics.FillRectangle(LinearGradientBrushs, Rect);
                   e.Graphics.DrawRectangle(new Pen(mGridColor), Rect);
                   e.PaintContent(e.CellBounds);
                   e.Handled = true;
               }
              
           }

           catch
           { }
           finally
           {
               if (LinearGradientBrushs != null)
               {
                   LinearGradientBrushs.Dispose();
               }

           }

           /*不显示焦点颜色*/
           //this.DefaultCellStyle.SelectionBackColor = Color.Transparent ;
           //this.DefaultCellStyle.SelectionForeColor  = Color.Black ;


           base.OnCellPainting(e);
       }
 
       private void InitializeComponent()
       {
           System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
           ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
           this.SuspendLayout();
           // 
           // MyDataGridView
           // 
           this.AllowUserToResizeRows = false;
           this.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
           this.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
           dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
           dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
           dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
           dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
           dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
           dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
           dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
           this.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
           this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
           this.RowHeadersWidth = 40;
           this.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
           this.RowTemplate.Height = 23;
           ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
           this.ResumeLayout(false);

       }
    }
}
