using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Reflection.Emit;
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
using System.Net.NetworkInformation;


namespace WpfApp1IP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DisplayIPAddress();
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            
  
                ProcessStartInfo psi = new ProcessStartInfo("cmd", "/c route print")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                MessageBox.Show(output);
            

        }

        public void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd", "/c route print")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process p = Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            string fileName = "testMyTest.txt";
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);



            p.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show("Wystąpił błąd: " + error);
            }
            else
            {
                
                File.WriteAllText(path, output);
            }
        }

       

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd", "/c route print")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process p = Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            MessageBox.Show(output);
        }



        public void DisplayIPAddress()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ipAddress in ipAddresses)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipLabel.Content = "Twój adress ip:"+" "+ipAddress.ToString();
                   
                    break;
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
            textBox1.Text = "";
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            textBox1.Text += ni.Name + ": " + ip.Address.ToString() + Environment.NewLine;
                        }
                    }
                }
            }


        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }


    }
}


    
