﻿using AppForJoystickCameraAndSerial.Models;
using System.IO.Ports;

namespace AppForJoystickCameraAndSerial.Controllers
{
    public class SerialController : ControllerBase
    {
        SerialPacketHandler Handler = new SerialPacketHandler();

        public SerialPort[] _SerialPort { get; private set; }
        public SerialPortSetting[] Settings { get; set; }
        public bool Open { get; private set; } = false;
        public bool Disposed { get; private set; } = false;
        const Parity ParityBit = Parity.None;
        const StopBits StopBit = StopBits.One;
        int Baudrate, DataBit;
        string PortNumber;

        private readonly ComboBox _Com_ComboBox, _Baud_ComboBox, _DataBits_ComboBox;
        private readonly ComboBox _Com_ComboBox2, _Baud_ComboBox2, _DataBits_ComboBox2;
        private readonly TextBox _SerialMonitoring_TextBox;
        private readonly TextBox _Fov_TextBox, _AzError_TextBox, _EiError_TextBox, _Ax_TextBox, _Ay_TextBox, _Az_TextBox;
        private readonly PictureBox _Serial1Status, _Serial2Status;
        private readonly Button _openPortBtn;

        private readonly CancellationToken _cancellationToken;
        private readonly Task[] serialPortTasks;
        private readonly bool[] isRunning;
        private readonly bool[] recording;

        int DataInBuffer_Size = 200;
        byte[] Data_Rx;
        int Data_Counter = 0;
        int serialportIndex = 0;


        public string RecordingDirectory { get; set; }

        public SerialController(CancellationToken cancellationToken, PictureBox serial1Status, PictureBox serial2Status, Button openPortBtn,
            TextBox fov_TextBox, TextBox azError_TextBox, TextBox eiError_TextBox, TextBox ax_TextBox, TextBox ay_TextBox, TextBox az_TextBox)
        {
            _SerialPort = new SerialPort[2];
            Settings = new SerialPortSetting[2]
            {
                new SerialPortSetting{PortNumber = "COM3",Baudrate = 9600, DataBit = 8},
                new SerialPortSetting{PortNumber = "COM5",Baudrate = 115200, DataBit = 8}
            };

            _Serial1Status = serial1Status; _Serial2Status = serial2Status;
            _openPortBtn = openPortBtn;

            _cancellationToken = cancellationToken;
            serialPortTasks = new Task[2];
            isRunning = new bool[2];
            recording = new bool[2];
            _Fov_TextBox = fov_TextBox;
            _AzError_TextBox = azError_TextBox;
            _EiError_TextBox = eiError_TextBox;
            _Ax_TextBox = ax_TextBox;
            _Ay_TextBox = ay_TextBox;
            _Az_TextBox = az_TextBox;

            Data_Rx = new byte[DataInBuffer_Size];
        }
        public void Start(int SerialIndex)
        {
            if (0 <= SerialIndex || SerialIndex <= 2)
            {
                isRunning[SerialIndex] = true;
                serialPortTasks[SerialIndex] = Task.Factory.StartNew(() => StartSerial(SerialIndex), _cancellationToken).ContinueWith((t) => SerialTaskDone(t, SerialIndex == 0));
            }
            else
                throw new ArgumentOutOfRangeException();
        }
        public void Stop(int SerialIndex)
        {
            isRunning[SerialIndex] = false;
            _openPortBtn.BeginInvoke((MethodInvoker)delegate ()
            {
                _openPortBtn.Enabled = true;
            });
        }

        public void Record(int SerialIndex)
        {
            recording[SerialIndex] = true;
        }
        public void StopRecord(int SerialIndex)
        {
            recording[SerialIndex] = false;
        }
        public void RecordDirectory(string Directory)
        {
            RecordingDirectory = Directory;
        }

