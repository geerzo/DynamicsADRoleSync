**NOTE** - This project follows the Git Flow methodology, so the develop branch is active develop and the master branch is released code, tagged with versions.

# Overview
This project is designed to allow organizations to be able to use Azure Active Directory groups to manage what roles a user is assigned to in Dynamics 365 Customer Engagement. Since there is no native integration this is implemented as a one-way sync between AD and Dynamics on a timer.

For the time being this project is more of a proof of concept vs production ready code.

# Setup
This will describe the steps required to deploy this project to your organization. The function uses Server-to-Server authentication vs username/password authentication which requires some of the additional setup tasks.

## Prerequisites
* Azure Account to deploy an Azure Functions application
* Dynamics 365 Customer Engagement Online system connected to Azure AD (you'll need admin access for the setup)
* The Azure account where the Function runs can be the same or different tenant as Dynamics

## Steps
1. Create an App Registration in the Dynamics tenant with appropriate permissions.
2. Create a password key for the App Registration
3. Provide admin consent to the requested permissions
4. Create an Application User in Dynamics with the Application ID of the App Registration
5. Deploy the Function App and fill in the required Application Settings
6. Sit back and have it sync!

## Details
### Create an App Registration in the Dynamics tenant with appropriate permissions
TBD

### Create a password key for the App Registration
TBD

### Provide admin consent to the requested permissions
1. Navigate to this URL (fill in values) `https://login.microsoftonline.com/<DynamicsTenant>/oauth2/authorize?client_id=<AppID>&redirect_uri=<RedirectURLFromAppRegistration>&response_type=code&prompt=admin_consent`
2. Login with an Admin Account
3. Accept the permissions

### Create an Application User in Dynamics with the Application ID of the App Registration
TBD

### Deploy the Function App and fill in the required Application Settings
1. Deploy Functions app from either Visual Studio or via the web console
2. Set Application Settings
   1. AzureWebJobsStorage - Azure Storage Connection String
   2. Tenant - Dynamics Tenant
   3. AppClientID - The Application ID from the App Registration
   4. AppClientSecret - The password generated on the App Registration
   5. DynamicsRootUrl - Base URL of your dynamics instance
   6. GroupMatchRegEx - The regular expression used to determine if the AD group is mappable to Dynamics

## Reference
* Server-to-Server Authentication - https://msdn.microsoft.com/en-us/library/mt790168.aspx