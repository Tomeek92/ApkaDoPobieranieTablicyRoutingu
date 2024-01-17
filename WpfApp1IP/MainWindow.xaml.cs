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
using Microsoft.Win32;
using System.ComponentModel;
using tik4net;


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
            string fileName = "TablicaRoutingu.txt";
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);



            p.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show("Wystąpił błąd: " + error);
            }
            else
            {

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "TablicaRoutingu"; // Default file name
                dlg.DefaultExt = ".txt"; // Default file extension
                dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension


                Nullable<bool> result = dlg.ShowDialog();


                if (result == true)
                {

                    string filename = dlg.FileName;
                    File.WriteAllText(filename, output);
                }
            }
        }



        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            textBox1.Text = string.Empty;
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProps = ni.GetIPProperties();
                foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                {
                    textBox1.Text += $"Nazwa: {ni.Name} \n IP: {ip.Address} \n Mask: {ip.IPv4Mask} \n\n";
                }

            }
        }

        public void DisplayIPAddress()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ipAddress in ipAddresses)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBox1.Text = "Twój adress ip:" + " " + ipAddress.ToString();

                    break;
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {


            textBox1.Text = string.Empty;
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            textBox1.Text += $"Nazwa: {ni.Name} \n IP: {ip.Address} \n\n";


                        }
                    }
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

            textBox1.Text = string.Empty;
            ProcessStartInfo psi = new ProcessStartInfo("cmd", "/c netsh wlan show networks mode=bssid")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process p = Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            textBox1.Text = output;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            string[] routers = textBox1.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string routerIP in routers)
            {
                Task.Run(() =>
                {
                    try
                    {
                        using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                        {
                            connection.Open("192.168.12.1", "admin", "admin");

                            var command = connection.CreateCommandAndParameters("/ip/route/print");
                            var result = command.ExecuteList();

                            var output = new StringBuilder();

                            foreach (var sentence in result)
                            {
                                foreach (var word in sentence.Words)
                                {
                                    string line = $"{word.Key}: {word.Value}";
                                    output.AppendLine(line);
                                }
                            }

                            Dispatcher.Invoke(() =>
                            {
                                textBox1.Text = $"Routing table for 192.168.12.1:\n{output}";
                            });
                        }
                    }
                    catch (TikCommandException ex)
                    {
                        // Handle the exception for connection issues
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Nie można nawiązać połączenia z 192.168.12.1: " + ex.Message, "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                    catch (Exception ex)
                    {
                        // Handle other exceptions
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Wystąpił błąd podczas pobierania tablicy routingu z 192.168.12.1: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }

        }




        private void Button_Click_8(object sender, RoutedEventArgs e)
        {


            string[] routers = textBox1.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string routerIP in routers)
            {
                Task.Run(() =>
                {
                    try
                    {
                        using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                        {
                            connection.Open("192.168.12.1", "admin", "admin");

                            var command = connection.CreateCommandAndParameters("/ip/route/print");
                            var result = command.ExecuteList();

                            // Use Dispatcher to update UI from a non-UI thread
                            Dispatcher.Invoke(() =>
                            {
                                // Open save file dialog
                                SaveFileDialog saveFileDialog = new SaveFileDialog();
                                saveFileDialog.Filter = "Text file (*.txt)|*.txt";
                                if (saveFileDialog.ShowDialog() == true)
                                {
                                    // Convert the result to a string
                                    var resultString = string.Join("\n", result.Select(r => r.ToString()));

                                    // Save the result to the selected file
                                    File.WriteAllText(saveFileDialog.FileName, $"Routing table for 192.168.12.1:\n{resultString}");
                                }
                            });
                        }
                    }
                    catch (TikCommandException ex)
                    {
                        // Handle the exception for connection issues
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Nie można nawiązać połączenia z {routerIP}: " + ex.Message, "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                    catch (Exception ex)
                    {
                        // Handle other exceptions
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Wystąpił błąd podczas pobierania tablicy routingu z {routerIP}: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }


        }
    }
}
    


    



    
