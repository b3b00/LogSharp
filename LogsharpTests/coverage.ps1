
dotnet test -v normal /p:CollectCoverage=true '/p:CoverletOutputFormat="json,lcov"' /p:CoverletOutput=./lcov '/p:exclude="[LogSharpTester]*,[LogsharpTests]*"' LogsharpTests.csproj
reportgenerator.exe -reports:lcov.info -targetdir:coverage-report -reporttypes:Html

