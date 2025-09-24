 ### Bot Web Application — ASP.NET Core MVC on Azure (CI/CD with Azure Pipelines)

This project demonstrates how to build, test, and deploy an ASP.NET Core MVC app to Azure App Service with Azure DevOps Pipelines. It’s purpose-built to showcase cloud engineering skills: CI/CD design, secure configuration, deployment slots, observability, and (optionally) containers & IaC.

Live Demo: https://drive.google.com/file/d/13YLY7zCiUT5tIk5xuJofqbaQeHCJHbSs/view 

Build Status: add your Azure Pipelines badge  

Repo highlights: botapp.csproj, azure-pipelines.yml, azure-pipelines-1.yml are present. 
| Area             | What’s implemented here                                         |
| ---------------- | --------------------------------------------------------------- |
| CI               | Azure Pipelines YAML: restore → build → test → publish artifact |
| CD               | Deploy to App Service staging slot → swap to production         |
| Config & Secrets | App Service Configuration (and Key Vault references if enabled) |
| Observability    | Application Insights + `/healthz` endpoint                      |
| Security         | HTTPS/HSTS, secret hygiene, least-privilege service connections |
| Portability      | Optional Dockerfile + IaC path (Bicep/Terraform)                |

<pre>
Developer → Git push (main)
              │
              ▼
   Azure Pipelines (CI)
Restore → Build → Test → Publish
              │
              ▼
       Artifact: drop
              │
              ▼
   Azure Pipelines (CD)
 Download → Deploy (Staging)
     Smoke test (/healthz)
              │
           Swap to Prod
              └──► App Insights (logs/metrics/traces)
</pre>
 
## Repository layout

<pre>
.
├── Controllers/   Models/   Views/   wwwroot/
├── Program.cs    Startup.cs
├── appsettings.json            # safe, non-secret defaults
├── bot.sln      botapp.csproj  # project/solution files
├── azure-pipelines.yml         # CI pipeline
├── azure-pipelines-1.yml       # extra pipeline (can repurpose as CD)
└── README.md

</pre>
Note: In this README I refer to a CD YAML named azure-pipelines-cd.yml. You can either rename azure-pipelines-1.yml to that and adjust, or keep your filename—functionally the same

## Local development

Prereqs  

.NET SDK (match the TargetFramework in botapp.csproj)  

Azure CLI (optional, for smoke tests/deploys)  

Node.js only if you add front-end tooling  

Run  

<pre>
 dotnet restore
dotnet build -c Release
dotnet run --project botapp.csproj
# open the URL shown in console (e.g., http://localhost:5000)

</pre>

### Health check (recommended)  
<pre>
 // Program.cs
builder.Services.AddHealthChecks();
var app = builder.Build();
app.MapHealthChecks("/healthz");
app.Run();

</pre>

### Configuration & secrets

Do not commit secrets. Store production values in App Service → Configuration:  

ASPNETCORE_ENVIRONMENT=Production  

Any connection strings or API keys  

Optionally use Key Vault references in App Service Configuration.  

Keep appsettings.json safe; ignore appsettings.Development.json in   
.gitignore.
<pre>
 bin/
obj/
.vs/
*.user
*.suo
appsettings.Development.json
*.secret.json

</pre>

## Tests

Add a tiny xUnit project so CI runs real tests:  
<pre>
 dotnet new xunit -n BotApp.Tests
dotnet add BotApp.Tests/BotApp.Tests.csproj reference botapp.csproj

</pre> 
Example:  

<pre>
 using Xunit;
public class SmokeTests { [Fact] public void True_is_true() => Assert.True(true); }

</pre>  

### CI pipeline (Azure DevOps)  

File: azure-pipelines.yml (you already have this file) 
GitHub  

Purpose: restore → build → test → publish artifact  

<pre>
 trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  project: 'botapp.csproj'
  publishDir: '$(Build.ArtifactStagingDirectory)/publish'

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '8.x'  # match TargetFramework in botapp.csproj

- script: dotnet restore $(project)
  displayName: Restore

- script: dotnet build $(project) -c $(buildConfiguration) --no-restore
  displayName: Build

- script: dotnet test **/*Tests/*.csproj -c $(buildConfiguration) --no-build --logger trx
  displayName: Test

- script: dotnet publish $(project) -c $(buildConfiguration) -o $(publishDir) --no-build
  displayName: Publish

- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: '$(publishDir)'
    ArtifactName: 'drop'

</pre>  

### CD pipeline (Azure DevOps → Azure App Service)  

File (suggested): azure-pipelines-cd.yml  

If you prefer, rename your existing azure-pipelines-1.yml to this and update its content.
<pre>
 trigger: none

pool:
  vmImage: 'ubuntu-latest'

variables:
  WebAppName: '<YOUR-APP-SERVICE-NAME>'
  ResourceGroup: '<YOUR-RG>'
  SlotName: 'staging'
  ArtifactName: 'drop'

steps:
- download: current
  artifact: $(ArtifactName)

- task: AzureWebApp@1
  displayName: 'Deploy to Staging Slot'
  inputs:
    azureSubscription: '<YOUR-SERVICE-CONNECTION>'
    appType: 'webApp'
    appName: '$(WebAppName)'
    slotName: '$(SlotName)'
    package: '$(Pipeline.Workspace)/$(ArtifactName)/**/*.zip'

- task: AzureAppServiceManage@0
  displayName: 'Swap Staging → Production'
  inputs:
    azureSubscription: '<YOUR-SERVICE-CONNECTION>'
    Action: 'Swap Slots'
    WebAppName: '$(WebAppName)'
    ResourceGroupName: '$(ResourceGroup)'
    SourceSlot: '$(SlotName)'
    PreserveVnet: true

</pre>

Service Connection: Project Settings → Service connections → Azure Resource Manager (scope it minimally).  

### Observability

Application Insights: enable it and set APPLICATIONINSIGHTS_CONNECTION_STRING in App Service → Configuration.  

Log Stream: check startup/runtime logs directly from the App Service.  

Health checks: point App Service Health check to /healthz for automatic restarts and better availability.  

### Security quick wins

Enforce HTTPS only and add HSTS in Production.  

Keep secrets in App Service/Key Vault, never in git.  

Use least-privilege service connections in Azure DevOps.  

Add branch policies + required CI for main.  

### Troubleshooting (fast)  

## 502 Bad Gateway (App Service)  

App not listening on expected port → set ASPNETCORE_URLS accordingly (80 for classic App Service; 8080 in container example).  

Framework mismatch → ensure App Service runtime matches your TargetFramework.  

Startup crash → check Log Stream and App Insights exceptions.  

Missing env vars → add them in App Service Configuration.  

Artifacts vs Blob confusion  

PublishBuildArtifacts@1 creates a pipeline artifact named drop (not a Blob container).  

Point the CD task’s package to the pipeline artifact path.  

If you intentionally push to Blob Storage, ensure the container exists and your copy step targets the correct name.  

Health probe fails  

Confirm /healthz is mapped.  

Make sure staging & production slots have the same config keys.  

### License

MIT — see LICENSE.
