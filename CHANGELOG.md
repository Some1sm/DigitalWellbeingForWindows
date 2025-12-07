# Changelog

All notable changes to this project will be documented in this file.

## [2.1.0.0] - 2025-12-07

### Added
- **Custom Installer**: Replaced Advanced Installer with a lightweight, native C# installer (`DigitalWellbeing_Installer.exe`).
    - **Interactive UI**: Automatic detection of installation state (Install / Repair / Uninstall).
    - **System Integration**: Registers in "Programs and Features", adds Start Menu and Desktop shortcuts.
    - **Smart Process Management**: Automatically closes running application instances before installation or uninstallation.
    - **Clean Uninstall**: Removes all application files, shortcuts, and registry entries.
- **Crash Reporting**: Added a **Global Exception Handler** to `DigitalWellbeingWPF`. Startup crashes now display a detailed error message instead of failing silently.
- **Build System**: Added `publish.ps1` PowerShell script for one-click compilation and packaging.
    - Automatically cleans up intermediate "Portable" artifacts, producing a single installer executable.
    - Explicitly copies build artifacts to ensure reliable packaging.

### Performance
- **Disk I/O Optimization**: Changed data logging mechanism to buffer persistent usage data in memory.
    - Previous behavior: Write to disk every 3 seconds.
    - New behavior: Accumulate in memory and flush to disk only once every **60 seconds** (or on application exit). This significantly reduces disk writes and improves system responsiveness.

### Changed
- **Versioning**: Updated project version to **2.1.0.0**.
- **Metadata**: Updated Copyright to **© 2025 David Cepero** across all executables and installer registry entries.
- **Localization**: Set default neutral language to **English (United States)** (`en-US`).
- **Project Structure**: Migrated `AssemblyInfo.cs` attributes to modern SDK-style `.csproj` properties for better maintainability and to resolve build duplication errors.

### Fixed
- **Startup Failure**: Fixed an issue where the installed application would fail to start due to missing Working Directory context. The installer now explicitly sets this on shortcuts and launch.
- **Data Logging**: Critical fix ensuring the **Background Service** (`DigitalWellbeingService.NET4.6.exe`) is correctly started after installation and added to the Windows Startup folder. Previously, only the UI was started, preventing data collection.
- **AppTag Error**: Fixed a crash in `AppUsageViewModel` caused by a `String` to `Enum` type mismatch when parsing tag colors.
- **Date Logging**: Fixed a bug in `ActivityLogger.FlushBuffer` where data crossing midnight could be logged to the wrong date file.
- **File Locking**: Fixed `IOException` during installation by ensuring the uninstaller copies itself to a temporary location if needed (though the current design prefers a self-contained single-file approach).
