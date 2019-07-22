mcs -r:"ServerPackages/UnityEngine.dll" "Assets/Shared/*.cs" "Assets/Server/*.cs" -out:"ServerPackages/Server.exe"
mono ServerPackages/Server.exe localhost

