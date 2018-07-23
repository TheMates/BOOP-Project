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

using System.Data.SqlClient;

// ctrl + m + o = collapse all

namespace Projekt_BOOP
{
    /// <summary>
    /// Aplikace na správu hodnocení studentů s GUI a prácí s SQL databází.
    /// </summary>
    public partial class MainWindow : Window
    {

        public KlasifikaceDataClassesDataContext db = null;
        string connectionString;
        string conStrpart1 = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=";
        string conStrpart2 = @";Integrated Security=True;MultipleActiveResultSets=True";
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Vyhodí okno pro výběr souboru, zkontroluje jestli je to správná přípona a připojí se k databázi.
        /// </summary>
        private void MenuSouborOpen(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();      //default okno pro výběr souboru
            dlg.DefaultExt = ".mdf";        //formát kterej chci
            dlg.Filter = "Database file (*.mdf)|*.mdf|All Files (*.*)|*.*"; //takhle si zvolím co se bude ukazovat ve volbě formátu

            SqlConnection.ClearAllPools();      //TOHLE JE TO NEJDŮLEŽITĚJŠÍ, TAKHLE APPKA PUSTÍ SPOJENÍ S POSLEDNÍ DATABÁZÍ A UŽ JI NEPOUŽÍVÁ!!!

            Nullable<bool> result = dlg.ShowDialog();   //vyvolání to je to okno a do něj se mi zapíše jestli jsem vybral nebo ne

            while (true)
            {
                if (result == true)         //check jesi jsme vybrali
                {
                    string filename = dlg.FileName;     //získám cestu k souboru
                    string ext = System.IO.Path.GetExtension(filename); //získám příponu ale ve formátu s tečkou
                    if (ext == "." + dlg.DefaultExt)        //defaultext mi to hodí bez tečky
                    {
                        DebugCesta.Text = filename;
                        connectionString = conStrpart1 + filename + conStrpart2;
                        LoadDatabase();     //moje metoda
                        Vynuluj.Background = Brushes.LawnGreen;
                        break;
                    }
                    else
                    {
                        MessageBox.Show("Neplatný formát souboru!");
                        result = dlg.ShowDialog();
                        continue;
                    }
                }
                return; //když to zavřu křížkem
            }
        }

