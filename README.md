[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=DFE-Digital_get-information-about-pupils)](https://sonarcloud.io/summary/new_code?id=DFE-Digital_get-information-about-pupils)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=DFE-Digital_get-information-about-pupils&metric=bugs)](https://sonarcloud.io/summary/new_code?id=DFE-Digital_get-information-about-pupils)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=DFE-Digital_get-information-about-pupils&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=DFE-Digital_get-information-about-pupils)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=DFE-Digital_get-information-about-pupils&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=DFE-Digital_get-information-about-pupils)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=DFE-Digital_get-information-about-pupils&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=DFE-Digital_get-information-about-pupils)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=DFE-Digital_get-information-about-pupils&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=DFE-Digital_get-information-about-pupils)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=DFE-Digital_get-information-about-pupils&metric=coverage)](https://sonarcloud.io/summary/new_code?id=DFE-Digital_get-information-about-pupils)

# Get Information About Pupils

The Get Information About Pupils (GIAP) service allows education professionals to search
the [National Pupil Database](https://www.find-npd-data.education.gov.uk/) to retrieve pupil data.
GIAP is the replacement for the legacy service Key to Success (KtS).
It allows education professionals to search the National Pupil Database to retrieve pupil data,
and the following to access pupil-level census and attainment data:

- schools
- academies
- further education colleges
- academy trusts
- local authorities

GIAP also allows users to create and maintain their own custom lists of pupils,
making it easy to track the progress of specific groups or cohorts over time.

The service provides access to [pupil premium](https://www.gov.uk/government/publications/pupil-premium/pupil-premium)
funding allocation, helping schools effectively manage the financial aspects of new student enrolment from the start.

https://www.gov.uk/guidance/get-information-about-pupil-giap

## Status pages

| Tool   | StatusPage | Usage |
|--------|------------|-------|
| GitHub | <https://www.githubstatus.com> | Source code, GitHub actions for CI/CD |
| Azure | <https://status.azure.com> | CIP Infrastructure |
| npm | <https://status.npmjs.org> | `npm` package restore |
| NuGet | <https://status.nuget.org> | `dotnet` package restore |

Related repositories
- [GIAP wiki](https://github.com/DFE-Digital/get-information-about-pupils-wiki)

## Getting Started / Setup
Prerequisites
- [Git](https://git-scm.com/downloads) (for getting a copy of the source code and contributing changes)
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (for building and running the C#/.NET web
application)
- [Node.js](https://nodejs.org/en/download/) (for building web artefacts: (S)CSS, JS, etc.)
- IDE/Editor of choice (e.g., Visual Studio, Visual Studio Code, JetBrains Rider, etc.)
- Local environment configured to authenticate to the GitHub NuGet feed
    - Create a classic [personal access token (PAT)](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens) with `read:packages` scope from your GitHub account (fine-grained tokens [do not support package scopes](https://github.com/github/roadmap/issues/558)).
    - Add the NuGet source to your local environment by filling in the placeholders and running:
    ```sh
    dotnet nuget add source --username <YOUR_GITHUB_USERNAME> --password <YOUR_PERSONAL_ACCESS_TOKEN> --store-password-in-clear-text --name dfedigital "https://nuget.pkg.github.com/DFE-Digital/index.json"
    ```
Clone the repository
```sh
git clone https://github.com/DFE-Digital/get-information-about-pupils
```
Install node dependencies, then build and package the web artefacts (JS, CSS, etc.)
```sh
## DfE.GIAP.All/DfE.GIAP.Web/
npm install
npm run gulp
```

If you have `gulp` installed globally (`npm install -g gulp`), you can run `gulp` directly:
```sh
## DfE.GIAP.All/DfE.GIAP.Web/
gulp
```
Build the C#/.NET solution
```sh
## cd DfE.GIAP.All/
dotnet build
```

Confirm tests are passing locally
```sh
## cd DfE.GIAP.All/
dotnet test
```

Run the application
```sh
## cd DfE.GIAP.All/DfE.GIAP.Web/
dotnet run
```

If using Visual Studio, ensure the `DfE.GIAP.Web` project is set as the startup project and run it (F5 or use the
menu).



### Architecture

GIAP-Web relies on a set of Azure Functions in the GIAP.AzureFunctions application, these then query Azure Cognitive Services/CosmosDB for data. The initial access is granted via [DSI](https://services.signin.education.gov.uk/) which provides authentication and authorisation to the application.

```mermaid
graph TD
X[DFE Sign-in] --> A
A --> X
A[GIAP Web] --- B[GIAP Azure Function]
B --- E[Azure BLOB storage]
B --- D[Azure Cognitive Search]
B --- C[Azure CosmosDB]
D --- C
```

### Project dependencies

GIAP web has a number of dependancies listed below, some are closed source, others are open.

- .NET 8
- node
- gulp
- DSI (DfE sign-in)
- [Azure CosmosDb library](https://github.com/DFE-Digital/infrastructure-persistence-cosmosdb)
- [Azure AISearch library](https://github.com/DFE-Digital/infrastructure-cognitive-search)

### Settings

There are a number of key settings contained in the `appsettings.json` file.
It is recommended to create an `appsettings.local.json` file to store values as they won't be checked into source control as `.gitignore`.

An example is provided for reference, additionally a launchSettings.json is required in Properties.

### launchSettings.json
```json
{
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "https://localhost:44378",
      "sslPort": 44378
    }
  },
  "profiles": {
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Local"
      }
    },
    "DfE.GIAP.Web": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Local"
      },
      "applicationUrl": "https://localhost:44378;http://localhost:5000"
    }
  }
}
```

### appsettings.local.json
```json
{
  "IsSessionIdStoredInCookie": true,
  "MaximumNonUPNResults": 100,
  "MaximumNonULNResults": 100,
  "MaximumUPNsPerSearch": 4000,
  "DsiClientId": "dsi-client-id",
  "DsiClientSecret": "dsi-client-secret",
  "DsiApiClientSecret": "dsi-api-client-secret",
  "DsiMetadataAddress": "dsi-metadata-address",
  "DsiAuthorisationUrl": "dsi-authorisation-url",
  "DsiRedirectUrlAfterSignout": "dsi-redirect-url-after-signout",
  "DsiServiceId": "dsi-service-id",
  "DsiAudience": "dsi-audience",
  "StorageAccountName": "account-name",
  "StorageAccountKey": "account-key",
  "StorageContainerName": "container-name",
  "NonUpnPPLimit": 4000,
  "NonUpnNPDMyPupilListLimit": 100,
  "UpnPPMyPupilListLimit": 4000,
  "UpnNPDMyPupilListLimit": 4000,
  "MaximumULNsPerSearch": 4000,
  "CommonTransferFileUPNLimit": 10,
  "MetaDataDownloadListDirectory": "AllUsers/Metadata",
  "DownloadOptionsCheckLimit": 500,
  "UseLAColumn": true,
  "NpdUseGender": true,
  "PpUseGender": false,
  "FeUseGender": false,
  "FeatureManagement": {
    "FurtherEducation": true
  },
  "GetUserProfileUrl": "http://localhost:7071/api/get-user-profile?code=",
  "GetLatestNewsStatusUrl": "http://localhost:7071/api/get-latest-news-status?code=",
  "DeleteNewsArticleUrl": "http://localhost:7071/api/delete-news-article?code=",
  "DownloadPupilsByUPNsCSVUrl": "http://localhost:7071/api/download-pupils-by-upns-csv?code=",
  "DownloadPupilsByUPNsTABUrl": "http://localhost:7071/api/download-pupils-by-upns-tab?code=",
  "DownloadPupilPremiumByUPNFforCSVUrl": "http://localhost:7071/api/download-pupil-premium-by-upns-csv?code=",
  "QueryLAByCodeUrl": "http://localhost:7071/api/get-la-by-code?code=",
  "QueryLAGetAllUrl": "http://localhost:7071/api/get-la-all?code=",
  "GetAcademiesURL": "http://localhost:7071/api/get-academies?code=",
  "LoggingEventUrl": "http://localhost:7071/api/logging-event?code=",
  "CreateOrUpdateUserProfileUrl": "http://localhost:7071/api/create-or-update-user-profile?code=",
  "DownloadCommonTransferFileUrl": "http://localhost:7071/api/download-common-transfer-file?code=",
  "DownloadSecurityReportByUpnUrl": "http://localhost:7071/api/download-security-report-by-upn-searches?code=",
  "DownloadSecurityReportByUlnUrl": "http://localhost:7071/api/download-security-report-by-uln-searches?code=",
  "DownloadSecurityReportDetailedSearchesUrl": "http://localhost:7071/api/download-detailed-searches?code=",
  "DownloadSecurityReportLoginDetailsUrl": "http://localhost:7071/api/download-login-details?code=",
  "DownloadPupilsByULNsUrl": "http://localhost:7071/api/download-further-education?code=",
  "DownloadPrepreparedFilesUrl": "http://localhost:7071/api/pre-prepared-downloads?code=",
  "PaginatedSearchUrl": "http://localhost:7071/api/get-page/{indexType}/{queryType}?code=",
  "GetAllFurtherEducationURL": "http://localhost:7071/api/get-all-fe?code=",
  "GetFurtherEducationByCodeURL": "http://localhost:7071/api/get-fe-by-code?code=",
  "SetLatestNewsStatusUrl": "http://localhost:7071/api/set-latest-news-status?code=",
  "FeatureFlagAppConfigUrl": "Endpoint="
}
```

## Continual Integration (CI)

`giap-web` has an [build pipeline](./github/ci.yml) which runs on each commit.

The solution depends on the DfEDigital GitHub Packages NuGet Feed.

## Logging

GIAP uses Application Insights to log auditing, warnings, and errors.

## Gulp Runner

We have a Gulp task which combines the JS files and CSS files into a minified version. We’re using npm to install dependencies. We need to install Gulp globally. Assuming you have node and npm installed already.

```sh
npm install -g gulp
```

This installs it system wide as opposed to installing on a project-by-project basis. Gulp runner also compile Sass (for CSS styling) files into CSS. Unfortunately, SCSS won’t work out of the box. Some browsers don’t understand what SASS/SCSS syntax is, so we need to compile it down to a plain css file for them. The only library we need will be gulp-sass, so install it like this: 

```sh
npm install gulp-sass
```

You may notice you have a package.json and package-lock.json file. This process also creates a node_modules directory within the solution folder.

gulp-sass: Sass is a preprocessor and to run in the browsers it needs to be compiled into css, that’s why we need gulp-sass, this gulp plugin will compile the Scss files into CSS.
gulp-rename: this gulp plugin is useful if we want to change the extension file names.

The Gulp task runs behind the scenes when you compile your project.

## License

Unless stated otherwise, the codebase is released under the MIT License. This covers both the codebase and any sample code in the documentation.

The documentation is © Crown copyright and available under the terms of the Open Government 3.0 licence

