cd ..
DEL /Q /S bin
DEL /Q /S packed
dotnet publish -c Release
cd tools
windows-x64.warp-packer.exe --arch windows-x64 --input_dir ../bin/Release/netcoreapp3.1/publish --exec FindMyMACNotMacintosh.exe --output ../packed/FindMyMACNotMacintosh.exe
cd ..
tools\amc.exe packed/FindMyMACNotMacintosh.exe 2
cd tools