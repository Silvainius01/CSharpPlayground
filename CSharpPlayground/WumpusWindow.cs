using CSharpPlayground.Wumpus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace CSharpPlayground
{
    public partial class WumpusWindow : Form
    {
        WumpusGameManager manager;

        delegate void StringDelegate(string str);
        delegate void StringTextBoxDelegate(string str, TextBox textBox);
        delegate void TextBoxDelegate(TextBox textBox);

        public WumpusWindow(WumpusGameManager manager)
        {
            this.manager = manager;
            InitializeComponent();
        }

        public static WumpusWindow StartWindow(WumpusGameManager manager)
        {
            WumpusWindow window = new WumpusWindow(manager);
            Thread formThread = new Thread(new ParameterizedThreadStart(Run));
            formThread.Start(window);
            return window;
        }

        private static void Run(object form)
        {
            Application.EnableVisualStyles();
            Application.Run((Form)form);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public string GetCommandLineInput()
        {
            if (consoleInput.Focus())
            {
                string retval = consoleInput.Text;
                consoleInput.ResetText();
                return retval;
            }
            return null;
        }

        public void ConsoleInput_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    manager.ReceiveCommand(consoleInput.Text);
                    consoleInput.ResetText();
                    break;
                case Keys.Up:
                    consoleInput.Clear();
                    consoleInput.AppendText(manager.GetCommandHistoryNext(1));
                    break;
                case Keys.PageUp:
                    consoleInput.Clear();
                    consoleInput.AppendText(manager.GetCommandHistoryLast());
                    break;
                case Keys.Down:
                    consoleInput.Clear();
                    consoleInput.AppendText(manager.GetCommandHistoryNext(-1));
                    break;
                case Keys.PageDown:
                    consoleInput.Clear();
                    manager.ResetCommandIndex();
                    break;
            }
        }
        public void ConsoleInput_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Up)
                consoleInput.SelectionStart = consoleInput.TextLength;
        }
        private void ConsoleInput_Enter(object sender, System.EventArgs e)
        {
            if(consoleInput.Text.Length > 0)
            {
                manager.ReceiveCommand(consoleInput.Text);
                consoleInput.Clear();
            }
        }

        public static void AppendToTextBoxSafe(string msg, TextBox textBox)
        {
            if (textBox.InvokeRequired)
            {
                var d = new StringTextBoxDelegate(AppendToTextBoxSafe);
                textBox.Invoke(d, msg, textBox);
            }
            else textBox.AppendText(msg);
        }
        public static void ClearTextSafe(TextBox textBox)
        {
            if (textBox.InvokeRequired)
            {
                var d = new TextBoxDelegate(ClearTextSafe);
                textBox.Invoke(d);
            }
            else textBox.Clear();
        }
        public static void SetTextSafe(string msg, TextBox textBox)
        {
            if (textBox.InvokeRequired)
            {
                var d = new StringTextBoxDelegate(AppendToTextBoxSafe);
                textBox.Invoke(d, msg, textBox);
            }
            else
            {
                textBox.Clear();
                textBox.AppendText(msg);
            }
        }
    }
}
