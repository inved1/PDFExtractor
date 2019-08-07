using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFExtractor.Forms
{
    public partial class Form1 : Form
    {

        private PDFExtractor.@class.Extractor Extractor;

        private string _currentDirectory;

        public Form1()
        {
            InitializeComponent();
            this.btnExec.Enabled = false;
            this.tabControl1.TabPages.Clear();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Extractor = new @class.Extractor();

        }

        private void btnReadDir_Click(object sender, EventArgs e)
        {
            try
            {

                FolderBrowserDialog fbd = new FolderBrowserDialog()
                {

                };

                
                if(fbd.ShowDialog() == DialogResult.OK)
                {
                    _currentDirectory = fbd.SelectedPath;
                    this.txtDir.Text = _currentDirectory;
                    this.btnExec.Enabled = true;
                    fnReadFiles();
                }


            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            };
        }


        private void fnReadFiles()
        {
            this.txtFiles.Text = "";

            string[] files = System.IO.Directory.GetFiles(_currentDirectory);
            foreach(string file in files)
            {
                this.txtFiles.Text += file + System.Environment.NewLine;
            }
        }

        private void btnExec_Click(object sender, EventArgs e)
        {

            //clear it 
            Extractor.PDFFiles = new List<string>();

            foreach(string s in txtFiles.Text.Split(new[]{ System.Environment.NewLine}, StringSplitOptions.None))
            {
                if (s.EndsWith("pdf"))
                {
                    Extractor.PDFFiles.Add(s);
                    System.Console.WriteLine(String.Format("... add file {0}", s));
                }

            }
            Extractor.Run();


            foreach (KeyValuePair<string,DataTable> kv in Extractor.OutputResult)
            {
                TabPage tabPage = new TabPage(kv.Key);
                DataGridView dgv = new DataGridView();
                dgv.DataSource = kv.Value;

                dgv.Dock = DockStyle.Fill;
                tabPage.Controls.Add(dgv);
                


                this.tabControl1.TabPages.Add(tabPage);
            }
           
            
        }
    }
}
