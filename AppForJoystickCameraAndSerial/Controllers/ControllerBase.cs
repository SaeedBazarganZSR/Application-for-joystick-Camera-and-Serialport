﻿namespace AppForJoystickCameraAndSerial.Controllers
{
    public abstract class ControllerBase
    {
        private static PaintEventArgs e;

        protected static void ChangeTextBox(TextBox textBox, string txt)
        {
            textBox.BeginInvoke((MethodInvoker)delegate ()
            {
                textBox.Text = txt;
            });
        }

        public void ChangePictureBox(PictureBox pictureBox, Bitmap image)
        {
            if (pictureBox.Image != null)
                pictureBox.Image.Dispose();
            pictureBox.Image = image;
        }

        protected static void ChangeLabel(Label label, Color color)
        {
            label.BeginInvoke((MethodInvoker)delegate ()
            {
                label.ForeColor = color;
            });
        }

        protected static void HidePictureBox(PictureBox box)
        {
            box.BeginInvoke((MethodInvoker)delegate ()
            {
                box.Hide();
            });
        }
    }
}
