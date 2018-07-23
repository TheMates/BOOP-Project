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
    /// Interaction logic for AddHodnoc.xaml
    /// </summary>
    public partial class AddHodnoc : Window
    {
        Dictionary<string, string> PredmetyDict = new Dictionary<string, string>();     //pro propojení zkratky předmětu a názvu
        List<string> HodnList = new List<string> { "A", "B", "C", "D", "E", "F" };      //pořád stejná kolekce nabídky hodnocení
        public AddHodnoc()
        {
            InitializeComponent();            
        }
        /// <summary>
        /// Hlavně sleduje změnu textu a zajišťuje propojení zkratky a názvu předmětu v comboboxech
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbAddHodnName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox cbox = sender as ComboBox;
            if (CmbAddHodnStudName.IsEnabled == false)  //na zjištění jestli jsme klikli na přidání hodnocení v záložce studenti nebo předměty
            {
                switch (cbox.Name)
                {
                    //když se změní vstupní hodnota a nenajde se k tomu pár tak se vymaže ta druhá hodnota(text jednoho ze dvou comboboxů) než právě editujeme - nesmaže se to kde píšem
                    case "CmbAddHodnName":
                        if (PredmetyDict.FirstOrDefault(p => p.Value == CmbAddHodnName.Text.ToString()).Key == null || CmbAddHodnName.Text == "" && !CmbAddHodnocZkr.IsKeyboardFocusWithin)
                        { CmbAddHodnocZkr.SelectedIndex = -1; return; }
                        CmbAddHodnocZkr.SelectedIndex = CmbAddHodnocZkr.Items.IndexOf(PredmetyDict.FirstOrDefault(p => p.Value == CmbAddHodnName.Text.ToString()).Key); break; //nejdřív vrátím Key k určitý value a podle toho pak přiřadím selected index
                    case "CmbAddHodnocZkr":
                        if (CmbAddHodnocZkr.Text == "" || PredmetyDict[CmbAddHodnocZkr.Text] == null && !CmbAddHodnName.IsKeyboardFocusWithin)  //nejdřív se testuje podmínka na prázdnej řetězec, protože chceme-li přistupovat k hodnotě s klíčem "" tak to hodí chybu
                        { CmbAddHodnName.SelectedIndex = -1; return; }
                        CmbAddHodnName.SelectedIndex = CmbAddHodnName.Items.IndexOf(PredmetyDict[CmbAddHodnocZkr.Text]); break;
                }
             }

        }
        /// <summary>
        /// Kontroluje, jestli je vše potřebné vyplněné. Poté vytvoří instanci nového hodnocení a pošle ji do metody v hlavním okně, kde se vyhodnotí, zda už hodnocení přidané je, nebo jestli neproběhne jiná chyba. Pokud ne, mainwindow metoda automaticky provede update datagridů.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddHodnocSAVE_Click(object sender, RoutedEventArgs e)
        {
            bool missing = false;
            missing = IsMissing(CmbAddHodnStudName, missing);
            missing = IsMissing(CmbAddHodnName, missing);
            missing = IsMissing(CmbAddHodnocZkr, missing);
            missing = IsMissing(CmbAddHodnocHodnoc, missing);

            if (missing == true)
            {
                System.Media.SystemSounds.Exclamation.Play();   //hodí chyba zvuk
                return;
            }

            IQueryable<Studenti> student = from stud in ((MainWindow)Application.Current.MainWindow).db.Studentis
                                           select stud;
 
            switch(CmbAddHodnStudName.IsEnabled)    //jestli přišlo z studenti nebo předměty
            {
                case true:
                    student = student.Where(s => s.StudentID == Int32.Parse(CmbAddHodnStudName.Text.Substring(0, 6)));
                    break;
                case false:
                    student = student.Where(s => s.StudentID == Int32.Parse(LblAddHodnocStudId.Content.ToString()));
                    break;
            }
            var predmet = from predm in ((MainWindow)Application.Current.MainWindow).db.Znamkies    //jen konverze hodnocení z písmena na číslo
                          where predm.ZnamkaAlias == CmbAddHodnocHodnoc.Text[0]
                          select predm.ZnamkaValue;
            Hodnoceni nove = new Hodnoceni() { IdStud = student.First().Id, IdPredm = CmbAddHodnocZkr.Text, Hodn = predmet.First() };
            try
            {
                ((MainWindow)Application.Current.MainWindow).AddHodnoc(nove);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            this.Close();
        }

        /// <summary>
        /// Kontroluje, jestli je v comboboxu text.
        /// </summary>
        /// <param name="cmb">Kontrolovaný combobox.</param>
        /// <param name="val">Současná hodnota bool ismissing proměnné.</param>
        /// <returns></returns>
        private bool IsMissing(ComboBox cmb, bool val)
        {
            if(cmb.Text == "")
            {
                cmb.BorderBrush = Brushes.Red;
                //jestli je to hodnocenibox
                switch (cmb.Name) 
                {
                    case "CmbAddHodnocHodnoc": BrdAddHodnHodn.BorderBrush = Brushes.Red; break;
                    case "CmbAddHodnName":
                    case "CmbAddHodnZkr":
                        BrdAddHodnName.BorderBrush = Brushes.Red;
                        BrdAddHodnZkr.BorderBrush = Brushes.Red;
                        break;
                    case "CmbAddHodnStudName":
                        BrdAddHodnStudName.BorderBrush = Brushes.Red;
                        break;       
                }
                return true; 
            }
            else
            {
                cmb.ClearValue(ComboBox.BorderBrushProperty);
                switch (cmb.Name) //jestli je to BrdHod"nH"odn 
                {
                    case "CmbAddHodnocHodnoc": BrdAddHodnHodn.ClearValue(ComboBox.BorderBrushProperty); break;
                    case "CmbAddHodnName":
                    case "CmbAddHodnZkr":
                        BrdAddHodnName.ClearValue(ComboBox.BorderBrushProperty);
                        BrdAddHodnZkr.ClearValue(ComboBox.BorderBrushProperty);
                        break;
                    case "CmbAddHodnStudName":
                        BrdAddHodnStudName.ClearValue(ComboBox.BorderBrushProperty);
                        break;       
                }

                if (val == true)
                { return true; }
                return false;
            }

        }
        /// <summary>
        /// Metoda na naplnění comboboxů různými kolekcemi na základě toho, jestli jsme klikli v záložce Studenti nebo Předměty.
        /// </summary>
        public void LoadComponents()
        {
            switch(CmbAddHodnStudName.IsEnabled)
            {
                case true:  //když bude z předmětů tak automaticky vyplním předmět a budu volit studento
                    var studenti = ((MainWindow)Application.Current.MainWindow).DataGridStud.ItemsSource as IQueryable<StudentiList>;
                    var StudentiZobraz = from s in studenti
                                         orderby s.Prijmeni
                                         select new { s.ID, s.Jmeno, s.Prijmeni };
                    List<string> StdList = new List<string>();
                    foreach(var s in StudentiZobraz)
                    {
                        StdList.Add(s.ID + "   " + s.Prijmeni + " " + s.Jmeno);
                    }

                    CmbAddHodnStudName.ItemsSource = StdList;
                    CmbAddHodnocHodnoc.ItemsSource = HodnList;
                    break;

                case false: //když bude ze studentů automaticky se vyplní jméno a budu volit předmět
                    var predm = ((MainWindow)Application.Current.MainWindow).DataGridPredm.ItemsSource as IQueryable<VypisPredmety>;
                    var CmbNameSource = from a in predm
                                        orderby a.Název
                                        select a.Název;
                    var CmbZkrSource = from b in predm
                                       orderby b.Zkratka
                                       select b.Zkratka;
                    var predmetysel = from c in predm
                                      select new { c.Zkratka, c.Název };
                    PredmetyDict = predmetysel.ToList().ToDictionary(p => p.Zkratka, p => p.Název);
                    CmbAddHodnName.ItemsSource = CmbNameSource;
                    CmbAddHodnocZkr.ItemsSource = CmbZkrSource;
                    CmbAddHodnocHodnoc.ItemsSource = HodnList;
                    break;
            }
        }

    }
}
