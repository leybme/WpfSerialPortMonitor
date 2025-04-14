using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System;
using System.Reflection;

namespace WpfSerialPortMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort serial = new SerialPort();
        string recieved_data=null!;
        public MainWindow()
        {
            InitializeComponent();
            LoadAvailablePorts();
            Connect.Content = "Connect";

        }
        private void LoadAvailablePorts()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                Debug.WriteLine("The following serial ports were found:");
                foreach (string port in ports)
                {
                    Debug.WriteLine(port);
                }
                Comport.ItemsSource = ports;
                if (ports.Length > 0)
                {
                    Comport.SelectedIndex = 0; // Select the first port by default
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading ports: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var x in Comport.Items)
            {
                Debug.WriteLine(x);
                //if (Settings.Default.COMPORT == x.ToString())
                //{
                //    Debug.WriteLine(x);
                //    Comport.SelectedItem = x;
                //}
            }
        }


        private void Connect_Click(object sender, RoutedEventArgs e)
        {
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
            if (Connect.Content == "Connect")

            {
                try
                {
                    serial.Close();
                    serial.PortName = Comport.Text;
                    serial.BaudRate = int.Parse(Baudrate.Text);
                    serial.Handshake = Handshake.None;
                    serial.Parity = Parity.None;
                    serial.DataBits = 8;
                    serial.StopBits = StopBits.One;
                    serial.ReadTimeout = 50;
                    serial.WriteTimeout = 100;
                   
                    serial.Open();
                    //Settings.Default.COMPORT = Comport.Text;
                    //Settings.Default.Save();


                    //Sets button State and Creates function call on data recieved
                    Connect.Content = "Disconnect";
                    SendCMDButton.IsEnabled = true;
                    CMDtextbox.IsEnabled = true;
             
                    serial.DataReceived += new SerialDataReceivedEventHandler(Recieve);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            else
            {
                try // just in case serial port is not open could also be acheved using if(serial.IsOpen)
                {
                    serial.Close();
                    Connect.Content = "Connect";
                    SendCMDButton.IsEnabled = false;
                    CMDtextbox.IsEnabled = false;

                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
        }
        #region Recieving

        private delegate void UpdateUiTextDelegate(string text);


        private void Recieve(object sender, SerialDataReceivedEventArgs e)
        {
            // Collecting the characters received to our 'buffer' (string).

            recieved_data += serial.ReadExisting();
            if(recieved_data.Contains("\r\n"))
            {
                Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(Update), recieved_data);
                recieved_data = "";
            }
        }
        private void Update(string text)
        {
            recieveTextBox.Text += text;
     recieveTextBox.ScrollToEnd();
        }
        #endregion
        private void SendCMDButton_Click(object sender, RoutedEventArgs e)
        {
            SerialCmdSend(CMDtextbox.Text);
            CMDtextbox.Text = "";
        }

        private void Send_Data(object sender, RoutedEventArgs e)
        {
            //SerialCmdSend(SerialData.Text);
            //SerialData.Text = "";
        }

        public void SerialCmdSend(String data)
        {
            if (serial.IsOpen)
            {
                try
                {
                    data += "\r\n";
                    serial.WriteLine(data);
                    Debug.WriteLine(serial.BytesToWrite);
                    //serial.Write(Encoding.ASCII.GetBytes("test"), 0, 4);
                    //serial.Write(Encoding.ASCII.GetBytes("test"), 0, 4);
                    //serial.Write(Encoding.ASCII.GetBytes("hekko"), 0, 4);


                    //foreach (byte hexval in hexstring)
                    //{
                    //    byte[] _hexval = new byte[] { hexval }; // need to convert byte to byte[] to write
                    //    serial.Write(_hexval, 0, 1);
                    //    Thread.Sleep(1);
                    //}
                    //   serial.Write(Encoding.ASCII.GetBytes("hello2\r\n"), 0, 7);

                    //serial.Write(Encoding.ASCII.GetBytes("\r\n"), 0, 2);
                }
                catch (Exception ex)
                {
                    CMDtextbox.Text = ("Failed to SEND" + data + "\n" + ex + "\n");
                    Debug.WriteLine("Failed to SEND" + data + "\n" + ex + "\n");
                }
            }
            else
            {
                CMDtextbox.Text = ("Serial Port Not OPEN\n");
            }


        }

        private void btnGetVersion_Click(object sender, RoutedEventArgs e)
        {
          //  serial.Write(Encoding.ASCII.GetBytes("[VERSION]\r\n"), 0, Encoding.ASCII.GetBytes("[VERSION]\r\n").Length);
            safeSerialWrite("[VERSION]\r\n");
        }

        private void btnSetUSB_Click(object sender, RoutedEventArgs e)
        {

            serial.WriteLine("[USET USB 1]\r\n");
        }

        private void btnSetNormal_Click(object sender, RoutedEventArgs e)
        {
            serial.WriteLine("[USET USB 0]\r\n");
        }

        private void btnSendConfig_Click(object sender, RoutedEventArgs e)
        {
            if(tbDeviceID.Text.Length != 0)
            {
                String cmd= "[USET DEVICEID " + tbDeviceID.Text + "]\r\n";
                //serial.WriteLine(cmd);
                safeSerialWrite(cmd);
                //Thread.Sleep(cmd.Length);
                recieveTextBox.Text+=cmd;
            }
            if(tbExperimentID.Text.Length != 0)
            {
                String cmd = "[USET EXPERIMENTID " + tbExperimentID.Text + "]\r\n";
                safeSerialWrite(cmd);
                recieveTextBox.Text += cmd;
            }
        }

        private void btnTestSend_Click(object sender, RoutedEventArgs e)
        {

           // serial.WriteLine("[USET STSTR TLD-1103;b45789a5-41a2-422f-9b9f-e6f36f3d327a;PLT001-AA;1,5,32;1;20;100;100000;1;100,143.84,206.91,297.64,428.13,615.85,885.87,1274.27,1832.98,2636.65,3792.69,5455.59,7847.6,11288.38,16237.77,23357.21,33598.18,48329.3,69519.28,100000;0;A96 1]\r\n");
            //Task.Run(() => serial.WriteLine("[USET STSTR TLD-1103;b45789a5-41a2-422f-9b9f-e6f36f3d327a;PLT001-AA;1,5,32;1;20;100;100000;1;100,143.84,206.91,297.64,428.13,615.85,885.87,1274.27,1832.98,2636.65,3792.69,5455.59,7847.6,11288.38,16237.77,23357.21,33598.18,48329.3,69519.28,100000;0;A96 1]\r\n"));
            String data = "[USET SETSTR TLD-1103;b45789a5-41a2-422f-9b9f-e6f36f3d327a;PLT001-AA;1,2,5,32;1;20;100;100000;1;100,143.84,206.91,297.64,428.13,615.85,885.87,1274.27,1832.98,2636.65,3792.69,5455.59,7847.6,11288.38,16237.77,23357.21,33598.18,48329.3,69519.28,100000;0;A96]\r\n";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int chunkSize = 64; // Adjust chunk size as needed
            for (int i = 0; i < buffer.Length; i += chunkSize)
            {
                int size = Math.Min(chunkSize, buffer.Length - i);
                serial.Write(buffer, i, size);
                //convert buffer to string
                string str = Encoding.ASCII.GetString(buffer, i, size);
                recieveTextBox.Text += str;
                Thread.Sleep(size);
            }
            

        }
        private void safeSerialWrite(String data)
        {
            try
            {
                serial.WriteLine(data);
                Thread.Sleep(data.Length);  
            }
            catch (Exception ex)
            {
                if (serial.IsOpen)
                {
                    serial.Close();
                    Connect.Content = "Connect";
                    SendCMDButton.IsEnabled = false;
                    CMDtextbox.IsEnabled = false;
                }
                CMDtextbox.Text = ("Failed to SEND" + data + "\n" + ex + "\n");
                Debug.WriteLine("Failed to SEND" + data + "\n" + ex + "\n");
            }
        }

        private void btnDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            String filename = tbFileName.Text;
            if (filename.Length != 0)
            {
                if(!filename.Contains("RESULTS"))
                {
                    filename = "RESULTS/"+filename;
                }
                String cmd = "[UCTRL DELETE " + filename + "]\r\n";
                safeSerialWrite(cmd);
                recieveTextBox.Text += cmd;
            }
        }
       
        private void btnGetFile_Click(object sender, RoutedEventArgs e)
        {
            String filename = tbFileName.Text;
            if (filename.Length != 0)
            {
                if (!filename.Contains("RESULTS"))
                {
                    filename = "RESULTS/" + filename;
                }
                String cmd = "[UGET FILE " + filename + "]\r\n";
                safeSerialWrite(cmd);
                recieveTextBox.Text += cmd;
            }
        }

        private void btnSendFile_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}