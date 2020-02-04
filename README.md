

  
  

![enter image description here](https://cmpct.azureedge.net/logo--light.png)

**What:** Azure functions to gracefully improve the quality of data in cmpct.io

#### Associated Repositories
1. Frontend website (Nuxt/Vue) - https://github.com/tommcclean/cmpct.io
2. .NETCore Web API - https://github.com/tommcclean/api.cmpct.io
3. C# Azure Functions - This repository.

#### Function List
1. RouteProcessor: The RouteProcessor is triggered by new routes created by the API and stored in BlobStorage. The purpose of the function is to add new data to the model so that users of the website can gain more insight about a link before they visit. Such as page title and a screenshot.

![Screenshot](https://cmpct.azureedge.net/marketing/dark-en.png)  
