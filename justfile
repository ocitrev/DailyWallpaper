set windows-shell := ['c:\program files\git\bin\sh.exe', '-uc']

build:
    dotnet build

publish:
    dotnet publish -c Release -o "$OneDriveConsumer/Tools"
