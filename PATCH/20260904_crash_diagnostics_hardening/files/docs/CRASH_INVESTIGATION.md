# Vision_Align 비정상 종료 대응 가이드

## 조사 결과 요약

저장소에 포함된 기존 로그를 기준으로 가장 가능성이 높은 종료 경로는 백그라운드 작업 스레드의 처리되지 않은 예외입니다.

- 2026-01-26 예외 로그에는 `ClsAutoThread.MainThreadRunAsync()`에서 Calibration CSV를 기록하던 중 파일 잠금, 권한 거부, 경로 없음 예외가 반복되어 있습니다.
- 기존 `AppDomain.UnhandledException` 이벤트는 예외를 파일에 기록만 했습니다. .NET Framework에서는 백그라운드 `Thread` 밖으로 빠져나온 예외를 기록한 뒤에도 프로세스가 종료됩니다.
- 카메라 로그에는 잘못된/짧은 버퍼 6건과 HALCON `gen_image1` 오류 2건이 있습니다. 이 예외도 Auto 스레드 밖으로 나가면 같은 방식으로 종료될 수 있었습니다.
- `MeasureSharpness()`는 촬영할 때마다 만든 HALCON 이미지·영역·튜플을 해제하지 않았습니다. 장시간 운전 시 네이티브 메모리 및 핸들 고갈 후보입니다.
- 결과 저장 스레드가 원본 이미지를 복사하는 동안 다음 촬영이 같은 HALCON 이미지를 해제할 수 있었습니다. 관리 예외뿐 아니라 네이티브 SDK 충돌 가능성도 있는 경쟁 상태였습니다.
- 작업 스레드가 카메라, 모션 객체 및 화면 핸들이 완성되기 전에 시작됐고, 정상 종료에는 `Thread.Abort()`와 `Environment.Exit()`가 사용됐습니다.

과거 로그만으로 특정 현장 종료의 단일 원인을 확정할 수는 없습니다. 다만 위 경로들은 실제 예외 기록과 코드 동작이 일치하므로 우선순위가 높은 원인 후보입니다.

## 이번 보완 내용

- Auto, PLC, 결과 저장 스레드에 최상위 예외 경계를 두었습니다. Auto 처리 또는 UI 스레드에서 예상하지 못한 예외가 발생하면 자동 운전을 중지하고 PLC 출력을 초기화하며 `Unknown` 알람으로 전환한 뒤 프로세스는 유지합니다.
- 스레드 종료를 `Thread.Abort()` 대신 종료 신호와 제한 시간 `Join()` 방식으로 변경했습니다.
- 모든 하드웨어·화면 객체를 만든 후 작업 스레드를 시작하도록 초기화 순서를 변경했습니다.
- CSV 기록을 프로세스 내부에서 직렬화하고, 공유 읽기/쓰기를 허용하며, 잠금 실패 시 총 5회 재시도하도록 변경했습니다. 최종 실패는 저장 작업 오류로 기록되며 프로그램 전체를 종료하지 않습니다.
- 카메라 취득을 카메라별로 직렬화하고, 파라미터로 AOI를 확정한 뒤 메모리를 할당하며, 모든 SDK 반환값과 프레임 버퍼 크기를 HALCON 전달 전에 검사합니다. 카메라 메모리 잠금은 항상 `finally`에서 해제하며 매 프레임 Free/재할당도 제거했습니다.
- HALCON 원본/처리 이미지 교체와 결과 이미지 복사를 동기화했습니다. 새 프레임 생성이 완전히 성공한 뒤에만 이전 프레임을 교체합니다.
- Sharpness 측정에서 생성하는 모든 HALCON 객체를 매회 해제합니다.
- 설정 JSON은 임시 파일에 먼저 기록한 뒤 원자적으로 교체하고 이전 파일을 `.bak`으로 보존합니다. 본 파일이 손상되면 백업을 읽습니다.
- 캘리브레이션 촬영 수와 각도 포인트 수는 배열 생성·나눗셈 전에 범위를 검사해 손상된 설정값이 공정을 진행하지 못하게 했습니다.
- 실행 경로가 아닌 EXE 위치를 기준으로 `CONFIG`, `RECIPE`, `RESULT`, `LOG` 경로를 계산합니다.
- 정상 종료 시 카메라, 조명, 모션, 작업 스레드를 순서대로 정리합니다.

## 재발 시 생성되는 진단 자료

기본 위치는 실행 파일 옆의 `LOG\DIAGNOSTIC`입니다. 이 위치에 쓸 수 없으면 `%LOCALAPPDATA%\Vision_Align\LOG\DIAGNOSTIC`을 사용합니다.

