.\_toolz\gitversion.exe /updateprojectfiles
$gitVersionInfo = .\_toolz\gitversion.exe | ConvertFrom-Json 
$semVer = $gitVersionInfo.SemVer;

git add .
git commit -m $semVer;

dotnet pack .\src\Tc.Prober.Recorder\Tc.Prober.csproj -p:PackageVersion=$semVer --output nugets
$nugets = Get-ChildItem -Path nugets\

foreach($nuget in $nugets)
{   
    # dotnet nuget push $nuget.FullName -s https://api.nuget.org/v3/index.json
    dotnet nuget push $nuget.FullName --source "PTKugithub"
}


