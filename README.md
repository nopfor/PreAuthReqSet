
# PreAuthReqSet

C# project to view or edit AD user account, setting/unsetting "Do not use Kerberos pre-authentication" UAC value. Leverages the Active Directory Services Interfaces (ADSI) through the `System.DirectoryServices` namespace.

## Requirements

The program relies on the following NuGet packages:

* `System.DirectoryServices`
* `System.DirectoryServices.AccountManagement`

## Usage

```
PreAuthReqSet.exe /user:USER [/set:True|False]
```
