# Web Page Screensaver

Display webpages as your screensaver.

Inspired by its predecesor [cwc/web-page-screensaver](https://github.com/cwc/web-page-screensaver) (_Achived_).

## Binaries

Version|64-bit
---|---
| build [`a72b6eb`](https://github.com/david-coneff/WebPageScreensaver/commit/a72b6eb) | [Download](https://github.com/david-coneff/WebPageScreensaver/releases/tag/build-a72b6eb) |

Self-contained single-file build (~130MB) — no separate .NET runtime install needed.
Published as a [GitHub Release](https://github.com/david-coneff/WebPageScreensaver/releases)
rather than committed in-tree: the file is well over GitHub's 100MB plain-blob limit, and
this is a fork, which GitHub does not allow pushing new Git LFS objects to (storage quota
is billed against the upstream repo owner). Built cross-compiled and never run or tested —
see the release notes for exactly how, and the [debugging instructions](#Debugging) if you'd
rather build (and verify) it yourself on a real Windows machine. No 32-bit build is published;
build one yourself via `PublishX86.pubxml` if you need it.


## Installation instructions

* Install the [dependencies](#Dependencies).
* Download The *.scr for your architecture.
* Right click the *.scr file. You have three options to choose from:
  * Select `Test` if you want to preview it in full screen. Note: Press ESC to exit the screensaver.
  * Select `Configure` to modify the screensaver settings. You should see this Window:

    ![Screenshot](screenshot.png)

  * Select `Install` if you want it to be added to your list of Windows screensavers. The Windows `Screen Saver Settings` window will pop up with this screensaver selected.

## Staying logged in

If a page needs you to be signed in, open `Configure` and click **Log In...**. This opens a
normal browser window sharing the screensaver's own persistent session — sign in there once,
close the window, and the screensaver will show the page already logged in from then on. No
separate account/credential setup: it's the same underlying browser profile (WebView2) either
way, just a window you can type into. See
[WebPageScreensaver-memory:state/session-recovery-design.md](https://github.com/david-coneff/WebPageScreensaver-memory/blob/main/state/session-recovery-design.md)
for how this works and its one known edge case (don't leave the login window open and idle for
a long time — it can conflict with the screensaver starting on its own).

## Zoom

Each screen has its own default zoom level (Preferences → the "% zoom" field, next to rotation
interval) — useful when a display needs a dashboard (e.g. a Home Assistant panel) scaled up or
down to actually fit. It's applied on every launch, not just remembered from the last time you
manually zoomed.

## Security

Because a screensaver is shown to whoever is physically at the machine, the display itself (not
the Log In window — see above) disables the browser surface that has nothing to do with
rendering a page: no right-click menu, no DevTools, no Ctrl+P/S/O/F shortcuts, no downloads,
popups redirect into the same window instead of opening a new one, navigation is restricted to
the screen's own configured site(s), and the Windows accessibility shortcuts (Sticky/Toggle/
Filter Keys) are disabled while showing and restored on exit. This assumes Windows' own "on
resume, display logon screen" setting is enabled — without it, none of this matters, since
dismissing the screensaver normally already hands over the real desktop. Full details and named
gaps: [WebPageScreensaver-memory:state/unattended-display-hardening.md](https://github.com/david-coneff/WebPageScreensaver-memory/blob/main/state/unattended-display-hardening.md).

## Dependencies

Whether you are just installing it or building it, you need the following dependencies:

* .NET 5.0 (>= Preview 8) Desktop Runtime for Windows: https://dotnet.microsoft.com/download/dotnet/5.0
* Microsoft Edge Insider (Canary): https://www.microsoftedgeinsider.com/en-us/download/
* Windows 10.

## Fixes and improvements

### 2.0.2-Alpha
* TFM is now targeting net5.0-windows (due to WinForms).
* Upgraded Microsoft.Web.WebView2 to 1.0.774.44.

### 2.0.1-Alpha
* Upgrade to .NET 5.0.
* Use Edge (WebView2) instead of Internet Explorer.

## Known issues

### 2.0.1-Alpha

* The WinForms theme does not look nice in the published single-file binary. It looks fine when debugging or when building normally (not publishing). It may be caused by the trimming process, which removes what it thinks are unnecessary UI dependencies.
* After installing the *.scr, it's not possible to open the Settings window directly from the Windows Screen Saver Settings button. The workaround is to open the settings by directly double clicking on the *.scr file and configuring the screen saver from there.
* Close on mouse movement does not work because Edge is capturing the mouse movement events, preventing the screensaver from detecting them. The workaround is to press the Esc key.
* ~~Publishing does not yet convert the generated *.exe to *.scr.~~ Fixed in `8b32c19` — the
  PostBuild copy now runs on `dotnet publish` too, not just `dotnet build`.
* ~~Can't host the final *.scr files in GitHub due to the large size.~~ Published as a
  [GitHub Release](https://github.com/david-coneff/WebPageScreensaver/releases) instead — see
  [Binaries](#binaries).

## Contributing

Issues and PRs are welcome.

## Debugging

* Install the [dependencies](#Dependencies).
* Clone the repo.
* Build with `dotnet build`.
* To debug from Visual Studio, edit the [launchSettings.json](Properties/launch.json) file and set the `"commandLineArgs"` value to:
  * `"/p"` if you want to debug the screensaver itself.
  * `"/c"` if you want to debug the settings window.
* Generate a single *.exe by publishing the project using the *.pubxml files in the `Properties/PublishProfiles` folder. Then rename the *.exe to *.scr.
