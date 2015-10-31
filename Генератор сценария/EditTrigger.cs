using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Генератор_сценария
{
    public partial class EditTrigger : Form
    {
        public Генератор_сценария.Form1.Script script = new Генератор_сценария.Form1.Script();
        public Генератор_сценария.Form1.Script.Trigger trigger = new Генератор_сценария.Form1.Script.Trigger();
        public EditTrigger()
        {
            InitializeComponent();
        }

        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count >0)
            {
               if (dataGridView1.SelectedRows.Count >0)
                {
                    var a = ((Генератор_сценария.Form1.Script.ActionBlock)dataGridView2.SelectedRows[0].DataBoundItem);
                    var b = ((Генератор_сценария.Form1.Script.ActionBlock)dataGridView1.SelectedRows[0].DataBoundItem);
                    trigger.Actions.Insert(trigger.Actions.IndexOf(b), a);
                    var source = new BindingSource();
                    source.DataSource = trigger.Actions;
                    dataGridView1.AutoGenerateColumns = true;
                    dataGridView1.DataSource = source;
                }
               else
               {
                   var a = ((Генератор_сценария.Form1.Script.ActionBlock)dataGridView2.SelectedRows[0].DataBoundItem);
                   trigger.Actions.Add ( a);
                   var source = new BindingSource();
                   source.DataSource = trigger.Actions;
                   dataGridView1.AutoGenerateColumns = true;
                   dataGridView1.DataSource = source;
               }
            }
            
        }
    }
}