| 파일 | 의미 |
|---|---|
| `active-session.log` | 실행 중 5초마다 갱신되는 마지막 상태입니다. Auto 단계, 알람, PLC 요청, 연결 상태, 메모리, 작업 스레드 하트비트를 포함합니다. |
| `INCIDENT_*_ERROR_*.log` | 코드가 격리하여 프로그램을 계속 실행한 오류입니다. |
| `INCIDENT_*_FATAL_*.log` | 종료로 이어지는 관리 예외입니다. 같은 이름의 `.dmp` 생성을 시도합니다. |
| `INCIDENT_*_UNCLEAN_SHUTDOWN.log` | 이전 실행이 정상 종료 표시 없이 끝났음을 다음 실행에서 감지한 기록입니다. 전원 차단, 작업 관리자 강제 종료, 네이티브 DLL 충돌 등을 구분할 때 사용합니다. |
| `incidents.log` | 모든 사고 보고서의 시간순 색인입니다. |
| `last-clean-session.log` | 마지막 정상 종료 상태입니다. |
| `emergency.log` | 주 진단 파일이나 NLog 기록 자체가 실패했을 때의 최종 대체 로그입니다. |

## 현장에서 다시 종료됐을 때

1. 발생 시각을 초 단위로 기록하고 당시 화면, Auto/Manual, 진행 공정, PLC 요청, 작업자 조작을 메모합니다.
2. 프로그램을 다시 실행해도 됩니다. 다음 실행 시 남아 있던 `active-session.log`가 `UNCLEAN_SHUTDOWN` 보고서로 자동 보존됩니다.
3. 다음 자료를 한 폴더로 복사해 전달합니다.
   - `LOG\DIAGNOSTIC` 전체
   - 발생일의 `LOG\EXCEPTION`, `LOG\SYSTEM`, `LOG\PLC`
   - `RESULT`의 발생 시각 전후 CSV 및 이미지
   - 아래 WER 덤프 기능을 켰다면 해당 `.dmp`
4. Windows 이벤트 뷰어의 **Windows 로그 > 응용 프로그램**에서 같은 시각의 `.NET Runtime`, `Application Error`, `Windows Error Reporting` 이벤트를 저장합니다. 오류 모듈 이름이 `halcondotnet.dll`, `uEye*.dll`, `AXL.dll` 등인지 확인합니다.
5. `ERROR` 보고서가 있고 프로그램이 계속 동작했다면 보고서의 `Origin`, `AutoStep`, `LastActivity`와 예외 StackTrace부터 확인합니다.
6. `UNCLEAN_SHUTDOWN`만 있고 `FATAL` 보고서가 없다면 전원/OS 강제 종료 또는 네이티브 DLL 충돌 가능성이 높습니다. 이 경우 WER 덤프와 이벤트 로그가 핵심 자료입니다.

수동 복사 대신 아래 명령을 실행하면 위 자료, 해당 시각 전후 결과 파일, 실행 파일/PDB 및 Windows 응용 프로그램 이벤트를 ZIP 하나로 수집합니다. `IncidentTime`은 실제 발생 시각으로 바꿉니다.

```powershell
.\support\Collect-VisionAlignDiagnostics.ps1 `
  -InstallPath .\BIN `
  -IncidentTime '2026-09-04 14:30:00' `
  -WindowMinutes 30
```

생성 위치는 기본적으로 `support\CollectedDiagnostics`입니다. 운영 데이터와 설비 설정이 포함될 수 있으므로 내부 담당자에게만 전달합니다.

## 네이티브 충돌 덤프 활성화

HALCON, IDS 카메라, Ajin 모션 DLL 내부의 Access Violation은 .NET 예외 이벤트가 실행되지 않을 수 있습니다. 운영 PC에서 관리자 PowerShell을 열어 아래 스크립트를 한 번 실행하면 Windows Error Reporting이 `Vision_Align.exe` 네이티브 충돌 덤프를 남깁니다.

```powershell
.\support\Configure-VisionAlignCrashDumps.ps1 -Action Enable -DumpType Mini
```

상태 확인과 해제:

```powershell
.\support\Configure-VisionAlignCrashDumps.ps1 -Action Status
.\support\Configure-VisionAlignCrashDumps.ps1 -Action Disable
```

기본 덤프 경로는 `%ProgramData%\Vision_Align\CrashDumps`이며 최근 10개를 보관합니다. 전체 메모리 덤프가 꼭 필요할 때만 `-DumpType Full`을 사용합니다.

## 빌드 및 현장 검증

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe' `
  .\wonik_sd_vision_align\Vision_Align.sln /t:Rebuild /p:Configuration=Release /p:Platform=x64
```

빌드 PC에는 PLC COM Type Library가 등록되어 있지 않으면 `MSB3284` 경고가 나올 수 있습니다. 저장소의 interop DLL로 컴파일은 가능하지만, 운영 전에는 아래 항목을 실제 설비에서 확인해야 합니다.

- 카메라 2대 연속 Grab 및 Live/Auto 전환
- PRE ALIGN, CONTACT ALIGN 각 20회 이상 반복
- Calibration CSV를 Excel로 열어 둔 상태에서 저장 요청: 프로그램 유지 및 진단 로그 생성 확인
- PLC 연결 해제/복구와 Alive 신호 복구
- 프로그램 종료 후 카메라·조명·모션 재실행
- `active-session.log`가 실행 중 갱신되고 정상 종료 후 제거되는지 확인
