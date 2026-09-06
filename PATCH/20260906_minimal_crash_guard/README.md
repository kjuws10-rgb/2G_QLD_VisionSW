# Vision_Align 최소 종료 방어 패치

- 기준 브랜치: `origin/main`
- 기준 커밋: `1c9ea1a9c7d965239b4fd54ddabd8b211ab14f6d`
- 작업 브랜치: `fix/minimal-crash-analysis`
- 기능 커밋: `3a86b74` (`Add minimal crash guards and diagnostics`)

## 분석 결론

제보된 종료 시각과 당시 조작 내용이 없어 이번 한 건의 원인을 하나로 확정할 수는 없다. 다만 현재 코드와 Git 이력에 남아 있던 운영 로그를 대조하면 다음 경로가 종료 가능성이 높다.

1. `ClsAutoThread`는 최상단 예외 보호가 없어서 CSV, 카메라, 알고리즘 예외가 스레드 밖으로 나오면 .NET 프로세스가 종료될 수 있다.
2. 이전 운영 로그(`f16e076` 커밋의 `BIN/LOG/EXCEPTION`)에는 실제로 다음 예외가 기록되어 있다.
   - 2026-01-26: `9PointResult.csv`, `ViberationResult.csv` 잠금 및 `CALIBRATION` 경로 권한/경로 오류가 `ClsAutoThread.MainThreadRunAsync`까지 전파됨.
   - 2026-01-13: 카메라 영상 버퍼 크기 불일치가 `ClsAutoThread.MainThreadRunAsync`까지 전파됨.
   - 2026-01-05: PLC 데이터 형식 변환 오류가 `ClsOmron.UpdateAI`에서 `ClsOmronThread.MainThreadRunAsync`까지 전파됨.
3. 기존 전역 예외 처리기는 `Global.logger`가 생성되기 전이거나 로그 경로 쓰기에 실패하면 예외 기록 자체가 남지 않을 수 있다.
4. 저장 스레드는 예외를 빈 `catch`로 버려 프로그램이 유지되어도 실패 원인을 확인할 수 없었다.

## 최소 변경 내용

- 관리 예외를 `LOG\CRASH\yyyyMMdd.log`에 독립 기록한다. 기본 경로 쓰기에 실패하면 `%TEMP%\Vision_Align\CRASH\yyyyMMdd.log`를 사용한다.
- 기록에는 발생 위치, 종료 여부, 프로세스/스레드, 실행 경로, AUTO 단계, 알람, 장비 연결 상태, PLC 요청 상태와 전체 스택을 포함한다.
- AUTO 스레드의 예상하지 못한 예외는 기록 후 AUTO를 해제하고 PLC 출력을 초기화한 뒤 `Unknown` 알람을 세운다. 불완전한 시퀀스를 자동 재개하지 않으며 프로그램 재시작이 필요하다.
- PLC 스레드 예외는 기록하고 10초 후 다시 루프에 진입한다.
- CSV 쓰기는 폴더를 보장하고 `IOException`에 한해 최대 3회(실패 사이 200ms, 400ms) 시도한다.
- 카메라 버퍼가 null이거나 예상 크기보다 작으면 알고리즘에 전달하지 않고 실패 처리한다. 동일 오류 기록은 카메라별 30초 간격으로 제한한다.
- 저장 작업의 빈 예외 처리를 실제 진단 로그로 교체한다.

검사 판정, 좌표 계산, 모션 이동량, PLC 주소 및 정상 시퀀스는 변경하지 않았다.

## 재발 시 확인 순서

1. 종료 시각, AUTO/MANUAL, 수행 중이던 작업(Pre Align/Contact/Calibration/Model Change), 열어 둔 CSV 파일 여부를 기록한다.
2. 실행 파일 폴더의 `LOG\CRASH\해당날짜.log`를 먼저 보관한다.
3. 파일이 없으면 `%TEMP%\Vision_Align\CRASH\해당날짜.log`를 확인한다.
4. 두 위치 모두 기록이 없으면 Windows 이벤트 뷰어의 `Windows 로그 > 응용 프로그램`에서 같은 시각의 `.NET Runtime` 및 `Application Error`를 확인한다. 이 경우 네이티브 HALCON/카메라 SDK 오류, 강제 종료, 전원 문제처럼 관리 예외 처리기가 실행되지 않은 종료일 가능성이 있다.
5. `LOG\EXCEPTION`, `LOG\SYSTEM`의 같은 시각 전후 파일도 함께 보관한다.

## 검증 결과와 제한

- 변경한 6개 C# 파일을 Roslyn 파서로 검사했으며 구문 오류는 0건이다.
- 전체 빌드는 새 기준 커밋에서 `BIN` 외부 DLL이 삭제된 상태이고 현재 PC에도 `VoAlgorithm.dll`, HALCON 및 uEye 개발 DLL이 없어 참조 오류로 중단됐다. 이는 이 패치의 소스 변경에서 발생한 오류가 아니다.
- 실제 카메라, PLC, 모션이 연결된 현장 운전 시험은 수행하지 않았다.
- 브랜치 전환 시 이미 존재한 미추적 `obj/x64/Debug/Vision_Align.FormTopMassage.resources`는 패치에서 제외했다.

## 적용 방법

Git 이력을 유지해 적용하려면 저장소 루트에서 다음 패치를 사용한다.

```powershell
git am PATCH/20260906_minimal_crash_guard/0001-Add-minimal-crash-guards-and-diagnostics.patch
```

Git 패치를 사용할 수 없는 환경에서는 `files` 아래의 파일을 저장소의 동일 상대 경로로 복사한다.
