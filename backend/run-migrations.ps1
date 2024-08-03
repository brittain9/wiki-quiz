Copy and paste these for now.
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5542;Database=WikiQuizGenerator;Username=postgres;Password=YourStrongPassword!"

dotnet ef database update --project .\src\WikiQuizGenerator.Data\WikiQuizGenerator.Data.csproj --startup-project .\src\WikiQuizGenerator.Api\WikiQuizGenerator.Api.csproj

dotnet ef migrations add migrationname --project . --startup-project ..\WikiQuizGenerator.Api\WikiQuizGenerator.Api.csproj // in data directory
