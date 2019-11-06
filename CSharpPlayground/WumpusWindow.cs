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

namespace CSharpPlayground
{
    public partial class WumpusWindow : Form
    {
        WumpusGameManager manager;

        public WumpusWindow(WumpusGameManager manager)
        {
            this.manager = manager;
            InitializeComponent();
        }

        [STAThread]
        public static WumpusWindow StartWindow(WumpusGameManager manager)
        {
            WumpusWindow window = new WumpusWindow(manager);
            Application.EnableVisualStyles();
            Application.Run(window);
            return window;
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
                    consoleInput.Text = manager.GetCommandHistoryNext(-1);
                    break;
                case Keys.PageUp:
                    consoleInput.Text = manager.GetCommandHistoryLast();
                    break;
                case Keys.Down:
                    consoleInput.Text = manager.GetCommandHistoryNext(1);
                    break;
                case Keys.PageDown:
                    manager.ResetCommandIndex();
                    consoleInput.Text = string.Empty;
                    break;
            }
        }
    }
}
