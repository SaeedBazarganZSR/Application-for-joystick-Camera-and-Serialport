﻿using System.IO.Ports;
using static AppForJoystickCameraAndSerial.Controllers.SerialPacketHandler;

namespace AppForJoystickCameraAndSerial.Controllers
{
    public class SerialController : ControllerBase
    {
        SerialPacketHandler Handler = new SerialPacketHandler();
        public SerialPort _SerialPort { get; private set; }
        public bool Open { get; private set; } = false;
        public bool Disposed { get; private set; } = false;
        private readonly ComboBox _Com_ComboBox, _Baud_ComboBox, _DataBits_ComboBox;
        private readonly TextBox _SerialMonitoring_TextBox;
        private readonly PictureBox _Serial1Status, _Serial2Status;
        const Parity ParityBit = Parity.None;
        const StopBits StopBit = StopBits.One;
        int Baudrate, DataBit;
        string PortNumber;
        private readonly bool[] recording;

        byte[] DataBuffer_Rx = new byte[55];

        public SerialController(ComboBox _ComComboBox, ComboBox _BaudComboBox, ComboBox _DataBitsComboBox, TextBox _SerialMonitoringTextBox, PictureBox serial1Status, PictureBox serial2Status)
        {
            _Com_ComboBox = _ComComboBox;
            _Baud_ComboBox = _BaudComboBox;
            _DataBits_ComboBox = _DataBitsComboBox;
            _SerialMonitoring_TextBox = _SerialMonitoringTextBox;
            _Serial1Status = serial1Status;
            _Serial2Status = serial2Status;

        }
        public void OpenPort()
        {
            _SerialPort.DataReceived += new SerialDataReceivedEventHandler(_SerialPort_DataReceived);
            Open = true;
            _SerialPort.Open();
            if (_SerialPort.IsOpen)
                ChangePictureBox(_Serial1Status, AppForJoystickCameraAndSerial.Properties.Resources.Green_Circle);
            else
                ChangePictureBox(_Serial1Status, AppForJoystickCameraAndSerial.Properties.Resources.Red_Circle);
        }
        public void ClosePort()
        {
            if (Open)
            {
                ChangePictureBox(_Serial1Status, AppForJoystickCameraAndSerial.Properties.Resources.Red_Circle);
                Open = false;
                Disposed = true;
                _SerialPort.Close();
                _SerialPort.Dispose();
            }
        }
        public void SetSetting_Port()
        {
            if (_Com_ComboBox.SelectedItem != null)
                PortNumber = _Com_ComboBox.SelectedItem.ToString();
            else
                MessageBox.Show("Wrong Port", "Faild to Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (_Baud_ComboBox.SelectedItem != null)
                Baudrate = int.Parse(_Baud_ComboBox.SelectedItem.ToString());
            else
                MessageBox.Show("Wrong Baudrate", "Faild to Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (_DataBits_ComboBox.SelectedItem != null)
                DataBit = int.Parse(_DataBits_ComboBox.SelectedItem.ToString());
            else
                MessageBox.Show("Wrong DataBits", "Faild to Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _SerialPort = new SerialPort(PortNumber, Baudrate, ParityBit, DataBit, StopBit);
            MessageBox.Show("Serial settings saved!");
        }
        private void _SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            for (int i = 0; i < 55; i++)
            {
                DataBuffer_Rx[i] = Readbyte();
                ChangeTextBox(_SerialMonitoring_TextBox, _SerialMonitoring_TextBox.Text + DataBuffer_Rx[i].ToString());
            }
            Handler.Master_CheckPacket(DataBuffer_Rx);
        }
        public void Write(byte Code, byte Value)
        {
            byte[] Data = new byte[55];
            Handler.WriteMessage(Code, Value, Data);
            //for (byte i = 0; i < 55; i++)
            //{
            //    Console.Write(i + ":      ");
            //    Console.WriteLine(Data[i]);
            //}

            if (Open)
                _SerialPort.Write(Data, 0, 55);
            else
                MessageBox.Show("SerialPort is not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        byte Readbyte()
        {
            return (byte)_SerialPort.ReadByte();
        }

    }
}
