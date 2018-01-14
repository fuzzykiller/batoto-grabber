using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BatotoGrabber.EntityModel;
using BatotoGrabber.Model;
using CefSharp;

namespace BatotoGrabber
{
    public partial class MainForm : Form
    {
        private readonly ChromiumWebBrowser _browserControl;
        private CancellationTokenSource _cts;

        public MainForm()
        {
            InitializeComponent();

            _browserControl =
                new ChromiumWebBrowser("https://bato.to/forums/index.php?app=core&module=global&section=login") { BrowserSettings = { ImageLoading = CefState.Disabled } };

            browserContainer.Controls.Add(_browserControl);
        }

        private void StartButtonOnClick(object sender, EventArgs e)
        {
            startButton.Enabled = false;
            cancelButton.Enabled = true;

            _cts = new CancellationTokenSource();

            Grab(_cts.Token);
        }

        private void CancelButtonOnClick(object sender, EventArgs e)
        {
            cancelButton.Enabled = false;
            _cts.Cancel();
        }

        private async void Grab(CancellationToken ctsToken)
        {
            try
            {
                await _browserControl.LoadUrl("https://bato.to/myfollows");

                var grabberControl = new GrabberApi(_browserControl);
                progressBar.Style = ProgressBarStyle.Marquee;
                statusLabel.Text = "Fetch followed series...";
                var followedSeries = await grabberControl.GetFollowedSeries();

                if (followedSeries.Length == 0)
                {
                    MessageBox.Show(
                        this,
                        "No followed series found!",
                        "Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                ctsToken.ThrowIfCancellationRequested();

                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Minimum = 0;
                progressBar.Maximum = followedSeries.Length - 1;

                var seriesInfos = new List<SeriesInfo>();

                var seriesCount = followedSeries.Length;
                for (var i = 0; i < seriesCount; i++)
                {
                    var series = followedSeries[i];
                    statusLabel.Text = $"[{i + 1}/{seriesCount}] {series.Name}";
                    progressBar.Value = i;

                    var seriesInfo = await grabberControl.GetSeriesInfo(series);
                    seriesInfos.Add(seriesInfo);

                    // Don't hammer the servers too much
                    await Task.Delay(100, ctsToken);
                }

                var groupRefs = seriesInfos.SelectMany(si => si.Chapters.SelectMany(c => c.Groups))
                    .DistinctBy(x => x.Url)
                    .ToArray();

                progressBar.Value = 0;
                progressBar.Maximum = groupRefs.Length - 1;

                var groupInfos = new List<GroupInfo>();

                var groupCount = groupRefs.Length;
                for (var i = 0; i < groupCount; i++)
                {
                    var groupRef = groupRefs[i];
                    statusLabel.Text = $"[{i + 1}/{groupCount}] {groupRef.Name}";
                    progressBar.Value = i;

                    var groupInfo = await grabberControl.GetGroupInfo(groupRef);
                    groupInfos.Add(groupInfo);

                    // Don't hammer the servers too much
                    await Task.Delay(100, ctsToken);
                }

                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "SQLite database|*.sqlite";
                    sfd.Title = "Save database";

                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        var fileExists = File.Exists(sfd.FileName);
                        using (var ctx = new DbContext(sfd.FileName))
                        {
                            if (!fileExists)
                            {
                                ctx.CreateDatabase();
                            }

                            DbContext.SaveToDatabase(ctx, seriesInfos, groupInfos);
                        }
                    }
                }

                MessageBox.Show("Finished! :)");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(this, "Aborted", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            startButton.Enabled = true;
            cancelButton.Enabled = false;
        }
    }
}
