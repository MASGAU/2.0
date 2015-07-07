using System;
using Eto.Forms;
using Eto.Drawing;
using GameSaveInfo.Data;

namespace MASGAU
{
	/// <summary>
	/// Your application's main form
	/// </summary>
	public partial class MainForm
	{
		public MainForm()
		{
            this.Construct();
            cmdButton.Executed += clickMe_Executed;

		}

        void clickMe_Executed(object sender, EventArgs e) {
            
            MASGAU masgau = new MASGAU();
            lblContent.Text = masgau.Init();
            


        }
	}
}
