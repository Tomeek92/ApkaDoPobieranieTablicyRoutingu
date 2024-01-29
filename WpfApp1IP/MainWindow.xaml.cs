using Microsoft.Win32;
using Renci.SshNet;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using tik4net;




namespace WpfApp1IP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SshClient client;
        
        public MainWindow()
        {
            InitializeComponent();
            DisplayIPAddress();
            textbox2.KeyDown += textbox2_KeyDown;
            textbox2.IsEnabled = false;
            button14.Click += button14_Click;      
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
        public void Button_Click_3(object sender, RoutedEventArgs e)
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
        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            string host = "192.168.12.1";
            string username = "admin";
            string password = "admin";

            string? result = await Task.Run(() =>
            {
                using (var client = new SshClient(host, username, password))
                {
                    try
                    {
                        client.Connect();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Nie udało się połączyć: " + ex.Message);
                        MessageBox.Show("Nie udało się połączyć: " + ex.Message, "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }

                    if (client.IsConnected)
                    {
                        var cmd = client.RunCommand("/interface print");
                        Debug.WriteLine(cmd.Result);

                        client.Disconnect();

                        return cmd.Result;
                    }
                    else
                    {
                        Debug.WriteLine("Nie udało się połączyć.");
                        MessageBox.Show("Nie udało się połączyć.", "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
            });
            if (result != null)
            {
                textBox1.Text = result;
            }
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
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Nie można nawiązać połączenia z 192.168.12.1: " + ex.Message, "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                    catch (Exception ex)
                    {
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
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            string[] routers = textBox1.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string routerIP in routers)
            {
                Task.Run(() =>
                {
                    try
                    {
                        using (var client = new SshClient("100.20.20.0", "admin", "admin"))
                        {
                            client.Connect();

                            var command = client.CreateCommand("show ip route");
                            var result = command.Execute();

                            Dispatcher.Invoke(() =>
                            {
                                textBox1.Text = $"Routing table for 100.20.20.0:\n{result}";
                            });

                            client.Disconnect();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Wystąpił błąd podczas pobierania tablicy routingu z 100.20.20.0: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
        }
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            string[] routers = textBox1.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string routerIP in routers)
            {
                Task.Run(() =>
                {
                    try
                    {
                        using (var client = new SshClient("100.20.20.0", "username", "password"))
                        {
                            client.Connect();

                            var command = client.CreateCommand("show ip route");
                            var result = command.Execute();

                            // Użyj SaveFileDialog do wybrania lokalizacji zapisu pliku
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
                            saveFileDialog.DefaultExt = "txt";
                            saveFileDialog.AddExtension = true;
                            bool? dialogResult = saveFileDialog.ShowDialog();

                            if (dialogResult == true)
                            {
                                // Zapisz wynik do wybranego pliku
                                File.WriteAllText(saveFileDialog.FileName, $"Routing table for 100.20.20.0:\n{result}");
                            }

                            client.Disconnect();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Wystąpił błąd podczas pobierania tablicy routingu z 100.20.20.0: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
        }
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";
            openFileDialog.DefaultExt = "txt";
            bool? dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == true)
            {
                // Sprawdź, czy plik ma rozszerzenie .txt
                if (System.IO.Path.GetExtension(openFileDialog.FileName).ToLower() == ".txt")
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    textBox1.Text = fileContent;
                }
                else
                {
                    MessageBox.Show("Wybrany plik nie jest plikiem tekstowym (.txt). Wybierz poprawny plik.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;
            bool? dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == true)
            {

                if (System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower() == ".txt")
                {

                    File.WriteAllText(saveFileDialog.FileName, textBox1.Text);
                }
                else
                {
                    MessageBox.Show("Wybrany plik nie jest plikiem tekstowym (.txt). Wybierz poprawny plik.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
       private async void Button_Click_11(object sender, RoutedEventArgs e)
       {
            string host = "100.20.20.0";
            string username = "admin";
            string password = "admin";
            string result = await Task.Run(() =>
            {
                using (var client = new SshClient(host, username, password))
                {
                    try
                    {
                        client.Connect();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Nie udało się połączyć: " + ex.Message);
                        MessageBox.Show("Nie udało się połączyć: " + ex.Message, "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                    if (client.IsConnected)
                    {
                        var cmd = client.RunCommand("show interfaces");
                        Debug.WriteLine(cmd.Result);
                        client.Disconnect();
                        return cmd.Result;
                    }
                    else
                    {
                        Debug.WriteLine("Nie udało się połączyć.");
                        MessageBox.Show("Nie udało się połączyć.", "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
            });
        }
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
            {

            }

        private async void Button_Click_12(object sender, RoutedEventArgs e)
        {
            textbox2.IsEnabled = true;
            string host = "192.168.12.1";
            string username = "admin";
            string password = "admin";
            string command = textbox2.Text;
            try
            {
                await Task.Run(() =>
                {
                    using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
                    {
                        connection.Open(host, username, password);
                        if (connection.IsOpened)
                        {
                            var cmd = connection.CreateCommandAndParameters(command);
                            var result = cmd.ExecuteList();

                            Dispatcher.Invoke(() =>
                            {
                                foreach (var sentence in result)
                                {
                                    foreach (var word in sentence.Words)
                                    {
                                        string line = $"{word.Key}: {word.Value}";
                                        textbox2.Text += line + "\n";
                                    }
                                }
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // Wyświetl komunikat o błędzie
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Wystąpił błąd podczas próby połączenia z serwerem SSH: " + ex.Message);
                });
            }
        }
        private async void Button_Click_13(object sender, RoutedEventArgs e)
        {
            textbox2.IsEnabled = true;
            string host = "100.20.20.0";
            string username = "admin";
            string password = "admin";
            string command = textbox2.Text;

            try
            {
                await Task.Run(() =>
                {
                    using (var client = new SshClient(host, username, password))
                    {
                        client.Connect();
                        if (client.IsConnected)
                        {
                            var cmd = client.RunCommand(command);
                            Dispatcher.Invoke(() =>
                            {
                                if (string.IsNullOrEmpty(cmd.Result))
                                {
                                    MessageBox.Show("Polecenie SSH nie zwróciło żadnych wyników.", "Brak wyników", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    textbox2.Text = cmd.Result;
                                }
                            });
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show("Nie udało się połączyć z serwerem SSH.", "Błąd połączenia", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                        }
                        client.Disconnect();
                    }
                });
            }
            catch (Exception ex)
            {
                // Wyświetl komunikat o błędzie
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Wystąpił błąd podczas próby połączenia z serwerem SSH: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private async void button14_Click(object sender, RoutedEventArgs e)
        {
            textbox2.IsEnabled = true;
        }
        private void textbox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string command = textbox2.Text;
                textbox2.Text = RunCommand(command);
            }
        }
        private static string RunCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd", "/c " + command)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        private void Button_Click_14(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;

            bool? dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == true)
            {

                if (System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower() == ".txt")
                {

                    File.WriteAllText(saveFileDialog.FileName, textbox2.Text);
                }
                else
                {
                    MessageBox.Show("Wybrany plik nie jest plikiem tekstowym (.txt). Wybierz poprawny plik.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
    
}
       


        
    



    


    



    
