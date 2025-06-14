# SonarQube

We use the DFE-Digital managed SonarQube Cloud to perform static code analysis for code smells, duplicated code and other similar issues

## SonarCloud Projects

- [giap-web](https://sonarcloud.io/project/overview?id=DFE-Digital_get-information-about-pupils)

## Setup

Creating the project in SonarQube is granted through a ServiceNow request to Digital Tools Support (see `#digital-tools-support` in DfE slack)

Documentation

- [Connecting SonarQube local to SonarQube cloud](https://docs.sonarsource.com/sonarqube-for-ide/visual-studio/team-features/connected-mode-setup/#sonarqube-cloud)
- [How SonarQube scans local](https://docs.sonarsource.com/sonarqube-for-ide/visual-studio/using/scan-my-project/#scanning-csharp-and-vbnet)
- [Video explaining how SonarQube works](https://www.youtube.com/watch?v=GRVA4AiO7OM)

## Local development

SonarQube provide a [VisualStudio extension](https://docs.sonarsource.com/sonarqube-for-ide/visual-studio/getting-started/installation/).

This can be connected through your IDE using to run analysis based on the same `QualityProfile` and `Issue management` as the SonarQube cloud project
This helps to resolve issues locally that would show up in a scan at PR

As of `2025/06/14` connecting in IDE requires generating a token, providing that to the connection wizard, and binding that to the SonarCloud project.

![token](./generate-sonar-token.png)

Files are scanned on open by default and should show warning analyzers if issue detected. Insert a // TODO to test
![scan](./scanning-as-opens.png)

![analyser-works](./analyser-working.png)

![analyser-output](/analyser-errorlist.png)