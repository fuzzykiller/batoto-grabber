using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.WinForms;

namespace BatotoGrabber
{
    public static class BrowserExtensions
    {
        public static Task LoadUrl(this ChromiumWebBrowser browser, string url)
        {
            var tcs = new TaskCompletionSource<object>();
            browser.FrameLoadEnd += OnFrameLoadEnd;
            browser.Load(url);

            return tcs.Task;

            void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs frameLoadEndEventArgs)
            {
                var success = frameLoadEndEventArgs.HttpStatusCode >= 200 &&
                              frameLoadEndEventArgs.HttpStatusCode <= 200;

                if (frameLoadEndEventArgs.Frame.IsMain && success)
                {
                    tcs.SetResult(null);
                    browser.FrameLoadEnd -= OnFrameLoadEnd;
                }
            }
        }

        public static async Task<object> EvaluateScriptAsyncEx(this ChromiumWebBrowser browser, string script)
        {
            var response = await browser.EvaluateScriptAsync(script);

            if (!response.Success)
            {
                throw new Exception(response.Message);
            }

            return response.Result;
        }
    }
}