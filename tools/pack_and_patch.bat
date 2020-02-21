cd ..
DEL /Q /S bin
DEL /Q /S packed
dotnet publish -c Release 
cd tools
warp-packer.exe --arch windows-x64 --input_dir ../bin/Release/netcoreapp3.1/publish --exec FindMyMACNotMacintosh.exe --output FindMyMACNotMacintosh.exe
amc.exe FindMyMACNotMacintosh.exe 2
cd tools