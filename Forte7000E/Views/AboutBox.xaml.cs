using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Forte7000C.Views
{
    /// <summary>
    /// Interaction logic for AboutBox.xaml
    /// </summary>
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void Load_page(object sender, RoutedEventArgs e)
        {
            string filepath = @"C:\PageOne.txt";
            if(File.Exists(filepath))
            {

                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(System.IO.File.ReadAllText(filepath));
                FlowDocument document = new FlowDocument(paragraph);
                FlowDocReader.Document = document;

                PageOneText.Text = File.ReadAllText(filepath);

                PageOneText2.Text = File.ReadAllText(filepath);
            }
            

        }
    }
}
