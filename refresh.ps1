Remove-Item .\youtube-feed.db
Remove-Item -Recurse .\Migrations\
dotnet ef migrations add InitialModel --context VideoContext
dotnet ef database update --context VideoContext
dotnet run