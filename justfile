set windows-shell := ['c:\program files\git\bin\sh.exe', '-uc']

build:
    dotnet build

clean:
    {{ if path_exists('bin') == 'true' { 'rm -r bin/' } else { '' } }}
    {{ if path_exists('obj') == 'true' { 'rm -r obj/' } else { '' } }}

publish:
    dotnet publish -c Release -o "$OneDriveConsumer/Tools"
