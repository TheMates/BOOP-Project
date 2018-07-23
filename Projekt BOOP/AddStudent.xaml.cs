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
    /// Interaction logic for AddStudent.xaml
    /// </summary>
    public partial class AddStudent : Window
    {
        public AddStudent()
        {
            InitializeComponent();
        }

        private void BtnAddStudSAVE_Click(object sender, RoutedEventArgs e)
        {
            bool missing = false;
            missing = IsMissing(TxtAddStudName,missing);
            missing = IsMissing(TxtAddStudSurname,missing);
            missing = IsMissing(TxtAddStudID,missing);
            missing = IsMissing(TxtAddStudID.Text, missing);
            missing = IsMissing(TxtAddStudFak,missing);
            missing = IsMissing(TxtAddStudRoc,missing);

            if (missing == true)
            {
                System.Media.SystemSounds.Exclamation.Play();   //hodí chyba zvuk
                return; 
            }
            
            Studenti Novy = new Studenti() { Jmeno = TxtAddStudName.Text, Prijmeni = TxtAddStudSurname.Text, Fakulta = (short)((short)TxtAddStudFak.SelectedIndex + 1), Rocnik = Int16.Parse(TxtAddStudRoc.Text.Substring(0, 1)), StudentID = Int32.Parse(TxtAddStudID.Text) };
            try
            {
                ((MainWindow)Application.Current.MainWindow).BtnAddStudent(Novy);
            }
            catch(Exception ex)
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
                if(val == true)
                { return true; }
                return false;
            }
        }
        private bool IsMissing(ComboBox cbox, bool val)
        {
            //ve windows 8 to není vidět ta hranice
            if (cbox.Text == "")
            {   
                cbox.BorderBrush = Brushes.Red;
                switch(cbox.Name.Contains("Fak")) //jenom pro win 8
                {
                    case true: ComBorderStudAddFak.BorderBrush = Brushes.Red; break;
                    case false: ComBorderStudAddRoc.BorderBrush = Brushes.Red; break;
                }                
                return true;

            }
            else
            {
                cbox.ClearValue(TextBox.BorderBrushProperty);
                switch (cbox.Name.Contains("Fak")) //jenom pro win 8
                {
                    case true: ComBorderStudAddFak.ClearValue(Border.BorderBrushProperty); break;
                    case false: ComBorderStudAddRoc.ClearValue(Border.BorderBrushProperty); break;
                }
                if (val == true)
                { return true; }
                return false;
            }

        }

        private bool IsMissing(string ID, bool val)
        {
            //ve windows 8 to není vidět ta hranice
            int temp;
            if (!Int32.TryParse(TxtAddStudID.Text,out temp))
            {
                TxtAddStudID.BorderBrush = Brushes.Red;
                return true;
            }
            else
            {
                TxtAddStudID.ClearValue(TextBox.BorderBrushProperty);
                if (val == true)
                { return true; }
                return false;
            }

        }


        private void TxtAddStudID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //aby tam šly dávat jen čísla 
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;

        }


    }
}