        public void SetSetting_Port(int SerialIndex)
        {
            byte setOk = 1;
            if (SerialIndex == 0)
            {
                if (_Com_ComboBox.SelectedItem != null)
                    PortNumber = _Com_ComboBox.SelectedItem.ToString();
                if (_Baud_ComboBox.SelectedItem != null)
                    Baudrate = int.Parse(_Baud_ComboBox.SelectedItem.ToString());
                if (_DataBits_ComboBox.SelectedItem != null)
                    DataBit = int.Parse(_DataBits_ComboBox.SelectedItem.ToString());
                else
                {
                    setOk = 0;
                    MessageBox.Show("Please fill in the fields.", "Faild to Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                _SerialPort[0] = new SerialPort(PortNumber, Baudrate, ParityBit, DataBit, StopBit);
            }
            if (SerialIndex == 1)
            {
                if (_Com_ComboBox2.SelectedItem != null)
                    PortNumber = _Com_ComboBox2.SelectedItem.ToString();
                if (_Baud_ComboBox2.SelectedItem != null)
                    Baudrate = int.Parse(_Baud_ComboBox2.SelectedItem.ToString());
                if (_DataBits_ComboBox2.SelectedItem != null)
                    DataBit = int.Parse(_DataBits_ComboBox2.SelectedItem.ToString());
                else
                {
                    setOk = 0;
                    MessageBox.Show("Please fill in the fields.", "Faild to Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                _SerialPort[1] = new SerialPort(PortNumber, Baudrate, ParityBit, DataBit, StopBit);
            }
            if (setOk == 1)
                MessageBox.Show("Serial settings saved!");
        }
        private void StartSerial(int index)
        {
            Open = true;
            serialportIndex = index;
            var setting = Settings[index];
            _SerialPort[index] = new SerialPort(setting.PortNumber, setting.Baudrate, ParityBit, setting.DataBit, StopBit);
            _SerialPort[index].Open();
            _openPortBtn.BeginInvoke((MethodInvoker)delegate ()
            {
                _openPortBtn.Enabled = false;
            });
            if (!_SerialPort[index].IsOpen)
                MessageBox.Show($"Port is disconnect! {index}");
                //throw new Exception($"Cannot open port {index}");
            ChangePictureBox(index == 0 ? _Serial1Status : _Serial2Status, AppForJoystickCameraAndSerial.Properties.Resources.Green_Circle);
            while (isRunning[index])
            {
                Data_Rx[Data_Counter] = (byte)_SerialPort[index].ReadByte();
                Data_Counter = (Data_Counter + 1) % DataInBuffer_Size;
                if (Data_Rx[0] == 85 && Data_Counter == 55)
                {
                    Handler.Master_CheckPacket(Data_Rx, RecordingDirectory, recording[index], index, _Fov_TextBox, _AzError_TextBox, _EiError_TextBox, _Ax_TextBox, _Ay_TextBox, _Az_TextBox);
                    Data_Counter = 0;
                }
                else if (Data_Rx[0] != 85)
                {
                    Array.Clear(Data_Rx, 0, 55);
                    Data_Counter = 0;
                }
            }
        }
        private void SerialTaskDone(Task task, bool isMain)
        {
            if (task.IsCompletedSuccessfully)
                ChangePictureBox(isMain == true ? _Serial1Status : _Serial2Status, AppForJoystickCameraAndSerial.Properties.Resources.Red_Circle);
            else
            {
                isRunning[Convert.ToInt32(isMain)] = false;
            }
        }
        public void Write(byte Code, byte Address, Int32[] Value, byte Length)
        {
            byte[] Data = new byte[55];
            Handler.WriteMessage_Generator(Code, Address, Value, Length, Data);
            for (byte i = 0; i < 55; i++)
            {
                Console.Write(i + ":      ");
                Console.WriteLine(Data[i]);
            }

            if (Open && serialportIndex == 0)
                _SerialPort[0].Write(Data, 0, 55);
            else if (Open && serialportIndex == 1)
                _SerialPort[1].Write(Data, 0, 55);
            else
                MessageBox.Show("SerialPort is not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}