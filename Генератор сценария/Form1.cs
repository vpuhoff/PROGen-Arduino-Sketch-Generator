using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace Генератор_сценария
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadScene("newscene");

        }

        [Serializable ]
        public class Script
        {
            public string id = Guid.NewGuid().ToString();
            public List<ActionBlock> Actions = new List<ActionBlock>();
            public ActionBlock RunAtOnce = new ActionBlock();
            public List<Sensor> Sensors = new List<Sensor>();
            public List<Trigger> Triggers = new List<Trigger>();
            [Serializable]
            public enum StateType{
                HIGH,
                LOW
            }
            [Serializable]
            public class Trigger
            {
                public string id = Guid.NewGuid().ToString();
                public string Name{ get; set; }
                public string Description{ get; set; }
                public string Event{ get; set; }
                public List<ActionBlock> Actions = new List<ActionBlock>();
                public string GetBlockCode()
                {
                    string actionsdata = "";
                    foreach (var item in Actions)
                    {
                        actionsdata += item.Name.Trim() + "();\r\n";
                    }
                    string codetemplate = File.ReadAllText("trigger.inf");
                    string code = codetemplate;
                    code = code.Replace("@description", Description);
                    code = code.Replace("@name", Name);
                    code = code.Replace("@event", Event);
                    code = code.Replace("@actions", actionsdata);
                    return code;
                }
            }
            [Serializable]
            public class Sensor
            {
                public string id = Guid.NewGuid().ToString();
                public int PinNumber { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public List<ActionBlock > Actions = new List<ActionBlock>();
                public StateType TrueState{ get; set; }
                public string GetBlockCode(){
                    string trueval;
                    if (TrueState == StateType.HIGH)
                    {
                        trueval = "HIGH";
                    }
                    else
                    {
                        trueval = "LOW";
                    }
                    string codetemplate = File.ReadAllText("sensor.inf");
                    string code = codetemplate;
                    code = code.Replace("@description", Description);
                    code = code.Replace("@name", Name);
                    code = code.Replace("@trueval", trueval);
                    code = code.Replace("@pinNumber", PinNumber.ToString ());
                    return code;
                }
            }
          
            [Serializable]
            public class ActionBlock
            {
                public string id = Guid.NewGuid().ToString();
                public List<string> IncludeList = new List<string>();
                public string Code;
                public string Name{ get; set; }
                public string Description{ get; set; }
                public string GetTestCode()
                {
                   string once = File.ReadAllText("runonce.inf");
                   string initcode = File.ReadAllText("init.inf");
                   string code = initcode;
                   code += "\r\nvoid setup() {\r\n" + once + "\r\n}\r\n";
                   code += "void loop() {\r\n" + Name + "();\r\n}";
                   code += GetBlockCode();
                   return code;
                }
                public string GetBlockCode()
                {
                    string codetemplate = File.ReadAllText("action.inf");
                    string code = codetemplate;
                    code = code.Replace("@description", Description);
                    code = code.Replace("@name", Name);
                    code = code.Replace("@id", id);
                    code = code.Replace("@code", Code);
                    return code;
                }
            }

        }

      Script Scene = new Script();
      public void SaveScene(string Filename)
        {
            //Сохраняем резервную копию
            if (File.Exists(Filename))
            {
                File.Copy(Filename, Filename + DateTime.Now.ToShortDateString() + ".bak", true);
            }
            BinaryFormatter bf = new BinaryFormatter();
            //откроем поток для записи в файл
            using (FileStream fs = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            using (GZipStream gz = new GZipStream(fs, CompressionMode.Compress, false))
            {
                bf.Serialize(gz, Scene);//сериализация
            }
        }
        public void LoadScene(string FileName)
        {
            if (!File.Exists(FileName))
            {
                Scene = new Script (); //указать тип нового объекта
                SaveScene("newscene");
            }
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress, false))
            {
                Scene = (Script )bf.Deserialize(gz); //указать тип объекта
            }
            RebindAll();

        }

        void RebindAll()
        {
            var source = new BindingSource();
            source.DataSource = Scene.Actions;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = source;

            var source2 = new BindingSource();
            source2.DataSource = Scene.Triggers;
            dataGridView2.AutoGenerateColumns = true;
            dataGridView2.DataSource = source2;

            var source3 = new BindingSource();
            source3.DataSource = Scene.Sensors;
            dataGridView3.AutoGenerateColumns = true;
            dataGridView3.DataSource = source3;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveScene("newscene");
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Scene.Actions.Add(new Script.ActionBlock());
            RebindAll();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Scene.Triggers.Add(new Script.Trigger());
            RebindAll();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Scene.Sensors.Add(new Script.Sensor());
            RebindAll();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
            
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {

            
           
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
           
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var a =((Script.ActionBlock)dataGridView1.SelectedRows[0].DataBoundItem);
                string s = a.GetTestCode();
                string tid = "temp";
                Directory.CreateDirectory(Application.StartupPath + "\\sketch_" + tid);
                string fileName = Application.StartupPath + "\\sketch_" + tid + "\\sketch_" + tid + ".ino";
                File.WriteAllText(fileName, s);
                try
                {
                    Process.Start(fileName);
                    if (MessageBox.Show("Обновить файл?", "Загрузка изменений...", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        string s2 = File.ReadAllText(fileName, Encoding.Default);
                        if (s2 != s)
                        {
                            int n1 = s2.IndexOf("//" + a.id);
                            int n2 = s2.IndexOf("//" + a.id, n1 + 4);
                            n1 += 2 + a.id.Length;
                            string s3 = s2.Substring(n1, n2 - n1);
                            if (MessageBox.Show(s3, "Сохранить изменения?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                a.Code = s3;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                } 
               
                
            }
        }

        private void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                var a = ((Script.Trigger)dataGridView2.SelectedRows[0].DataBoundItem);
                EditTrigger et = new EditTrigger();
                et.trigger = a;
                et.script = Scene;
                var source = new BindingSource();
                source.DataSource = a.Actions ;
                et.dataGridView1.AutoGenerateColumns = true;
                et.dataGridView1.DataSource = source;

                var source2 = new BindingSource();
                source2.DataSource = Scene.Actions;
                et.dataGridView2.AutoGenerateColumns = true;
                et.dataGridView2.DataSource = source2;
                et.ShowDialog();
                

            }
        }

        private void toolStripButton4_Click_1(object sender, EventArgs e)
        {
            Scene.RunAtOnce.Code = File.ReadAllText("runonce.inf");
            string initcode = File.ReadAllText("init.inf");
            string codetemplate = File.ReadAllText("program.inf");
            string loopcode = "";
            foreach (var item in Scene.Triggers)
            {
                loopcode += item.Name + "();\r\n";
            }
            string sensorscode = "";
            foreach (var item in Scene.Sensors)
            {
                sensorscode += item.GetBlockCode() + "\r\n";
            }
            string actionscode = "";
            foreach (var item in Scene.Actions)
            {
                actionscode += item.GetBlockCode() + "\r\n";
            }
            string triggerscode = "";
            foreach (var item in Scene.Triggers)
            {
                triggerscode += item.GetBlockCode() + "\r\n";
            }

            string code = codetemplate;
            code = code.Replace("@init", initcode);
            code = code.Replace("@runonce", Scene.RunAtOnce.Code);
            code = code.Replace("@loopcode", loopcode);
            code = code.Replace("@triggers", triggerscode);
            code = code.Replace("@sensors", sensorscode);
            code = code.Replace("@actions", actionscode);
 
            string tid = "ChipSketch";
            Directory.CreateDirectory(Application.StartupPath + "\\" + tid);
            string fileName = Application.StartupPath + "\\" + tid + "\\" + tid + ".ino";
            File.WriteAllText(fileName, code);
            Process.Start(fileName);
        }

        private void просмотрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count >0)
            {
                var act = (Script.ActionBlock)dataGridView1.SelectedRows[0].DataBoundItem;
                Form frm = new Form();
                RichTextBox txt = new RichTextBox();
                frm.Width = 640;
                frm.Height = 480;
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
                frm.Text = "Просмотр кода элемента";
                frm.Controls.Add(txt);
                txt.Dock = DockStyle.Fill;
                txt.Text = act.GetBlockCode();
                frm.Show();
            }
           
        }



       
    }
}
