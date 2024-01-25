using ServoNew_Black.ViewModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ServoNew_Black.Views
{
    /// <summary>
    /// Page_Manual.xaml 的交互逻辑
    /// </summary>
    public partial class Page_Manual : Window
    {
        public Page_Manual()
        {
            InitializeComponent();

            var con = new ViewModel.Mannual();
            this.DataContext = con;
        }
    }
}
