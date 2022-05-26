using AppForJoystickCameraAndSerial.Controllers;
using Com.Okmer.GameController;

using OpenCvSharp;
using OpenCvSharp.Extensions;


namespace AppForJoystickCameraAndSerial
{
    public partial class Form1 : Form
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly JoysticksController joysticksController;
        private readonly CamerasController camerasController;

        public Form1()
        {
            InitializeComponent();
            Joystick_Lable.ForeColor = Color.Red;
            cancellationTokenSource = new CancellationTokenSource();
            joysticksController = new JoysticksController(JoystickInfoTxtBox, Joystick_Lable);
            camerasController = new CamerasController(cancellationTokenSource.Token, MainCameraPictureBox, MinorPictureBox, Camera1_Lable, Camera2_Lable);
        }

        private void Exit_Btn_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void SearchCheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
                camerasController.Start();
            else
                camerasController.Stop();
        }
    }
}