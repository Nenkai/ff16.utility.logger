# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/ff16.utility.logger/*" -Force -Recurse
dotnet publish "./ff16.utility.logger.csproj" -c Release -o "$env:RELOADEDIIMODS/ff16.utility.logger" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location