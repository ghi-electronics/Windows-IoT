cd %1
nuget pack %1.csproj -Prop Configuration=Release -OutputDirectory %BUILD_OUTPUT%\NuGet\
cd ..