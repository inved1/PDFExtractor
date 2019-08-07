using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iTextSharp.text.pdf;


namespace PDFExtractor.@class
{

    

    public class Extractor
{

        public List<string> PDFFiles { get { return _myPDFFiles; } set { _myPDFFiles = value; } }

        public Dictionary<string,System.Data.DataTable> OutputResult { get { return _myDictDT; } }

        List<string> _myPDFFiles;

        Dictionary<string, System.Data.DataTable> _myDictDT; //Horse and datatable with result
        Dictionary<string, Dictionary<string, string>> _myDictPages; //filename and key-value-pairs
        List<string> _KeysToSearch;

        public Extractor()
        {
            System.Console.WriteLine("Init class");

            _myDictPages = new Dictionary<string, Dictionary<string, string>>();
            _myDictDT = new Dictionary<string, System.Data.DataTable>();

            _KeysToSearch = new List<string>();
            _myPDFFiles = new List<string>();

        }

        public void Run()
        {
            //this gets the data into _myDictPages
            RunExtractor();

        }


        private System.Data.DataTable createDT()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("BloodID");
            dt.Columns.Add("Horse");
            dt.Columns.Add("Date");
            dt.Columns.Add("Erythrozyten");
            dt.Columns.Add("Haemoglobin");
            dt.Columns.Add("Haematokrit");
            dt.Columns.Add("MCV");
            dt.Columns.Add("MCH");
            dt.Columns.Add("MCHC");
            dt.Columns.Add("Leukozyten");
            dt.Columns.Add("Neutrophile %");
            dt.Columns.Add("Neutrophile");
            dt.Columns.Add("Eosinophile %");
            dt.Columns.Add("Eosinophile");
            dt.Columns.Add("Basophile %");
            dt.Columns.Add("Basophile");
            dt.Columns.Add("Monozyten %");
            dt.Columns.Add("Monozyten");
            dt.Columns.Add("Lymphozyten %");
            dt.Columns.Add("Lymphozyten");
            dt.Columns.Add("Amylase");
            dt.Columns.Add("Alk._Phosphatase(altersab.)");
            dt.Columns.Add("GLDH(Glutamat - Dehydrogenase)");
            dt.Columns.Add("GOT(ASAT)");
            dt.Columns.Add("GGT(G - Glutamyl - Transpeptase)");
            dt.Columns.Add("LDH(Lactat - Dehydrogenase)");
            dt.Columns.Add("Creatin - Kinase");
            dt.Columns.Add("Bilirubin_gesamt");
            dt.Columns.Add("Bilirubin_direkt");
            dt.Columns.Add("Harnstoff");
            dt.Columns.Add("Kreatinin");
            dt.Columns.Add("Eisen");
            dt.Columns.Add("Calcium");
            dt.Columns.Add("Natrium");
            dt.Columns.Add("Kalium");
            dt.Columns.Add("Chlorid");
            dt.Columns.Add("Phosphor_anorganisch");
            dt.Columns.Add("Magnsium");
            dt.Columns.Add("Eiweiss(Protein)_total");
            dt.Columns.Add("Albumin");
            dt.Columns.Add("Cholesterin");
            dt.Columns.Add("Glucose");
            dt.Columns.Add("Gallensaeure");
            dt.Columns.Add("SAA");
            dt.Columns.Add("Selen");
            dt.Columns.Add("Kupfer");
            dt.Columns.Add("Zink");
            dt.Columns.Add("Triglyzeride");
            dt.Columns.Add("Magen - Darmnematoden");
            dt.Columns.Add("Kokzidienoozysten");
            dt.Columns.Add("Bandwurmeier / glieder");
            dt.Columns.Add("Culex(Stechmuecken)");
            dt.Columns.Add("Culicodes(Gnitze)");
            dt.Columns.Add("Simulium_sp.(Kriebelmuecken)");
            dt.Columns.Add("Stomoxys_c.(Wadenstecher)");
            dt.Columns.Add("Tabanus(Bremsen)");
            dt.Columns.Add("Visit");


            _KeysToSearch.Clear();
            foreach ( System.Data.DataColumn c in dt.Columns)
            {
                _KeysToSearch.Add(c.ColumnName);
            }


            return dt;

        }


        private void RunExtractor()
        {

            _myDictPages.Clear();
            _myDictDT.Clear();

            foreach (string file in _myPDFFiles)
            {
                var reader = new PdfReader(file);
                var lst = new List<string>();

                _myDictPages.Add(System.IO.Path.GetFileName(file), new Dictionary<string, string>());
                

                var stringslist = new List<string>();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {

                    var streambytes = reader.GetPageContent(i);
                    var tokenizer = new PRTokeniser(new RandomAccessFileOrArray(streambytes));
                    
                    while (tokenizer.NextToken())
                    {
                        if (tokenizer.TokenType == PRTokeniser.TK_STRING)
                        {
                            stringslist.Add(tokenizer.StringValue.Replace("ä","ae").Replace("ü", "ue").Replace("ö", "oe"));
                        }
                    }
                    
                }
                // here's the logic
                // 1. get data from stringbuilder to _myDictPages.value
                // 2. get data from key.value pair to datatable inside _myDictDT



                //1.
                _myDictPages[System.IO.Path.GetFileName(file)] = ReadPlainData(stringslist);


                //2.
                

            }

            _myDictDT= FuseDictionaries(_myDictPages);


            System.Console.WriteLine("Finished");

            //here add data from stringbuilder to dict
            
        }

        /// <summary>
        /// I actually do not know how many dicts I get - I need to fuse them by auftragsnummer
        /// 
        /// </summary>
        /// <param name="dicts"></param>
        /// <returns></returns>
        Dictionary<string, System.Data.DataTable> FuseDictionaries(Dictionary<string, Dictionary<string,string>> dicts)
        {
            Dictionary<string, System.Data.DataTable> ret = new Dictionary<string, System.Data.DataTable>();

            foreach(Dictionary<string,string> dict in dicts.Values)
            {
                var tmpAuftragsnummer = dict["Auftragsnummer:"];
                if (!ret.ContainsKey(tmpAuftragsnummer))
                {
                    ret.Add(tmpAuftragsnummer, createDT());
                    ret[tmpAuftragsnummer].Rows.Add();
                }
                System.Data.DataTable tmpDataTable = ret[tmpAuftragsnummer];

                foreach(KeyValuePair<string,string> kv in dict)
                {
                    string translateKey = kv.Key;
                    if (kv.Key == "Auftragsnummer:") translateKey = "BloodID";
                    if (kv.Key == "Pferd") translateKey = "Horse";
                    if (kv.Key == "Befundungsdatum:") translateKey = "Date";
                    tmpDataTable.Rows[0][translateKey] = kv.Value;
                }
                

                //tmpDataTable.Rows[0].
                


            }


            return ret;
        }

        Dictionary<string,string> ReadPlainData(List<string> lst)
        {
            Dictionary<string, string> retDict = new Dictionary<string, string>();
            createDT(); ; //call to create the keys
            foreach(string key in _KeysToSearch)
            {
                try
                {
                    for(int i = 0; i < lst.Count; i++)
                    {
                        string translateKey = key;

                        if (key == "BloodID") translateKey = "Auftragsnummer:";
                        if (key == "Horse") translateKey = "Pferd";
                        if (key == "Date") translateKey = "Befundungsdatum:";

                        if (translateKey.Trim() == lst[i].Trim())
                        {
                            var tmpvalue = lst[i + 1];
                            retDict.Add(translateKey, tmpvalue);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine(String.Format(".... {0}", string.Join(" ",lst.ToArray())));

            return retDict;
        }


}
}
