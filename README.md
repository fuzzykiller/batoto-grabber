# batoto-grabber
Grab “My Follows” and related data from Batoto

## What’s saved?

This tool grabs the “My Follows” list and then proceeds to grab all information on the series, including:

* The current cover image
* All visible metadata (artists, genres etc)
* All chapters (in the selected languages)
* All groups that did any of the chapters

## How is it saved?

When done, the tool creates a SQLite database with foreign keys and whatnot to properly link the downloaded information.

You could use something like “SQLite Browser” to inspect the data.

## How does it work?

This is a Windows application, written in C#/.NET. It’s centered around an embedded Chromium browser using the CefSharp library. It will simply visit all relevant pages and pull data using JavaScript snippets.

## But I don’t use Windows!

You can always use the JavaScript code (located in BatotoGrabber/Scripts/) to create your own tool!