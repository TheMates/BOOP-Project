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
using System.Windows.Shapes;

namespace Projekt_BOOP
{
    /// <summary>
    /// Interaction logic for AddSubject.xaml
    /// </summary>
    public partial class AddSubject : Window
    {
        public AddSubject()
        {
            InitializeComponent();

        }

        private void BtnAddSubjSAVE_Click(object sender, RoutedEventArgs e)
        {
            bool missing = false;
            missing = IsMissing(TxtAddSubjName, missing);
            missing = IsMissing(TxtAddSubjZkr, missing);
            if (missing == true)
            {
                System.Media.SystemSounds.Exclamation.Play();   //hodí chyba zvuk
                return;
            }

            Predmety novy = new Predmety() {ZkratkaPredm = TxtAddSubjZkr.Text, NazevPredm = TxtAddSubjName.Text };
            try
            {
                ((MainWindow)Application.Current.MainWindow).BtnAddPredm(novy);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            this.Close();

        }

        private bool IsMissing(TextBox txtbox, bool val)
        {
            if (txtbox.Text == "")
            {
                txtbox.BorderBrush = Brushes.Red;
                return true;
            }
            else
            {
                txtbox.ClearValue(TextBox.BorderBrushProperty);
                if (val == true)
                { return true; }
                return false;
            }
        }

    
    }
}
