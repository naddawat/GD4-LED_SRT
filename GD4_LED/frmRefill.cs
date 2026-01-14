using GD4_LED.cls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GD4_LED
{
    public partial class frmRefill : Form
    {
        clsvariable clsvariable = clsvariable.Instance;
        clsQuery _query = new clsQuery();
        public frmRefill()
        {
            InitializeComponent();
        }

        private void frmRefill_Load(object sender, EventArgs e)
        {
            DataTable dt_GetRefill = new DataTable();
            dt_GetRefill = _query.GetRefill();
            if (dt_GetRefill.Rows.Count > 0)
            {
                dataGridView1.DataSource = dt_GetRefill;
            }
        }
    }
}