        private void BtnStudAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DebugCesta.Text != "")
            {
                AddStudent AddStudWindow = new AddStudent();
                AddStudWindow.ShowDialog();
            }
        }

        private void MenuSouborTerminate_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void BtnSubjAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DebugCesta.Text != "")
            {
                AddSubject AddSubjWindow = new AddSubject();
                AddSubjWindow.ShowDialog();
            }
        }
        /// <summary>
        /// Vyunuluje barvu čudlíku, text cesty a oba gridy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vynuluj_Click(object sender, RoutedEventArgs e)
        {
            Vynuluj.ClearValue(Button.BackgroundProperty);  
            DebugCesta.ClearValue(TextBox.TextProperty);
            DataGridStud.ClearValue(DataGrid.ItemsSourceProperty);
            DataGridPredm.ClearValue(DataGrid.ItemsSourceProperty);
            Grids_Leave();
        }
       /// <summary>
       /// Načte databázi a updatne náhledy gridů.
       /// </summary>
        private void LoadDatabase () 
        {
           db = new KlasifikaceDataClassesDataContext(connectionString);
           var selectionstud = from stud in db.StudentiLists
                               orderby stud.Prijmeni
                               select stud;
           var selectionpredm = from predm in db.VypisPredmeties
                                orderby predm.Název
                                select predm;
           UpdateNahled(selectionstud, DataGridStud);
           UpdateNahled(selectionpredm, DataGridPredm);                     
        }   
    
        /// <summary>
        /// Najde studenty, které splňují vyplněné parametry.
        /// </summary>
        private void BtnStudFind_Click(object sender, RoutedEventArgs e)
        {
            if(DebugCesta.Text == ""){ return; }    //když není načtená databáze, konec hned
            string Jmeno = TxtStudName.Text;
            string[] jmena;
            bool isfilter = false;      //indikuje, jestli je danej nějakej filtr
            jmena = Jmeno.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); //ono to žere jen pole parametrů, takže i když chci jen jeden musím ho dát do pole
            
            var selectionStudFind = from stud in db.StudentiLists   //vyberu všechny
                                    select stud;
            foreach(string s in jmena)      //příkaz kterej vybere takový studenty, který maj ve jméně nebo příjmení každý slovo který tam zadám (počítám max s 2 i když bych tam mohl zadat víc). Když je to prázdný, přeskočí se
            {
                if (isfilter == false) { isfilter = true; }
                selectionStudFind = selectionStudFind.Where(stud => stud.Jmeno.Contains(s) || stud.Prijmeni.Contains(s));
            }
            
            if(String.IsNullOrWhiteSpace(TxtStudID.Text) == false)  //jestli je něco napsanýho v poli id, a tak dále...
            {
                if(isfilter == false){ isfilter = true; }
                selectionStudFind = selectionStudFind.Where(stud => stud.ID == Int32.Parse(TxtStudID.Text));
            }

            if(String.IsNullOrWhiteSpace(TxtStudRoc.Text) == false)
            {
                if(isfilter == false){ isfilter = true; }
                selectionStudFind = selectionStudFind.Where(stud => stud.Rocnik == Int32.Parse(TxtStudRoc.Text.Substring(0,1))); //je tam ještě tečka, tu nechcem
            }

            if (TxtStudFak.Text != "-všechny-")
            {
                if (isfilter == false) { isfilter = true; }
                selectionStudFind = selectionStudFind.Where(stud => stud.Fakulta == TxtStudFak.Text); 
            }
            selectionStudFind = selectionStudFind.OrderBy(stud => stud.Prijmeni);   //seřadím podle příjmení
            UpdateNahled(selectionStudFind,DataGridStud);
        }

        private void BtnPredmStudFind_Click(object sender, RoutedEventArgs e)
        {
            var obsah = from hodn in db.VypisHodnoceniAlias
                        where hodn.ZkratkaPredm == LblPredmStudZkr.Content.ToString()
                        join stud in db.Studentis on hodn.Id equals stud.StudentID
                        join fak in db.Fakulties on stud.Fakulta equals fak.FakultaID
                        orderby hodn.Prijmeni
                        select new { hodn.Id, hodn.Prijmeni, hodn.Jmeno, hodn.Hodnocení, stud.Rocnik, Fakulta = fak.FakultaNazev };
            string Jmeno = TxtPredmStudName.Text;
            string[] jmena;
            bool isfilter = false;      //indikuje, jestli je danej nějakej filtr
            jmena = Jmeno.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); //ono to žere jen pole parametrů, takže i když chci jen jeden musím ho dát do pole
            foreach (string s in jmena)      //příkaz kterej vybere takový studenty, který maj ve jméně nebo příjmení každý slovo který tam zadám (počítám max s 2 i když bych tam mohl zadat víc). Když je to prázdný, přeskočí se
            {
                if (isfilter == false) { isfilter = true; }
                obsah = obsah.Where(stud => stud.Jmeno.Contains(s) || stud.Prijmeni.Contains(s));
            }
            if (String.IsNullOrWhiteSpace(TxtPredmStudID.Text) == false)  //jestli je něco napsanýho v poli id, a tak dále...
            {
                if (isfilter == false) { isfilter = true; }
                obsah = obsah.Where(stud => stud.Id == Int32.Parse(TxtPredmStudID.Text));
            }

            if (String.IsNullOrWhiteSpace(TxtPredmStudRoc.Text) == false)
            {
                if (isfilter == false) { isfilter = true; }
                obsah = obsah.Where(stud => stud.Rocnik == Int32.Parse(TxtPredmStudRoc.Text.Substring(0, 1))); //je tam ještě tečka, tu nechcem
            }

            if (TxtPredmStudFak.Text != "-všechny-")
            {
                if (isfilter == false) { isfilter = true; }
                obsah = obsah.Where(stud => stud.Fakulta == TxtPredmStudFak.Text);
            }
            //obsah = obsah.OrderBy(stud => stud.Prijmeni);   //seřadím podle příjmení
            UpdateNahled(obsah, DataGridPredmDblClck);
            
        }

        private void BtnSubjFind_Click(object sender, RoutedEventArgs e)
        {
            if (DebugCesta.Text == "") { return; }    //když není načtená databáze, konec hned
            string Jmeno = TxtPredmName.Text;
            string[] jmena;
            jmena = Jmeno.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); //ono to žere jen pole parametrů, takže i když chci jen jeden musím ho dát do pole
            bool isfilter = false;
            var selectionPredmFind = from predm in db.VypisPredmeties
                                     select predm;
            foreach (string s in jmena)
            {
                if (isfilter == false) { isfilter = true; }
                selectionPredmFind = selectionPredmFind.Where(predm => predm.Název.Contains(TxtPredmName.Text));
            }

            if (String.IsNullOrWhiteSpace(TxtPredmZkr.Text) == false)
            {
                if (isfilter == false) { isfilter = true; }
                selectionPredmFind = selectionPredmFind.Where(predm => predm.Zkratka.Contains(TxtPredmZkr.Text));
            }
            UpdateNahled(selectionPredmFind, DataGridPredm);

        }

        private void BtnStudPredmFind_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(TxtStudPredmName.Text) == false)
            {
                StudentiList ZvolStud = (StudentiList)DataGridStud.SelectedItem;
                var hodnoc = from hodn in db.VypisHodnoceniAlias
                             where hodn.Id == ZvolStud.ID
                             where hodn.NazevPredm.Contains(TxtStudPredmName.Text) || hodn.ZkratkaPredm.Contains(TxtStudPredmName.Text)
                             orderby hodn.NazevPredm
                             select new { Zkratka = hodn.ZkratkaPredm, Předmět = hodn.NazevPredm, hodn.Hodnocení };
                UpdateNahled(hodnoc, DataGridStudDblClck);
            }
        }

        private void BtnStudCancelSelection_Click(object sender, RoutedEventArgs e)
        {
            if (TxtStudName.Text != "" || TxtStudID.Text != "" || TxtStudRoc.Text != "" || TxtStudFak.Text != "-všechny-")
            {
                TxtStudName.Clear();
                TxtStudID.Clear();
                TxtStudRoc.ClearValue(ComboBox.SelectedItemProperty);
                TxtStudFak.SelectedIndex = 0;
            }
            if (DebugCesta.Text == "") { return; }
            var selectionStud = from stud in db.StudentiLists
                                orderby stud.Prijmeni
                                select stud;

            UpdateNahled(selectionStud, DataGridStud);

        }

        private void BtnStudPredmCancelSelection_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(TxtStudPredmName.Text) == false)
            {
                TxtStudPredmName.ClearValue(TextBox.TextProperty);
                StudentiList ZvolStud = (StudentiList)DataGridStud.SelectedItem;
                var hodnoc = from hodn in db.VypisHodnoceniAlias
                             where hodn.Id == ZvolStud.ID
                             orderby hodn.NazevPredm
                             select new { Zkratka = hodn.ZkratkaPredm, Předmět = hodn.NazevPredm, hodn.Hodnocení };
                UpdateNahled(hodnoc, DataGridStudDblClck);
            }
        }

        private void BtnPredmStudCancelSelection_Click(object sender, RoutedEventArgs e)
        {
            if (TxtPredmStudName.Text != "" || TxtPredmStudID.Text != "" || TxtPredmStudRoc.Text != "" || TxtPredmStudFak.Text != "-všechny-")
            {
                TxtPredmStudName.Clear();
                TxtPredmStudID.Clear();
                TxtPredmStudRoc.ClearValue(ComboBox.SelectedItemProperty);
                TxtPredmStudFak.SelectedIndex = 0;
            }
            if (DebugCesta.Text == "") { return; }
            var obsah = from hodn in db.VypisHodnoceniAlias
                        where hodn.ZkratkaPredm == LblPredmStudZkr.Content.ToString()
                        join stud in db.Studentis on hodn.Id equals stud.StudentID
                        join fak in db.Fakulties on stud.Fakulta equals fak.FakultaID
                        orderby hodn.Prijmeni
                        select new { hodn.Id, hodn.Prijmeni, hodn.Jmeno, hodn.Hodnocení, stud.Rocnik, Fakulta = fak.FakultaNazev };

            UpdateNahled(obsah, DataGridPredmDblClck);

        }

        private void BtnSubjCancelSelection_Click(object sender, RoutedEventArgs e)
        {
            if (TxtPredmName.Text != "" || TxtPredmZkr.Text != "")
            {
                TxtPredmName.Clear();
                TxtPredmZkr.Clear();
                if (DebugCesta.Text != "")
                {
                    var selectionPredm = from predm in db.VypisPredmeties
                                         orderby predm.Název
                                         select predm;

                    UpdateNahled(selectionPredm, DataGridPredm);
                }
            }
        }

        /// <summary>
        /// Updatne náhled určitého datagridu.
        /// </summary>
        /// <param name="selection">Výsledek linq dotazu.</param>
        /// <param name="DtGrid">Datagrid, který chci updatnout.</param>
        private void UpdateNahled(IQueryable selection, DataGrid DtGrid)
        {
            DtGrid.ItemsSource = selection;
        }
        /// <summary>
        /// Updatne náhled určitého datagridu.
        /// </summary>
        /// <param name="DtGrid">Datagrid, který chci updatnout.</param>
        private void UpdateNahled(DataGrid DtGrid)
        {
            dynamic selection = null;
            switch(DtGrid.Name)
            {
                case "DataGridStud":
                    selection = from stud in db.StudentiLists
                                orderby stud.Prijmeni
                                select stud;
                    break;
                case "DataGridPredm":
                    selection = from predm in db.VypisPredmeties
                                orderby predm.Název
                                select predm;
                    break;
                case "DataGridStudDblClck":
                    selection = from hodn in db.VypisHodnoceniAlias
                                where hodn.Id == Int32.Parse(StudPredmId.Content.ToString())
                                orderby hodn.NazevPredm
                                select new { Zkratka = hodn.ZkratkaPredm, Předmět = hodn.NazevPredm, hodn.Hodnocení };
                    break;
                case "DataGridPredmDblClck":
                    selection = from hodn in db.VypisHodnoceniAlias
                                   where hodn.ZkratkaPredm == LblPredmStudZkr.Content.ToString()
                                   join stud in db.Studentis on hodn.Id equals stud.StudentID
                                   join fak in db.Fakulties on stud.Fakulta equals fak.FakultaID
                                   orderby hodn.Prijmeni
                                   select new { hodn.Id, hodn.Prijmeni, hodn.Jmeno, hodn.Hodnocení, stud.Rocnik, Fakulta = fak.FakultaNazev };
                    break;


            }
            DtGrid.ItemsSource = selection;
        }
       
        /// <summary>
        /// Funkce na přidání studenta do databáze. Kontroluje i, jestli tam už není se stejným ID.
        /// </summary>
        /// <param name="novy">Nový student,kterého přidám.</param>
        public void BtnAddStudent(Studenti novy)
        {
            var sel = from stud in db.Studentis
                      where stud.StudentID == novy.StudentID
                      select stud.StudentID;
            if (sel.ToList().Capacity >0)
            {
                MessageBox.Show("Student se stejným ID už existuje!");
                return;
            }
            db.Studentis.InsertOnSubmit(novy);
            db.SubmitChanges();
            UpdateNahled(DataGridStud);
            
        }
        /// <summary>
        /// Funkce na přidání nového předmětu do databáze. Kontroluje, zda tam u stejný není.
        /// </summary>
        /// <param name="novy">Instance nově vytvořeného předmětu.</param>
        public void BtnAddPredm(Predmety novy)
        {
            var sel = from predm in db.Predmeties
                      where predm.ZkratkaPredm == novy.ZkratkaPredm
                      select predm.ZkratkaPredm;
            if (sel.ToList().Capacity > 0)
            {
                MessageBox.Show("Stejný předmět už existuje!");
                return;
            }
            db.Predmeties.InsertOnSubmit(novy);
            db.SubmitChanges();
            UpdateNahled(DataGridPredm);

        }

        /// <summary>
        /// Vyhodí okno s přidáním hodnocení. Rozhodne se jestli budeme zadávat předmět, nebo jméno.
        /// </summary>
        /// <param name="sender">Button na kterej jsem klikl.</param>
        /// <param name="e"></param>
        private void BtnHodnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddHodnoc AddHodnocWindow = new AddHodnoc();
            Button source = sender as Button;
            switch (source.Name)
            {
                case "BtnStudPredmAdd":
                    AddHodnocWindow.CmbAddHodnStudName.Text = StudPredmJmeno.Content.ToString();
                    AddHodnocWindow.CmbAddHodnStudName.IsEnabled = false;
                    AddHodnocWindow.LblAddHodnocStudId.Content = StudPredmId.Content;
                    break;

                case "BtnPredmStudAdd":
                    AddHodnocWindow.CmbAddHodnName.Text = LblPredmStudPredm.Content.ToString();
                    AddHodnocWindow.CmbAddHodnocZkr.Text = LblPredmStudZkr.Content.ToString();
                    AddHodnocWindow.CmbAddHodnStudName.Width = 207;
                    AddHodnocWindow.BrdAddHodnStudName.Width = 207;
                    AddHodnocWindow.LblAddHodnocStudId.Visibility = System.Windows.Visibility.Hidden;
                    AddHodnocWindow.LblId.Visibility = System.Windows.Visibility.Hidden;
                    AddHodnocWindow.CmbAddHodnName.IsEnabled = false;
                    AddHodnocWindow.CmbAddHodnocZkr.IsEnabled = false;
                    break;

            }
            AddHodnocWindow.LoadComponents();
            AddHodnocWindow.ShowDialog();
        }

        /// <summary>
        /// Přidá nové hodnocení a updatuje zobrazení datagridu.
        /// </summary>
        /// <param name="nove">Nová instance hodnocení.</param>
        public void AddHodnoc(Hodnoceni nove)
        {
            var stejny = from hodnoceni in db.Hodnocenis
                         where hodnoceni.IdStud == nove.IdStud && hodnoceni.IdPredm == nove.IdPredm
                         select hodnoceni;
            if (stejny.ToList().Capacity > 0)
            {
                MessageBox.Show("Stejné hodnocení už existuje!");
                return;
            }
            db.Hodnocenis.InsertOnSubmit(nove);
            db.SubmitChanges();
            if (StudPredmId.Content.ToString() != "idečko")
            {
                UpdateNahled(DataGridStudDblClck);
            }
            UpdateNahled(DataGridPredm);
            UpdateNahled(DataGridPredmDblClck);
        }

        /// <summary>
        /// Vymaže studenta nebo studenty z databáze.
        /// </summary>
        private void BtnStudDel_Click(object sender, RoutedEventArgs e)
        {
            if (DebugCesta.Text != "")  //když nebudeme mít načtenej žádnej soubor, tak neprovedem nic
            {
                //otevřem yes/no messagebox 
                MessageBoxResult messageBoxResult = new MessageBoxResult();
                switch(DataGridStud.SelectedItems.Count)
                {
                    case 0: return;
                    case 1: messageBoxResult = System.Windows.MessageBox.Show("Chcete odebrat vybraného studenta?", "Odebrat studenta", System.Windows.MessageBoxButton.YesNo); break;
                    default: messageBoxResult = System.Windows.MessageBox.Show("Chcete odebrat vybrané studenty?", "Odebrat studenty", System.Windows.MessageBoxButton.YesNo); break;
                }
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var zvoleni = DataGridStud.SelectedItems.Cast<StudentiList>().ToArray();       //items typ IList, pretypuju na IEnumerable protože tam můžu dát .ToArray
                    
                    var zvol = from stud in db.Studentis    //vyberu všechny studenty
                               select stud;
                    var jejichpredm = from hodn in db.Hodnocenis        //vyberu všechny hodnocení
                                      select hodn;
                    List<Studenti> ZvoleniList = new List<Studenti>();      //do toho budu zapisovat co chci nakonec vymazat
                    List<Hodnoceni> HodnoceniList = new List<Hodnoceni>();
                    for (int i = 0; i < zvoleni.Length; i++)    //do pole vyfiltruju z selekce jen to co jsem označil
                    {
                        Studenti[] temp = zvol.Where(stud => stud.StudentID == zvoleni[i].ID).ToArray();    //jelikož .where podporuje .ToArray() tak si vytvořím Studenti[] temp i když by stačila proměnná ne pole,to pak zapíšu do listu
                        Hodnoceni[] temppred = jejichpredm.Where(stud => stud.IdStud == temp[0].Id).ToArray();  //stejně i s hodnocenimi, kdyby jich bylo víc, tak je do listu přidám všechny
                        ZvoleniList.Add(temp[0]);
                        foreach(var p in temppred)
                        {
                            HodnoceniList.Add(p);
                        }

                    }
                    db.Hodnocenis.DeleteAllOnSubmit(HodnoceniList); //nejdřív se smažou hodnocení,protože obsahují cizí klíče na studenty
                    db.Studentis.DeleteAllOnSubmit(ZvoleniList);    //až potom smažu studenty
                    try
                    {
                        db.SubmitChanges();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    UpdateNahled(DataGridStud);

                }
            }
        }
        private void BtnSubjDel_Click(object sender, RoutedEventArgs e)
        {

            MessageBoxResult messageBoxResult = new MessageBoxResult();
            switch (DataGridPredm.SelectedItems.Count)
            {
                case 0: return;
                case 1: messageBoxResult = System.Windows.MessageBox.Show("Chcete odebrat vybraný předmět?", "Odebrat předmět", System.Windows.MessageBoxButton.YesNo); break;
                default: messageBoxResult = System.Windows.MessageBox.Show("Chcete odebrat vybrané předměty?", "Odebrat předměty", System.Windows.MessageBoxButton.YesNo); break;
            }
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                VypisPredmety[] zvol = DataGridPredm.SelectedItems.Cast<VypisPredmety>().ToArray();
                List<Hodnoceni> HodnDelete = new List<Hodnoceni>(); //vysledne mazani nejdriv hodnoceni a pak vlastnich predmetu
                List<Predmety> PredmDelete = new List<Predmety>();
                foreach (var z in zvol)
                {
                    var zvolpredm = from predm in db.Predmeties     //předmět, u kterýho právě mažu hodnocení
                                    where predm.ZkratkaPredm == z.Zkratka
                                    select predm;
                    if (z.Počet > 0)    //když u sebe nebude mít žádné hodnocení, tak nemusím žádné mazat a tohle jednoduše přeskočím
                    {
                        var vsechnyhodn = from hodn in db.Hodnocenis    //všechny hodnocení
                                          select hodn;
                        Hodnoceni[] tempHodn = vsechnyhodn.Where(h => h.IdPredm == z.Zkratka).ToArray();    //všechny hodnocení právě mazaného předmětu
                        foreach (var t in tempHodn)
                        {
                            HodnDelete.Add(t);  //může být víc tak všechny přidám do listu
                        }
                    }
                    PredmDelete.Add(zvolpredm.ToArray().First());   //nakonec přidám samotnej předmět
                }
                db.Hodnocenis.DeleteAllOnSubmit(HodnDelete);
                db.Predmeties.DeleteAllOnSubmit(PredmDelete);
                db.SubmitChanges();
                UpdateNahled(DataGridPredm);
            }

        }
        private void BtnStudPredmDel_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridStudDblClck.SelectedItems.Count > 0)
            {
                MessageBoxResult messageBoxResult = new MessageBoxResult();
                switch (DataGridStud.SelectedItems.Count)
                {
                    case 0: return;
                    case 1: messageBoxResult = System.Windows.MessageBox.Show("Chcete odebrat vybrané hodnocení?", "Odebrat hodnocení", System.Windows.MessageBoxButton.YesNo); break;
                    default: messageBoxResult = System.Windows.MessageBox.Show("Chcete odebrat vybraná hodnocení?", "Odebrat hodnocení", System.Windows.MessageBoxButton.YesNo);

                        return;
                }
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    dynamic zvoleni = DataGridStudDblClck.SelectedItems;      // dám dynamic,protože var neumožňuje to ve foru zvoleni[i].Zkratka . Var se vyhodnocuje při překladu a tady se ještě neví co to bude, dynamicu je to jedno, kdyžtak hodí chybu až při tom řádku zvoleni[i].Zkratka
                    string[] zkratky = new string[zvoleni.Count];       //array zkratek předmětů
                    for (int i = 0; i < zvoleni.Count; i++)
                    {
                        zkratky[i] = zvoleni[i].Zkratka;
                    }
                    var zvol = from hodn in db.Hodnocenis       //všechny hodnocení
                               select hodn;
                    var studentquery = from stud in db.Studentis        //zvolím studenta
                                       where Int32.Parse(StudPredmId.Content.ToString()) == stud.StudentID
                                       select stud;
                    var studarr = studentquery.Cast<Studenti>().ToArray();  //složitej proces jak z IQueryable<> udělat Studnenti
                    Studenti student = studarr[0];
                    List<Hodnoceni> zvolene = new List<Hodnoceni>();        //vysledná kolekce kterou předám databázi na vymazání
                    for (int i = 0; i < zkratky.Length; i++)
                    {
                        Hodnoceni[] temp = zvol.Where(predm => (predm.IdStud == student.Id) && (predm.IdPredm == zkratky[i])).ToArray();     //opět získání typu z IQueryable
                        zvolene.Add(temp[0]);
                    }
                    db.Hodnocenis.DeleteAllOnSubmit(zvolene);
                    db.SubmitChanges();
                    //update datagridu
                    UpdateNahled(DataGridStudDblClck);
                    UpdateNahled(DataGridPredm);


                }
            }
        }
        private void BtnPredmStudDel_Click(object sender, RoutedEventArgs e)
        {
            //nešikovný
            MessageBoxResult messageBoxResult = new MessageBoxResult();
            switch (DataGridPredmDblClck.SelectedItems.Count)
            {
                case 0: return;
                case 1: messageBoxResult = System.Windows.MessageBox.Show("Chcete odebrat vybrané hodnocení?", "Odebrat hodnocení", System.Windows.MessageBoxButton.YesNo); break;
                default: messageBoxResult = System.Windows.MessageBox.Show("Chcete odebrat vybraná hodnocení?", "Odebrat hodnocení", System.Windows.MessageBoxButton.YesNo); break;
            }
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                dynamic zvolene = DataGridPredmDblClck.SelectedItems;
                var hodnoceni = from hodn in db.Hodnocenis
                                where hodn.IdPredm == LblPredmStudZkr.Content.ToString()
                                select hodn;
                var studenti = from stud in db.Studentis
                               select stud;
                List<Studenti> zvoleni = new List<Studenti>();
                foreach (var v in zvolene)
                {
                    int tempid = v.Id;
                    Studenti[] tempstud = studenti.Where(stud => stud.StudentID == tempid).ToArray<Studenti>();
                    zvoleni.Add(tempstud.First());
                }
                List<Hodnoceni> HodnDel = new List<Hodnoceni>();
                foreach (var z in zvoleni)
                {
                    Hodnoceni[] temp = hodnoceni.Where(hodn => hodn.IdStud == z.Id).ToArray<Hodnoceni>();
                    HodnDel.Add(temp.First());
                }

                db.Hodnocenis.DeleteAllOnSubmit(HodnDel);
                db.SubmitChanges();
                UpdateNahled(DataGridPredmDblClck);
                UpdateNahled(DataGridPredm);
            }
        }

        private void Row_DoubleClick(object sender, RoutedEventArgs e)
        {
            //chtěl bych editovat jen jeden sloupec,ale nějak to nejde přes readonly
            //je to nějaký pošahaný
            DataGridRow selRow = sender as DataGridRow;
            switch (selRow.Item.GetType().ToString())
            {
                case "Projekt_BOOP.StudentiList":
                    DataGridStudDblClck.Visibility = System.Windows.Visibility.Visible;
                    DataGridRow ZvolRow = sender as DataGridRow;
                    StudentiList ZvolStud = (StudentiList)ZvolRow.Item;
                    var hodnoc = from hodn in db.VypisHodnoceniAlias
                                 where hodn.Id == ZvolStud.ID
                                 orderby hodn.NazevPredm
                                 select new { Zkratka = hodn.ZkratkaPredm, Předmět = hodn.NazevPredm, hodn.Hodnocení };
                    UpdateNahled(hodnoc, DataGridStudDblClck);
                    StudPredmJmeno.Content = ZvolStud.Jmeno + " " + ZvolStud.Prijmeni;
                    StudPredmId.Content = ZvolStud.ID.ToString();
                    TxtStudPredmName.IsEnabled = true;  //zbytek je nabindovanej tak by to mělo fungovat
                    break;
                case "Projekt_BOOP.VypisPredmety":
                    DataGridRow ZvolRow1 = sender as DataGridRow;
                    VypisPredmety ZvolPredm = (VypisPredmety)ZvolRow1.Item;
                    LblPredmStudPredm.Content = ZvolPredm.Název;
                    LblPredmStudZkr.Content = ZvolPredm.Zkratka;
                    var studenti = from hodn in db.VypisHodnoceniAlias
                                   where hodn.ZkratkaPredm == ZvolPredm.Zkratka
                                   join stud in db.Studentis on hodn.Id equals stud.StudentID
                                   join fak in db.Fakulties on stud.Fakulta equals fak.FakultaID
                                   orderby hodn.Prijmeni
                                   select new { hodn.Id, hodn.Prijmeni,hodn.Jmeno, hodn.Hodnocení, stud.Rocnik,Fakulta = fak.FakultaNazev };
                    DataGridPredmDblClck.ItemsSource = studenti;
                    TxtPredmStudName.IsEnabled = true;
                    DataGridPredmDblClck.Visibility = System.Windows.Visibility.Visible;
                    break;
            }            
        }

        private void Grid_Leave(object sender, RoutedEventArgs e)   
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "BtnStudHodnZpet":
                    DataGridStudDblClck.Visibility = System.Windows.Visibility.Hidden;
                    TxtStudPredmName.IsEnabled = false; //zbytek nabindovanej
                    break;

                case "BtnPredmStudZpet":
                    DataGridPredmDblClck.Visibility = System.Windows.Visibility.Hidden;
                    TxtPredmStudName.IsEnabled = false;
                    break;
            }
        }
        
        /// <summary>
        /// Nastaví visibility a IsEnabled u obou subgridů u zobrazení předmětů u studentů i u zobrazení studentů u předmětů.
        /// </summary>
        private void Grids_Leave()
        {
            DataGridStudDblClck.Visibility = System.Windows.Visibility.Hidden;
            TxtStudPredmName.IsEnabled = false; //zbytek nabindovanej
            TxtPredmStudName.IsEnabled = false; 
            DataGridPredmDblClck.Visibility = System.Windows.Visibility.Hidden;

        }

        private void TxtStudName_KeyUp(object sender, KeyEventArgs e)
        {
            
            if(e.Key == Key.Enter)
            {
                TextBox box = sender as TextBox;
                switch (box.Name)
                {
                    case "TxtStudName" :
                    case "TxtStudID": BtnStudFind_Click(sender, e); break;
                    case "TxtPredmName" :
                    case "TxtPredmZkr": BtnSubjFind_Click(sender, e); break;
                    case "TxtStudPredmName": BtnStudPredmFind_Click(sender, e); break;
                }
            }
        }

        private void TxtStudID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //aby tam šly dávat jen čísla 
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        /// <summary>
        /// Vyvolá okno úprav.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridAlter(object sender, RoutedEventArgs e)
        {
            Alter AlterWindow = new Alter();
            DataGrid zamereny = WhichGridFocused(DataGridStud, DataGridPredm, DataGridStudDblClck, DataGridPredmDblClck);
            if(zamereny == null)
            {
                return;
            }
            dynamic item = zamereny.SelectedItem; //vic typu
            if(item is StudentiList)
            {
                AlterWindow.state = 1;
                AlterWindow.identifier = item.ID.ToString();
                AlterWindow.TxtAlterStudId.Text = item.ID.ToString();
                AlterWindow.TxtAlterStudName.Text = item.Jmeno;
                AlterWindow.TxtAlterStudPrijm.Text = item.Prijmeni;
                AlterWindow.CmbAlterFak.Text = item.Fakulta.ToString();
                AlterWindow.CmbAlterRoc.Text = item.Rocnik.ToString() + ".";
                AlterWindow.CmbAlterHodn.Visibility = System.Windows.Visibility.Hidden;
            }
            if(item is VypisPredmety)
            {
                return;
            }
            if(item.GetType().GetProperty("Id") != null)    //check jestli má property s tímto jménem protože nemůžu dát is AnounymusType
            {
                AlterWindow.state = 2;
                AlterWindow.identifier = LblPredmStudZkr.Content.ToString();
                AlterWindow.TxtAlterStudId.Visibility = System.Windows.Visibility.Visible;
                AlterWindow.TxtAlterStudName.IsEnabled = false;
                AlterWindow.TxtAlterStudId.Text = item.Id.ToString();
                AlterWindow.TxtAlterStudName.Text = item.Jmeno;
                AlterWindow.TxtAlterStudPrijm.Text = item.Prijmeni;
                AlterWindow.CmbAlterFak.Text = item.Fakulta.ToString();
                AlterWindow.CmbAlterRoc.Text = item.Rocnik.ToString() + ".";
                AlterWindow.CmbAlterHodn.Text = item.Hodnocení.ToString();
            }
            if (item.GetType().GetProperty("Předmět") != null)
            {
                AlterWindow.state = 3;
                AlterWindow.identifier = StudPredmId.Content.ToString();
                AlterWindow.TxtAlterStudId.Visibility = System.Windows.Visibility.Hidden;
                AlterWindow.TxtAlterPredmName.Visibility = System.Windows.Visibility.Visible;
                AlterWindow.TxtAlterPredmName.IsEnabled = false;
                AlterWindow.TxtAlterPredmZkr.IsEnabled = false;
                AlterWindow.TxtAlterPredmName.Text = item.Předmět.ToString();
                AlterWindow.TxtAlterPredmZkr.Text = item.Zkratka.ToString();
                AlterWindow.CmbAlterHodn.Text = item.Hodnocení.ToString();
            }
            AlterWindow.ShowDialog();
            UpdateNahled(zamereny);
            
        }
    
        /// <summary>
        /// Zjistí, na jakejgrid jsme klikli pravým.
        /// </summary>
        /// <param name="gridy">Výčet gridů které chcem testovat</param>
        /// <returns></returns>
        private DataGrid WhichGridFocused( params DataGrid [] gridy)
        {
            DataGrid ret = null;
            foreach(var v in gridy)
            {
                if (v.IsKeyboardFocusWithin)
                    return v;
            }
            return ret;
        }

    } //partial class end
}// namespace end
