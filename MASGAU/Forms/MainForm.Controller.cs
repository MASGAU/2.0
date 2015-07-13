using System;
using Eto.Forms;
using Eto.Drawing;
using GameSaveInfo.Data;

namespace MASGAU {
    /// <summary>
    /// Your application's main form
    /// </summary>
    public partial class MainForm {
        Core core;

        public MainForm() {
            this.Construct();
            cmdButton.Executed += clickMe_Executed;

            var progress = new Progress<ProcessStatus>(status => {
                Application.Instance.Invoke((Action)delegate() {
                if (status.Value.HasValue) {
                    prgBar.Indeterminate = false;
                    prgBar.Value = status.Value.Value;
                } else {
                    prgBar.Indeterminate = true;
                }
                prgBar.MinValue = status.MinValue;
                prgBar.MaxValue = status.MaxValue;
                lblContent.Text = status.Message;
            });
            });


            core = new Core(progress);
        }





        async void clickMe_Executed(object sender, EventArgs e) {
            lblContent.Text = await core.Initialize();
        }



    }
}
