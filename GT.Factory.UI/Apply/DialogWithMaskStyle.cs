using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.Apply
{

    public static class DialogWithMaskStyle
    {
        public static DialogResult ShowDialogWithMask(Form dialog, Form parentForm)
        {
            using (Form mask = new Form())
            {
                mask.StartPosition = FormStartPosition.Manual;
                mask.FormBorderStyle = FormBorderStyle.None;
                mask.BackColor = Color.Black;
                mask.Opacity = 0.6; // 60%透明黑
                mask.ShowInTaskbar = false;
                mask.Size = parentForm.Size;
                mask.Location = parentForm.Location;

                mask.Show(parentForm);
                dialog.StartPosition = FormStartPosition.CenterParent;
                DialogResult result = dialog.ShowDialog(mask);
                mask.Close();

                return result;
            }
        }
    }
}
