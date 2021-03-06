﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AlreySolutions.Class;
using AlreySolutions.Class.Load;

namespace AlreySolutions.Reports
{
    public partial class frmSCashTransReport : Form
    {
        List<clsUsers> lstUsers = new List<clsUsers>();
        List<clsLoadAccount> m_lstAccountInfo = new List<clsLoadAccount>();

        public frmSCashTransReport()
        {
            InitializeComponent();
        }

        private void frmSCashTransReport_Load(object sender, EventArgs e)
        {
            clsThemes.ApplyTheme(this, new clsThemes.ThemeSettings(Properties.Settings.Default.Theme));
            cboCashier.Items.Add("All");

            lstUsers = clsUsers.GetUsers();
            foreach (clsUsers str in lstUsers)
            {
                cboCashier.Items.Add(str.UserName);
            }
            cboCashier.SelectedIndex = 0;
            dtPickEnd.Value = dtPickStart.Value.AddDays(1);
        }

        private void frmSCashTransReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void dtPickStart_ValueChanged(object sender, EventArgs e)
        {
            if (dtPickStart.Value >= dtPickEnd.Value) dtPickEnd.Value = dtPickStart.Value.AddDays(1);
        }

        private void dtPickEnd_ValueChanged(object sender, EventArgs e)
        {
            if (dtPickEnd.Value <= dtPickStart.Value) dtPickStart.Value = dtPickEnd.Value.AddDays(-1);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string cashier = cboCashier.SelectedItem.ToString();
            clsUsers SelectedUser = lstUsers.Find(x => x.UserName == cboCashier.Text);
            int userid = SelectedUser != null ? SelectedUser.UserId : 0;

            if (cashier == "All") cashier = "";
            SearchReloadHistory(dtPickStart.Value, dtPickEnd.Value, userid, txtCustomer.Text.Trim());
        }
        private void SearchReloadHistory(DateTime startdate, DateTime enddate, int cashier, string customer)
        {
            dbConnect con = new dbConnect();
            List<clsSmartCashTransaction> lstLoadHistory = clsSmartCashTransaction.GetGcashTransactionReport(startdate, enddate, cashier, customer);
            dgvTrans.Rows.Clear();
            if (lstLoadHistory.Count > 0)
            {
                foreach (clsSmartCashTransaction hist in lstLoadHistory)
                {
                    AddItemToGrid(hist);
                }
            }

        }
        private void AddItemToGrid(clsSmartCashTransaction hist)
        {
            int rowidx = dgvTrans.Rows.Add();
            dgvTrans.Rows[rowidx].Cells[0].Value = hist.TransDate;
            dgvTrans.Rows[rowidx].Cells[1].Value = hist.AccountName;
            dgvTrans.Rows[rowidx].Cells[2].Value = hist.RefNum;
            dgvTrans.Rows[rowidx].Cells[3].Value = hist.SenderName;
            dgvTrans.Rows[rowidx].Cells[4].Value = hist.SenderContact;
            dgvTrans.Rows[rowidx].Cells[5].Value = hist.RecipientName;
            dgvTrans.Rows[rowidx].Cells[6].Value = clsSmartCashTransaction.GetTransType(hist.TransType);
            dgvTrans.Rows[rowidx].Cells[7].Value = clsSmartCashTransaction.GetPaymentMode(hist.PaymentMode);
            dgvTrans.Rows[rowidx].Cells[8].Value = hist.TransAmount;
            dgvTrans.Rows[rowidx].Cells[9].Value = hist.SvcFeeAmount;


            double transamt = clsSmartCashTransaction.GetTransAmount(hist.TransType, hist.PaymentMode, hist.TransAmount, hist.SvcFeeAmount);
            if (transamt < 0) dgvTrans.Rows[rowidx].Cells[10].Value = string.Format("({0})", Decimal.Negate(Decimal.Parse(transamt.ToString())));
            else dgvTrans.Rows[rowidx].Cells[10].Value = string.Format("{0}", transamt);

            dgvTrans.Rows[rowidx].Cells[11].Value = hist.UserName;
        }
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvTrans.Rows.Count == 0)
            {
                MessageBox.Show("Nothing to export.", "Export To Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            clsExportToExcel export = new clsExportToExcel();
            SaveFileDialog savedlg = new SaveFileDialog();
            savedlg.Filter = "Excel File (*.xls)|*.xls";
            savedlg.InitialDirectory = Application.StartupPath;
            if (savedlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string columns = "";
                foreach (DataGridViewColumn col in dgvTrans.Columns)
                {
                    columns += col.HeaderText + (col != dgvTrans.Columns[dgvTrans.Columns.Count - 1] ? "\t" : "");
                }
                List<string> lstValues = new List<string>();
                foreach (DataGridViewRow row in dgvTrans.Rows)
                {
                    string val = "";
                    for (int ctr = 0; ctr < dgvTrans.Columns.Count; ctr++)
                    {
                        val += row.Cells[ctr].Value.ToString() + (ctr != dgvTrans.Columns.Count - 1 ? "\t" : "");
                    }
                    lstValues.Add(val);
                }
                export.SaveToExcel(savedlg.FileName, columns, lstValues);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
