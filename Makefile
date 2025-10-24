default: test

test:
	dotnet test

build: 
	dotnet build

run:
	dotnet run --project src/OldenEraHome.Wiki.WebApi/OldenEraHome.Wiki.WebApi.csproj -lp http
