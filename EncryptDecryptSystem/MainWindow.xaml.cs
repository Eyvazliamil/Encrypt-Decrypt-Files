using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EncryptDecryptSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancelProgress;
        protected string key = "key123"; // Password
        public MainWindow()
        {
            InitializeComponent();
        }

        private void txtbox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Filebtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if(ofd.ShowDialog() == true)
            {
                string selectedFile = ofd.FileName;
                txtbox1.Text = selectedFile;
            }
        }

        private void Progressbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void txtbox2_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private async void Startbtn_Click(object sender, RoutedEventArgs e)
        {
            if (key != txtbox2.Text)
            {
                MessageBox.Show("Wrong Password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(txtbox1.Text))
            {
                MessageBox.Show("File not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Radioencrypt.IsChecked != true && Radiodecrypt.IsChecked != true) 
            {
                MessageBox.Show("Please select Encrypt or Decrypt!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            cancelProgress = new CancellationTokenSource();

            try
            {
                string from = txtbox1.Text;

                string to = null; 

                if (Radioencrypt.IsChecked == true)
                    to += from + ".enc";
                else
                    to += from + ".dec";

                FileInfo fileInfo = new FileInfo(from);
                long totalLength = fileInfo.Length;
                long totalProcessed = 0;

                Progressbar.Minimum = 0;
                Progressbar.Maximum = 100;
                Progressbar.Value = 0;

                using FileStream fromStream = new FileStream(from, FileMode.Open, FileAccess.Read);
                using FileStream toStream = new FileStream(to, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[81920];

                using Aes aes = Aes.Create();
                aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));
                aes.IV = new byte[16];

                ICryptoTransform cryptoTransform;

                if (Radioencrypt.IsChecked == true)
                    cryptoTransform = aes.CreateEncryptor();
                else
                    cryptoTransform = aes.CreateDecryptor();

                using CryptoStream cryptoStream = new CryptoStream(toStream, cryptoTransform, CryptoStreamMode.Write);

                while (true)
                {
                    cancelProgress.Token.ThrowIfCancellationRequested();

                    int bytesRead = await fromStream.ReadAsync(buffer, 0, buffer.Length, cancelProgress.Token);

                    if (bytesRead == 0)
                        break;

                    await cryptoStream.WriteAsync(buffer, 0, bytesRead, cancelProgress.Token);

                    totalProcessed += bytesRead;
                    Progressbar.Value = (double)totalProcessed / totalLength * 100;
                }

                await cryptoStream.FlushAsync(cancelProgress.Token);

                Progressbar.Value = 100;

                MessageBox.Show("Completed!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                Progressbar.Value = 0;
            }
            catch (OperationCanceledException)
            {
                Progressbar.Value = 0;
                MessageBox.Show("Progress Canceled!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Progressbar.Value = 0;
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Cancelbtn_Click(object sender, RoutedEventArgs e)
        { 
            if (cancelProgress != null)
                cancelProgress.Cancel();
        }

        private void Radioencrypt_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Radiodecrypt_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}