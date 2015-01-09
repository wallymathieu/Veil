if (Get-Command "node.exe" -ErrorAction SilentlyContinue) 
{
    cinst install nodejs
}
cd Build
& .\build.cmd
