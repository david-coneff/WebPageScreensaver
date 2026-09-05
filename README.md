# Web Page Screensaver

Display webpages as your screensaver.

Inspired by its predecesor [cwc/web-page-screensaver](https://github.com/cwc/web-page-screensaver) (_Achived_).

## Binaries

Version|64-bit
---|---
| build [`8b32c19`](https://github.com/david-coneff/WebPageScreensaver/commit/8b32c19) | [Download](https://github.com/david-coneff/WebPageScreensaver/releases/tag/build-8b32c19) |

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
