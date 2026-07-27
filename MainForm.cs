using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace DLLFixer
{
    public partial class MainForm : Form
    {
        private DLLScanner scanner;
        private DLLRepairer repairer;
        private Logger logger;

        public MainForm()
        {
            InitializeComponent();
            scanner = new DLLScanner();
            repairer = new DLLRepairer();
            logger = new Logger();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = "DLL Error Fixer";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            logger.Log("Application started");
        }

        private async void btnScan_Click(object sender, EventArgs e)
        {
            logger.Log("Starting system scan...");
            btnScan.Enabled = false;
            btnRepair.Enabled = false;
            progressBar.Style = ProgressBarStyle.Marquee;

            try
            {
                var results = await Task.Run(() => scanner.ScanSystem());
                DisplayResults(results);
                logger.Log($"Scan completed. Found {results.Count} issues.");
            }
            catch (Exception ex)
            {
                logger.Log($"Error during scan: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnScan.Enabled = true;
                btnRepair.Enabled = true;
                progressBar.Style = ProgressBarStyle.Continuous;
            }
        }

        private async void btnRepair_Click(object sender, EventArgs e)
        {
            if (listViewResults.Items.Count == 0)
            {
                MessageBox.Show("Please run a scan first.", "No Issues Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("This will attempt to repair DLL issues. Continue?", "Confirm Repair", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            logger.Log("Starting repair process...");
            btnScan.Enabled = false;
            btnRepair.Enabled = false;
            progressBar.Style = ProgressBarStyle.Marquee;

            try
            {
                var repairResults = await Task.Run(() => repairer.RepairDLLs());
                logger.Log($"Repair completed. {repairResults.Item1} issues fixed, {repairResults.Item2} failed.");
                MessageBox.Show($"Repair complete!\n\nFixed: {repairResults.Item1}\nFailed: {repairResults.Item2}", "Repair Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                logger.Log($"Error during repair: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Repair Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnScan.Enabled = true;
                btnRepair.Enabled = true;
                progressBar.Style = ProgressBarStyle.Continuous;
            }
        }

        private void DisplayResults(System.Collections.Generic.List<string> results)
        {
            listViewResults.Items.Clear();
            foreach (var result in results)
            {
                listViewResults.Items.Add(result);
            }
        }

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            string logs = logger.GetLogs();
            MessageBox.Show(logs, "Application Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
