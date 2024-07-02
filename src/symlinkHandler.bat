if "%GITHUB_WORKSPACE%"=="" (call) else (
	echo "Github Actions detected, not doing symlink stuff..."
	exit /b 0
)
REM First step: find Rainworld's folder
set progfiles=C:/Program Files
set progfiles86=C:/Program Files (x86)
set rainworld=Rain World/RainWorld_Data/StreamingAssets
if "%DeadlandsLocation%"=="" (
	echo "'DeadLandsLocation' not defined, automatically determining Rainworld's location..."
	if EXIST "%progfiles%/Steam/steamapps/common/%rainworld%" (
		set DeadlandsLocation="%progfiles%/Steam/steamapps/common/%rainworld%/mods/Deadlands"
	) else if EXIST "%progfiles86%/Steam/steamapps/common/%rainworld%" (
		set DeadlandsLocation="%progfiles86%/Steam/steamapps/common/%rainworld%/mods/Deadlands"
	) else if EXIST "D:/Steam/steamapps/common/%rainworld%" (
		set DeadlandsLocation="D:/Steam/steamapps/common/%rainworld%/mods/Deadlands"
	) else (
		echo "Unable to find the location of Deadland's mod folder within Rainworld!'"
		exit /b 1
	)
)
REM Second: Determine if Deadlands is already there; make a symlink if it isn't
if EXIST %DeadLandsLocation% (
	echo "Symbolic link seemingly already exists! Cool c:"
) else (
	echo "Generating symbolic link for Deadlands repo..."
	echo %DeadLandsLocation%
	echo "%cd%"
	mklink /d %DeadLandsLocation% "%cd%/../mod"
)