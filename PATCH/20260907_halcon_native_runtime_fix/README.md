# Vision_Align HALCON 네이티브 런타임 시작 오류 수정

- 기준 커밋: `bd1fc70935f167362879d2dfc8132401ba16d340`
- 작업 브랜치: `fix/native-dll-startup-failure`
- 기능 커밋: `78a8d48` (`Fix HALCON native runtime startup`)

## 확인된 원인

제공된 디버그 출력과 동일한 종료를 재현해 `BIN\LOG\CRASH\20260907.log`를 확인했다.

- 관리 래퍼 `halcondotnet.dll`은 정상 로드됐다.
- 첫 HALCON 호출인 `HOperatorSet.SetSystem`에서 네이티브 `halcon.dll`을 찾지 못했다.
- 예외는 `FormBase.Initializ()` 42행에서 발생해 `Program.Main`까지 전달됐다.
- 기존 코드는 예외를 파일에만 기록하고 정상 종료 코드 `0`으로 끝나 사용자에게 원인을 알리지 않았다.

HALCON Windows x64 런타임의 기본 위치는 `%HALCONROOT%\bin\%HALCONARCH%`이며 이 프로그램은 HALCON 18.11 x64(`HALCONARCH=x64-win64`)를 사용한다.

## 변경 내용

- 시작 시 `halcon.dll`을 다음 순서로 찾아 미리 로드한다.
  1. 실행 파일과 함께 배포된 HALCON 폴더
  2. `HALCONROOT` 및 `HALCONARCH` 환경 변수 경로
  3. `C:\Program Files\MVTec` 아래의 HALCON 18.11 설치 경로
  4. 현재 프로세스의 `PATH`
- 런타임을 발견하면 해당 폴더를 현재 프로세스의 `PATH`에 우선 등록하고 `HALCONROOT`, `HALCONARCH`를 맞춘다.
- `halcon.dll`이 없거나 그 하위 종속 DLL을 로드하지 못하면 검색 경로와 Win32 오류를 충돌 로그에 남긴다.
- 시작 실패 시 한국어 오류 창과 로그 위치를 표시하고 종료 코드 `1`을 반환한다.
- 정상 시작 시 실제로 선택된 HALCON 런타임 경로를 SYSTEM 로그에 기록한다.
- 프로젝트의 HALCON 및 uEye 관리 DLL 참조를 특정 PC의 `Program Files` 경로 대신 저장소의 `BIN` 경로로 변경했다.
- 수정된 Debug x64 `BIN\Vision_Align.exe`와 PDB를 함께 반영했다.

## 대상 PC에서 필요한 확인

다음 파일이 설치돼 있어야 한다.

```text
C:\Program Files\MVTec\HALCON-18.11-Steady\bin\x64-win64\halcon.dll
```

설치 위치가 다르면 시스템 또는 사용자 환경 변수 `HALCONROOT`에 HALCON 18.11 루트 폴더를 지정한다. 파일이 없다면 MVTec의 정식 HALCON 18.11 Steady x64 Runtime과 라이선스를 설치하거나 복구해야 한다. 네이티브 HALCON 런타임 자체는 이 저장소에 포함하지 않는다.

실패 세부 내용은 실행 폴더의 `LOG\CRASH\yyyyMMdd.log`에서 확인한다. 정상 로드 경로는 `LOG\SYSTEM\yyyyMMdd.log`의 `HALCON Runtime` 항목에서 확인한다.

MVTec 설치 가이드: https://www.mvtec.com/fileadmin/Redaktion/mvtec.com/products/halcon/documentation/halcon/installation_guide.pdf

## 검증 결과

- 제공된 상황과 동일하게 네이티브 HALCON이 없는 환경에서 기존 EXE가 `System.DllNotFoundException: halcon`으로 종료되는 것을 재현했다.
- Visual Studio MSBuild로 `Debug|x64` 전체 빌드에 성공했다. 기존 소스의 미사용 변수 경고만 남고 빌드 오류는 없다.
- 런타임 미설치 시험에서 상세 충돌 로그, `Vision_Align 시작 실패` 오류 창 및 종료 코드 `1`을 확인했다.
- 격리된 x64 DLL을 이용한 탐색 시험에서 로컬 HALCON 배포 폴더를 발견하고 선로드하는 경로를 확인했다.
- 현재 검증 PC에는 정식 HALCON 런타임, 라이선스 및 현장 카메라/PLC/모션 장비가 없어 실제 영상 처리 운전 시험은 수행하지 않았다.

## 패치 적용

기준 커밋에 Git 패치를 적용하려면 저장소 루트에서 다음을 실행한다.

```powershell
git am PATCH/20260907_halcon_native_runtime_fix/0001-Fix-HALCON-native-runtime-startup.patch
```

Git 패치를 사용할 수 없는 환경에서는 `files` 폴더의 파일들을 저장소의 동일 상대 경로로 복사한다.
