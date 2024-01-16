## HttpManagerFactoryFunc
**Context**: OnHttpManagerCreate
**Description**: Fires when a new HttpManager is created. Hook on Start to get the creation args of (name, address, port, bind address, bind port, is secure, and the C2 profile). Hook the End to get the created HttpManager
**Parameters:**
- name: String
- connectionAddress: String
- connectionPort: Int32
- bindAddress: String
- bindPort: Int32
- isSecure: Boolean
- profile: C2Profile
**Returns:** HttpManager
```json
{
  "Name": "HttpManagerFactoryFunc",
  "ContextName": "OnHttpManagerCreate",
  "Description": "Fires when a new HttpManager is created. Hook on Start to get the creation args of (name, address, port, bind address, bind port, is secure, and the C2 profile). Hook the End to get the created HttpManager",
  "SourceLocation": "TeamServer",
  "Parameters": [
    "System.String name",
    "System.String connectionAddress",
    "System.Int32 connectionPort",
    "System.String bindAddress",
    "System.Int32 bindPort",
    "System.Boolean isSecure",
    "HardHatCore.ApiModels.Shared.C2Profile profile"
  ],
  "ReturnType": "HardHatCore.TeamServer.Models.HttpManager"
}
```
