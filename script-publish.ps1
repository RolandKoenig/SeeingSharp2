Remove-Item -path ./publish/*

dotnet pack -c Release -o ./publish ./SeeingSharp
dotnet pack -c Release -o ./publish ./SeeingSharp.WinForms
dotnet pack -c Release -o ./publish ./SeeingSharp.WinFormsCore
dotnet pack -c Release -o ./publish ./SeeingSharp.Wpf
dotnet pack -c Release -o ./publish ./SeeingSharp.WpfCore