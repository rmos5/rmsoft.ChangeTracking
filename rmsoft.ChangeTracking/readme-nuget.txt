/* Sample nuget CLI */
nuget update -Self
nuget pack -Build -Symbols -Properties Configuration=Release
nuget push rmsoft.ChangeTracking.1.0.0.nupkg -Source [url here] -ApiKey AzureDevOps