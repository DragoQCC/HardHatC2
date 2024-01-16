## OnHttpManagerCreate
### Start Context:
 OnHttpManagerCreate
### End Context:
 OnHttpManagerCreate_End
### Source Location:
 TeamServer
### Description:
 Fires when a new HttpManager is created. Hook on Start to get the creation args of (name, address, port, bind address, bind port, is secure, and the C2 profile). Hook the End to get the created HttpManager
### Parameters:
- name : `String`
- connectionAddress : `String`
- connectionPort : `Int32`
- bindAddress : `String`
- bindPort : `Int32`
- isSecure : `Boolean`
- profile : `C2Profile`
	- **Properties:**
		- Name : `String`
		- Desc : `String`
		- Urls : `List<String>`
		- Cookies : `List<String>`
		- RequestHeaders : `List<String>`
		- ResponseHeaders : `List<String>`
		- UserAgent : `String`
### Returns:
 - `HttpManager`
	- **Properties:**
		- Name : `String`
		- ConnectionPort : `Int32`
		- ConnectionAddress : `String`
		- BindAddress : `String`
		- BindPort : `Int32`
		- Active : `Boolean`
		- IsSecure : `Boolean`
		- CertificatePath : `String`
		- CertificatePassword : `String`
		- c2Profile : `C2Profile`
		- Type : `ManagerType`
		- Id : `String`
		- CreationTime : `DateTime`
### Json Content
```json
{
  "Name": "HttpManagerFactoryFunc",
  "ContextName": "OnHttpManagerCreate",
  "Description": "Fires when a new HttpManager is created. Hook on Start to get the creation args of (name, address, port, bind address, bind port, is secure, and the C2 profile). Hook the End to get the created HttpManager",
  "SourceLocation": "TeamServer",
  "Parameters": {
    "name": {
      "TypeName": "String",
      "PropName": null,
      "Properties": []
    },
    "connectionAddress": {
      "TypeName": "String",
      "PropName": null,
      "Properties": []
    },
    "connectionPort": {
      "TypeName": "Int32",
      "PropName": null,
      "Properties": []
    },
    "bindAddress": {
      "TypeName": "String",
      "PropName": null,
      "Properties": []
    },
    "bindPort": {
      "TypeName": "Int32",
      "PropName": null,
      "Properties": []
    },
    "isSecure": {
      "TypeName": "Boolean",
      "PropName": null,
      "Properties": []
    },
    "profile": {
      "TypeName": "C2Profile",
      "PropName": null,
      "Properties": [
        {
          "TypeName": "String",
          "PropName": "Name",
          "Properties": []
        },
        {
          "TypeName": "String",
          "PropName": "Desc",
          "Properties": []
        },
        {
          "TypeName": "List<String>",
          "PropName": "Urls",
          "Properties": []
        },
        {
          "TypeName": "List<String>",
          "PropName": "Cookies",
          "Properties": []
        },
        {
          "TypeName": "List<String>",
          "PropName": "RequestHeaders",
          "Properties": []
        },
        {
          "TypeName": "List<String>",
          "PropName": "ResponseHeaders",
          "Properties": []
        },
        {
          "TypeName": "String",
          "PropName": "UserAgent",
          "Properties": []
        }
      ]
    }
  },
  "ReturnType": {
    "HttpManager": {
      "TypeName": "HttpManager",
      "PropName": null,
      "Properties": [
        {
          "TypeName": "String",
          "PropName": "Name",
          "Properties": []
        },
        {
          "TypeName": "Int32",
          "PropName": "ConnectionPort",
          "Properties": []
        },
        {
          "TypeName": "String",
          "PropName": "ConnectionAddress",
          "Properties": []
        },
        {
          "TypeName": "String",
          "PropName": "BindAddress",
          "Properties": []
        },
        {
          "TypeName": "Int32",
          "PropName": "BindPort",
          "Properties": []
        },
        {
          "TypeName": "Boolean",
          "PropName": "Active",
          "Properties": []
        },
        {
          "TypeName": "Boolean",
          "PropName": "IsSecure",
          "Properties": []
        },
        {
          "TypeName": "String",
          "PropName": "CertificatePath",
          "Properties": []
        },
        {
          "TypeName": "String",
          "PropName": "CertificatePassword",
          "Properties": []
        },
        {
          "TypeName": "C2Profile",
          "PropName": "c2Profile",
          "Properties": [
            {
              "TypeName": "String",
              "PropName": "Name",
              "Properties": []
            },
            {
              "TypeName": "String",
              "PropName": "Desc",
              "Properties": []
            },
            {
              "TypeName": "List<String>",
              "PropName": "Urls",
              "Properties": []
            },
            {
              "TypeName": "List<String>",
              "PropName": "Cookies",
              "Properties": []
            },
            {
              "TypeName": "List<String>",
              "PropName": "RequestHeaders",
              "Properties": []
            },
            {
              "TypeName": "List<String>",
              "PropName": "ResponseHeaders",
              "Properties": []
            },
            {
              "TypeName": "String",
              "PropName": "UserAgent",
              "Properties": []
            }
          ]
        },
        {
          "TypeName": "ManagerType",
          "PropName": "Type",
          "Properties": []
        },
        {
          "TypeName": "String",
          "PropName": "Id",
          "Properties": []
        },
        {
          "TypeName": "DateTime",
          "PropName": "CreationTime",
          "Properties": []
        }
      ]
    }
  }
}
```
