using System;
using Eto.Forms;
using Eto.Drawing;
using GameSaveInfo.Data;

namespace MASGAU
{
	/// <summary>
	/// Your application's main form
	/// </summary>
	public partial class MainForm : Form
	{

        private Command cmdButton;
        private Label lblContent;
        private ProgressBar prgBar;

		private void Construct()
		{
			Title = "My Eto Form";
			ClientSize = new Size(400, 350);

            lblContent = new Label { Text = "Hello World!" };
            prgBar = new ProgressBar();

			// scrollable region as the main content
			Content = new Scrollable
			{
				// table with three rows
				Content = new TableLayout(
					null,
					// row with three columns
					new TableRow(null, lblContent, null),
                    new TableRow(null, prgBar, null)
				)
			};

			// create a few commands that can be used for the menu and toolbar
            cmdButton = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "About my app...");

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new ButtonMenuItem { Text = "&File", Items = { cmdButton } },
					// new ButtonMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
					// new ButtonMenuItem { Text = "&View", Items = { /* commands/items */ } },
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
					new ButtonMenuItem { Text = "&Preferences..." },
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};

			// create toolbar			
            ToolBar = new ToolBar { Items = { cmdButton } };
		}


	}
}
