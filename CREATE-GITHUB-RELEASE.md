# Instructions to Create GitHub Release v2.0.0

## Files Ready for Release

✓ **Release Archive**: `TravellerSystemGenerator-v2.0.0-win-x64.zip` (31 MB)
✓ **Release Notes**: `RELEASE-NOTES-v2.0.0.md`
✓ **Installer Script**: `installer.iss` (for Inno Setup, optional)

## Steps to Create GitHub Release

### Option 1: Using GitHub Web Interface (Recommended)

1. **Navigate to GitHub Repository**
   - Go to: https://github.com/rtrm/TravellerSystemGenerator

2. **Create New Release**
   - Click on "Releases" (right side of repository page)
   - Click "Draft a new release" button

3. **Tag the Release**
   - Click "Choose a tag"
   - Type: `v2.0.0`
   - Click "Create new tag: v2.0.0 on publish"
   - Target branch: `main`

4. **Release Title**
   - Enter: `Traveller System Generator v2.0.0`

5. **Release Description**
   - Copy content from `RELEASE-NOTES-v2.0.0.md`
   - Paste into description box
   - GitHub will render the markdown

6. **Upload Release Assets**
   - Drag and drop `TravellerSystemGenerator-v2.0.0-win-x64.zip`
   - Or click "Attach binaries" and select the file

7. **Mark as Major Release**
   - ✓ Check "Set as the latest release"
   - ✓ Check "Create a discussion for this release" (optional)

8. **Publish**
   - Click "Publish release" button

### Option 2: Using GitHub CLI (if installed)

If you have GitHub CLI (`gh`) installed:

```bash
cd /c/Users/roy/VScode/TravellerSystemsGenerator

# Create release with notes
gh release create v2.0.0 \
  --title "Traveller System Generator v2.0.0" \
  --notes-file RELEASE-NOTES-v2.0.0.md \
  TravellerSystemGenerator-v2.0.0-win-x64.zip
```

### Option 3: Install GitHub CLI First

1. Download GitHub CLI from: https://cli.github.com/
2. Install and authenticate: `gh auth login`
3. Use commands from Option 2

## Post-Release Steps

1. **Verify Release**
   - Check release appears at: https://github.com/rtrm/TravellerSystemGenerator/releases
   - Download the ZIP to verify it's correct
   - Test the downloaded executable

2. **Update Repository README** (optional)
   - Add badge showing latest release
   - Update download links to point to v2.0.0

3. **Announce Release** (optional)
   - Create release discussion thread
   - Post to relevant communities

## Creating Installer (Optional)

If you want to create a Windows installer using Inno Setup:

1. **Download Inno Setup**
   - Get from: https://jrsoftware.org/isdl.php
   - Install Inno Setup 6.x

2. **Compile Installer**
   - Open `installer.iss` in Inno Setup
   - Click "Build" → "Compile"
   - Installer will be created in `installer_output/` folder

3. **Upload Installer to Release**
   - Edit the release
   - Upload the generated `.exe` installer
   - Update release notes to mention installer option

## Checksums (Optional but Recommended)

Generate SHA256 checksums for verification:

```bash
# Windows (PowerShell)
Get-FileHash TravellerSystemGenerator-v2.0.0-win-x64.zip -Algorithm SHA256

# Add checksums to release notes
```

## Notes

- The ZIP file is **self-contained** - includes .NET runtime
- No separate .NET installation required for users
- Executable is ~71 MB (compressed to ~31 MB in ZIP)
- Compatible with Windows 10/11 (64-bit)
