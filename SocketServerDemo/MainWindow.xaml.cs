using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SocketServerDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketServer socketServer;
        public MainWindow()
        {
            InitializeComponent();


            txt_ip.Text = "127.0.0.1";
            txt_port.Text = "8089";



            button.Click += (s, e) =>
            {
                ((Button)s).IsEnabled = false;
                ((TextBox)txt_ip).IsEnabled = false;
                ((TextBox)txt_port).IsEnabled = false;

                socketServer = new SocketServer(txt_ip.Text.Trim(),int.Parse(txt_port.Text));

                socketServer.MessageNotify += SocketServer_MessageNotify;
                socketServer.ClientManage += SocketServer_ClientManage;

                socketServer.StartServer();

            };

        }
        
        private void SocketServer_ClientManage(object sender, ClientChangeArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => {
                if (e.ChangeCode == 1)
                {
                    lsb_client.Items.Add(e.ClientName);
                }
                else
                {
                    lsb_client.Items.Remove(e.ClientName);
                }
            }));
        }

        private void SocketServer_MessageNotify(object sender, MessageNotifyArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                lsb_msg.Items.Add(e.Msg);
            }));
        }
    }
}
