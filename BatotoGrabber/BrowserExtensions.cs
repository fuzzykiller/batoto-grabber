﻿using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.WinForms;

namespace BatotoGrabber
{
    public static class BrowserExtensions
    {
        public static async Task<int> LoadUrl(this ChromiumWebBrowser browser, string url)
        {
            var tcs = new TaskCompletionSource<int>();
            browser.FrameLoadEnd += OnFrameLoadEnd;
            browser.Load(url);

            var wait30Seconds = Task.Delay(30000).ContinueWith(t => -1);
            var task = await Task.WhenAny(tcs.Task, wait30Seconds);
            return await task;

            void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs eventArgs)
            {
                if (eventArgs.Frame.IsMain)
                {
                    tcs.SetResult(eventArgs.HttpStatusCode);
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