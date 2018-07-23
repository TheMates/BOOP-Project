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
    /// Interaction logic for Alter.xaml
    /// </summary>
    public partial class Alter : Window
    {
        public int state;
        public string identifier;

        //hodně podobnýho kódu,dělaný skoro nakonec
        public Alter()
        {
            InitializeComponent();
        }

        private void BtnAlterSave_Click(object sender, RoutedEventArgs e)
        {
            
            dynamic selekce;
            switch(state)
            {
                case 1:   
                    selekce = from stud in ((MainWindow)Application.Current.MainWindow).db.Studentis
                            where stud.StudentID == Int32.Parse(identifier)
                            select stud;
                    var sel = selekce as IQueryable<Studenti>;
                    var sell = sel.First();
                    
                    sell.Jmeno = TxtAlterStudName.Text;
                    sell.Prijmeni = TxtAlterStudPrijm.Text;
                    sell.Rocnik = (Int16)Int32.Parse(CmbAlterRoc.Text.Substring(0, 1));
                    sell.Fakulta = (Int16)(Int32)(Enum.Parse(typeof(Fakulta), CmbAlterFak.Text));                //updatnu všechny věci jmeno prijmeni rocnik fakulta
                    
                    break;

                case 2: 
                        selekce = from stud in ((MainWindow)Application.Current.MainWindow).db.Studentis
                            where stud.StudentID == Int32.Parse(TxtAlterStudId.Text)
                            select stud;
                    var studd = selekce as IQueryable<Studenti>;
                    var idstud = studd.First();
                    var znamkasel = from predm in ((MainWindow)Application.Current.MainWindow).db.Znamkies    //jen konverze hodnocení z písmena na číslo
                                  where predm.ZnamkaAlias == CmbAlterHodn.Text[0]
                                  select predm.ZnamkaValue;
                    var znamka = znamkasel as IQueryable<Single>;
                    var hodnoceni = from hodn in ((MainWindow)Application.Current.MainWindow).db.Hodnocenis
                                    where hodn.IdStud == idstud.Id
                                    where hodn.IdPredm == identifier
                                    select hodn;
                    var hod = hodnoceni as IQueryable<Hodnoceni>;
                    var zmenithodn = hod.First();
                    zmenithodn.Hodn = znamka.First();
                    break;

                case 3: 
                    selekce = from stud in ((MainWindow)Application.Current.MainWindow).db.Studentis
                            where stud.StudentID == Int32.Parse(identifier)
                            select stud;
                    var studd1 = selekce as IQueryable<Studenti>;
                    var idstud1 = studd1.First();
                    var znamkasel1 = from predm in ((MainWindow)Application.Current.MainWindow).db.Znamkies    //jen konverze hodnocení z písmena na číslo
                                  where predm.ZnamkaAlias == CmbAlterHodn.Text[0]
                                  select predm.ZnamkaValue;
                    var znamka1 = znamkasel1 as IQueryable<Single>;
                    var hodnoceni1 = from hodn in ((MainWindow)Application.Current.MainWindow).db.Hodnocenis
                                    where hodn.IdStud == idstud1.Id
                                    where hodn.IdPredm == TxtAlterPredmZkr.Text
                                    select hodn;
                    var hod1 = hodnoceni1 as IQueryable<Hodnoceni>;
                    var zmenithodn1 = hod1.First();
                    zmenithodn1.Hodn = znamka1.First();
                    break;

            }
            try
            {
             ((MainWindow)Application.Current.MainWindow).db.SubmitChanges();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Close();
        }



    }
}
