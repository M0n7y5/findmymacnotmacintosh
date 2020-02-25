cd ..
DEL /Q /S bin
dotnet publish FindMyMACNotMacintosh.csproj -c Release -f netcoreapp3.1 
cd tools
DEL /Q /S packed
warp-packer.exe --arch windows-x64 --input_dir ../bin/Release/netcoreapp3.1/publish --exec FindMyMACNotMacintosh.exe --output packed/FindMyMACNotMacintosh.exe
amc.exe packed/FindMyMACNotMacintosh.exe 2
ResourceHacker.exe -script changeIconScript.txt