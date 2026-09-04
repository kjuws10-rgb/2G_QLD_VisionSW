# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build with MSBuild (Visual Studio required)
msbuild Vision_Align/Vision_Align.csproj /p:Configuration=Debug /p:Platform=x64

# Build Release
msbuild Vision_Align/Vision_Align.csproj /p:Configuration=Release /p:Platform=x64

# Restore NuGet packages
nuget restore Vision_Align.sln
```

Output is generated to `BIN/Vision_Align.exe`.

## Project Overview

Industrial vision alignment system for mark and glass detection using dual IDS cameras and HALCON image processing. The application communicates with Omron PLC for factory automation and AjinExtek EtherCAT for motion control.

**Tech Stack:**
- C# / .NET Framework 4.7.2 / WinForms
- HALCON 18.11-Steady (machine vision)
- IDS uEye SDK (camera driver)
- AjinExtek AXL (EtherCAT motion control)
- Newtonsoft.Json, NLog

## Architecture

### Global State (`Vision_Align/Global.cs`)
Central static class managing all application state:
- `Global.dicClsCam` - Camera instances keyed by `CamInfo` enum (CAM_1, CAM_2)
- `Global.dicClsMotion` - Motion control per camera
- `Global.clsAlgorithm[]` - HALCON algorithm instances per camera
- `Global.hModelInfo[,]` - Trained models indexed by [CamInfo, Matching_Type]
- Configuration classes: `Sys_Param`, `CamSet_Param`, `PreConfig_Param`

### Key Enums
- `CamInfo`: CAM_1, CAM_2 (dual camera system)
- `CamSet`: COMMON, MARK_ALIGN, CONTACT (camera parameter sets)
- `Matching_Type`: MARK_GRAY (NCC), MARK_SHAPE (SHM), GLASS_SHAPE (SHM)

### Form Hierarchy
```
FormBase (fullscreen container)
├── FormTitle (status bar)
├── FormMain
│   ├── FormDisplay[0,1] (camera views)
│   ├── FormMain_SubInfo (results)
│   ├── FormMain_SubCalibration
│   └── FormMain_SubConfig
└── FormMenu (control buttons)
```

### Hardware Abstraction Layer (`Vision_Align/Utill/`)
- `ClsCamera.cs` - IDS uEye camera wrapper
- `ClsMotion.cs` - AjinExtek motion control wrapper
- `ClsAlgorithm.cs` - HALCON shape/NCC matching algorithms
- `ClsOmron.cs` - PLC I/O signal mapping
- `ClsLight.cs` - LED lighting control via serial

### Background Threads (`Vision_Align/7.Thread/`)
- `ClsVisionThread` - Vision processing state machine (SeqStep enum: Wait, AutoCal, AutoAlign)
- `ClsOmronThread` - PLC communication polling
- `ClsFolderThread` - File system monitoring

### Motion API (`Vision_Align/AjinAPI/`)
P/Invoke wrappers for AXL.dll (AjinExtek motion library):
- `AXL.cs` - Library init/close
- `AXM.cs` - Axis motion commands
- `AXHS.cs` - Home/servo control

## Configuration System

JSON-based configuration stored in `CONFIG/` folder:
- `SYSTEM_LIST.json` - Current model selections
- `CAMSET_LIST.json` - Camera exposure, gain, lighting per CamSet
- `PRE_CONFIG_LIST.json` - Alignment thresholds, UVW limits
- `UVW_Info.json` - Stage calibration data

Recipe/model files stored in `RECIPE/`:
- `MaskG/` - Gray-scale mark models (NCC)
- `MaskS/` - Shape-based mark models (SHM)
- `GlassS/` - Glass shape models (SHM)

Load/save via `Global.IsLoadConfig(bool)` and `JsonConvertor` class.

## External Dependencies

**Required SDK installations:**
- MVTec HALCON 18.11-Steady: `C:\Program Files\MVTec\HALCON-18.11-Steady\`
- IDS uEye: `C:\Program Files\IDS\uEye\`
- AjinExtek AXL library

**Runtime config files:**
- `d:\IDS.ini`, `d:\IDS1.ini` - Camera parameters
- `D:\1.Program\BIN\CONFIG\Vision_Align.mot` - Motion configuration

## Coding Conventions

- Form files organized by numbered folders: `1.FormBase/`, `2.FormViewer/`, `3.FormMain/`, etc.
- Utility classes prefixed with `Cls` (e.g., `ClsCamera`, `ClsMotion`)
- Hungarian-ish notation: `b` for bool, `d` for double, `n` for int, `str` for string
- All hardware access through wrapper classes in `Utill/`

## WinForms UI Guidelines

- **Designer Compatibility Required**: All UI forms must work without errors in Visual Studio Designer
- **Controls in InitializeComponent()**: All controls must be created inside `InitializeComponent()` so they are visible in the Designer
- **No Runtime-Only Controls**: Avoid creating controls only at runtime (in constructor or other methods) - Designer should show the complete UI layout
- **Build Confirmation**: Always ask before running build commands
