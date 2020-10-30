##Migration Scripts

These scripts are auto-generated using the following command: 

```
dotnet ef migrations add InitialCreate -v -o .\Infrastructure\Migrations\
```

- virtual modifier in the models allow lazy loading.
- ID syntax. 



# API Endpoints

## Crawl Repositories

To submit a request for crawling a repository: 

```
Request:
	POST https://localhost:44376/api/v1/RepoCrawlingJobs/

Payload:
	{
		Repository:{
		"ID": 2
		}
	}
```

To get the status of the repository crawling jobs: 

```
Request:
    GET https://localhost:44376/api/v1/RepoCrawlingJobs/

Response:
    [
        {
            "ID": 1,
            "RepositoryID": 2,
            "Status": "Running",
            "UpdatedDate": "Friday, 30 October 2020 22:38:03",
            "CreatedDate": "Friday, 30 October 2020 22:38:03",
            "Repository": {
                "ID": 2,
                "Name": "BioTools",
                "URI": "https://github.com/bio-tools/content/archive/master.zip",
                "UpdatedDate": "Friday, 30 October 2020 22:36:22",
                "CreatedDate": "Friday, 30 October 2020 22:36:22"
            }
        }
    ]
```

# Tools

To get a list of tools: 

```
Request:
    GET https://localhost:44376/api/v1/tools
```

To get detailed information of a tool: 

```
Request:
    GET https://localhost:44376/api/v1/tools/[Tool ID]/
```
