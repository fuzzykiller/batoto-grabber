using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BatotoGrabber.EntityModel;
using BatotoGrabber.Model;
using BatotoGrabber.Scripts;
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
                var grabberControl = new GrabberApi(_browserControl);
                progressBar.Style = ProgressBarStyle.Marquee;
                statusLabel.Text = "Fetch followed series...";
                var followedSeries = await grabberControl.GetFollowedSeries();
                var lastReads = await grabberControl.GetFollowedSeriesLastRead();

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

                string fileName;
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "SQLite database|*.sqlite";
                    sfd.Title = "Save database";

                    if (sfd.ShowDialog(this) != DialogResult.OK)
                    {
                        throw new OperationCanceledException();
                    }

                    fileName = sfd.FileName;
                    File.Delete(sfd.FileName);
                }

                progressBar.Style = ProgressBarStyle.Marquee;
                statusLabel.Text = "Saving database...";

                using (var db = new SQLiteConnection($"Data Source={fileName}").OpenAndReturn())
                {
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = Script.CreateDatabbase;
                        cmd.ExecuteNonQuery();
                    }

                    using (var ctx = new DbContext(db))
                    {
                        await ctx.SaveToDatabase(seriesInfos, groupInfos, lastReads);
                    }

                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Value = 0;
                    progressBar.Maximum = seriesCount;
                    statusLabel.Text = "Fetching covers...";

                    using (var updateImageCommand = db.CreateCommand())
                    {
                        updateImageCommand.CommandText =
                            "UPDATE `Series` SET `Image` = @image WHERE `PrimaryName` = @name";

                        for (int i = 0; i < seriesCount; i++)
                        {
                            progressBar.Value = i;
                            var series = seriesInfos[i];
                            var image = await TryGetCover(series);

                            if (image == null) continue;
                            
                            updateImageCommand.Parameters.Clear();
                            updateImageCommand.Parameters.AddWithValue("@name", series.Name);
                            updateImageCommand.Parameters.AddWithValue("@image", image);

                            updateImageCommand.ExecuteNonQuery();
                        }
                    }
                }

                progressBar.Value = progressBar.Maximum;

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

        private static async Task<byte[]> TryGetCover(SeriesInfo series)
        {
            var wc = new WebClient { Headers = { ["Referer"] = series.Url } };
            try
            {
                return await wc.DownloadDataTaskAsync(series.ImageUrl);
            }
            catch
            {
                return null;
            }
            finally
            {
                wc.Dispose();
            }
        }
    }
}
