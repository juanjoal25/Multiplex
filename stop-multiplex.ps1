Stop-Process -Name "dotnet" -Force
     docker stop $(docker ps -q)