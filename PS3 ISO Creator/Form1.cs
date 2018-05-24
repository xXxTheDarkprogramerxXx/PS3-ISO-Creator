using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using IsoCreator;
using BER.CDCat.Export;

namespace PS3_ISO_Creator
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            m_creator = new IsoCreator.IsoCreator();
            m_creator.Progress += new ProgressDelegate(creator_Progress);
            m_creator.Finish += new FinishDelegate(creator_Finished);
            m_creator.Abort += new AbortDelegate(creator_Abort);

        }

        string version = "1.1";
        List<string> Checker = new List<string>();

        private void statusgood(string log)
        {
            //Create For Log Window
            lblStatus.Text =(log +" -Status OK");
            //Do Events
            Application.DoEvents();
        }

        private void statusbad(string log)
        {
            lblStatus.Text = (log + " -Status BAD");
            //Do Events
            Application.DoEvents();
        }

        private bool datachecker(string decryptedimagepath)
        {
            if(Directory.Exists(decryptedimagepath+@"\\PS3_GAME")==false)
            {
                statusbad("PS3_GAME");
            }
            else if (Directory.Exists(decryptedimagepath + @"\\PS3_GAME") == true)
            {
                statusgood("PS3_GAME");
            }
            if (Directory.Exists(decryptedimagepath + @"\\PS3_GAME\\USRDIR") == false)
            {
                statusbad("USRDIR");
            }
            else if (Directory.Exists(decryptedimagepath + @"\\PS3_GAME\\USRDIR") == true)
            {
                statusgood("USRDIR");
            }
            if (File.Exists(decryptedimagepath + @"\\PS3_GAME\\USRDIR\\EBOOT.BIN") == false)
            {
                statusbad("EBOOT.BIN");
            }
            else if (File.Exists(decryptedimagepath + @"\\PS3_GAME\\USRDIR\\EBOOT.BIN") == true)
            {
                statusgood("EBOOT.BIN");
            }
            if (File.Exists(decryptedimagepath + @"\\PS3_GAME\\PARAM.SFO") == false)
            {
                statusbad("PARAM.SFO");
            }
            else if (File.Exists(decryptedimagepath + @"\\PS3_GAME\\PARAM.SFO") == true)
            {
                statusgood("PARAM.SFO");
            }

            return true;
        }


        private Thread m_thread = null;
        private IsoCreator.IsoCreator m_creator = null;

        private delegate void SetLabelDelegate(string text);
        private delegate void SetNumericValueDelegate(int value);

        void creator_Abort(object sender, AbortEventArgs e)
        {
            MessageBox.Show(e.Message, "Abort", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            MessageBox.Show("The ISO creating process has been stopped.", "Abort", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            button2.Invoke(new Action(() => button2.Enabled = false)); 
            progressBar.Invoke(new Action(() => progressBar.Value = 0));
            progressBar.Invoke(new Action(() => progressBar.Maximum = 0));
            labelStatus.Invoke(new Action(() => labelStatus.Text = "Process not started"));
        }

        void creator_Finished(object sender, FinishEventArgs e)
        {
            MessageBox.Show(e.Message, "Finish", MessageBoxButtons.OK, MessageBoxIcon.Information);
            button2.Invoke(new Action(() => button2.Enabled = false)); 
            progressBar.Invoke(new Action(() => progressBar.Value = 0));
            labelStatus.Invoke(new Action(() => labelStatus.Text = "Process not started"));
            lblStatus.Invoke(new Action(()=> lblStatus.Text = ""));
        }

        private void SetLabelStatus(string text)
        {
            this.labelStatus.Text = text;
            this.labelStatus.Refresh();
        }

        private void SetProgressValue(int value)
        {
            this.progressBar.Value = value;
        }

        private void SetProgressMaximum(int maximum)
        {
            this.progressBar.Maximum = maximum;
        }

        void creator_Progress(object sender, ProgressEventArgs e)
        {
            if (e.Action != null)
            {
                if (!this.InvokeRequired)
                {
                    this.SetLabelStatus(e.Action);
                }
                else
                {
                    this.Invoke(new SetLabelDelegate(SetLabelStatus), e.Action);
                }
            }

            if (e.Maximum != -1)
            {
                if (!this.InvokeRequired)
                {
                    this.SetProgressMaximum(e.Maximum);
                }
                else
                {
                    this.Invoke(new SetNumericValueDelegate(SetProgressMaximum), e.Maximum);
                }
            }

            if (!this.InvokeRequired)
            {
                progressBar.Value = (e.Current <= progressBar.Maximum) ? e.Current : progressBar.Maximum;
            }
            else
            {
                int value = (e.Current <= progressBar.Maximum) ? e.Current : progressBar.Maximum;
                this.Invoke(new SetNumericValueDelegate(SetProgressValue), value);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = dialog.SelectedPath;
                textBoxVolumeName.Text = new DirectoryInfo(dialog.SelectedPath).Name + ".iso";


                DialogResult result = MessageBox.Show("Do you want to create an ISO of this folder for ps3 ?", "PS3ISO", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    DialogResult savedialog = MessageBox.Show("Do you want to save the .iso in a custom location?", "PS3ISO", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (savedialog == DialogResult.No)
                    {
                        if (m_thread == null || !m_thread.IsAlive)
                        {
                            if (datachecker(txtPath.Text) == true)
                            {
                                lblStatus.ForeColor = Color.Green;
                                lblStatus.Text =("Passed");
                            }
                            else
                            {
                                lblStatus.ForeColor = Color.Red;
                                lblStatus.Text =("Failed");
                            }
                            if (textBoxVolumeName.Text.Trim() != "")
                            {
                                m_thread = new Thread(new ParameterizedThreadStart(m_creator.Folder2Iso));
                                m_thread.Start(new IsoCreator.IsoCreator.IsoCreatorFolderArgs(txtPath.Text, Application.StartupPath + "\\" + textBoxVolumeName.Text, textBoxVolumeName.Text));

                                button2.Enabled = true;
                            }
                            else
                            {
                                MessageBox.Show("Please insert a name for the volume", "No volume name", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            }
                        }
                    }
                    else if (result == DialogResult.Yes)
                    {
                        string savepath = string.Empty;
                        string savename = string.Empty;
                        SaveFileDialog saveme = new SaveFileDialog();
                        saveme.Filter = "PS3 Disk Images (*.iso)|*.iso";
                        if (saveme.ShowDialog() == DialogResult.OK)
                        {
                            savepath = saveme.FileName;
                            savename = new System.IO.FileInfo(saveme.FileName).Name;
                            textBoxVolumeName.Text = savename;
                            tabControl1.SelectedIndex = 3;
                            if (datachecker(txtPath.Text) == true)
                            {
                                lblStatus.ForeColor = Color.Green;
                                lblStatus.Text =("Passed");
                            }
                            else
                            {
                                lblStatus.ForeColor = Color.Red;
                                lblStatus.Text =("Failed");
                            }
                        }
                        if (m_thread == null || !m_thread.IsAlive)
                        {
                            if (textBoxVolumeName.Text.Trim() != "")
                            {
                                m_thread = new Thread(new ParameterizedThreadStart(m_creator.Folder2Iso));
                                m_thread.Start(new IsoCreator.IsoCreator.IsoCreatorFolderArgs(txtPath.Text, savepath, savename));

                                button2.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_creator != null && m_thread != null && m_thread.IsAlive)
            {
                m_thread.Abort();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblVersion.Text = version;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_creator != null && m_thread != null && m_thread.IsAlive)
            {
                m_thread.Abort();
                button2.Enabled = false;
            }
        }

    }
}
