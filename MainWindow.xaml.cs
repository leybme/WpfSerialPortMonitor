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
                    serial.WriteTimeout = 50;
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
                    //data = "hello\r\n";
                    // Send the binary data out the port
                    byte[] hexstring = Encoding.ASCII.GetBytes(data);
                    //There is a intermitant problem that I came across
                    //If I write more than one byte in succesion without a 
                    //delay the PIC i'm communicating with will Crash
                    //I expect this id due to PC timing issues ad they are
                    //not directley connected to the COM port the solution
                    //Is a ver small 1 millisecound delay between chracters

                    serial.Write(Encoding.ASCII.GetBytes(data), 0, Encoding.ASCII.GetBytes(data).Length);
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

    }
}