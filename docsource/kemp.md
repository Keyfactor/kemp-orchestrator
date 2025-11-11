# Enabling API Access for Keyfactor on Kemp LoadMaster

This guide explains how to enable API access for a specific user on a Kemp LoadMaster appliance to allow Keyfactor Orchestrator integrations for certificate management and inventory operations.

---

## 🧩 Step-by-Step: Enabling API Access for a User

### 1. Log in to the Kemp Web UI
- In your browser, go to:
  ```
  https://<loadmaster-hostname-or-ip>:8443
  ```
- Log in as an administrator account that can manage users.

---

### 2. Edit the User Permissions
1. In the left-hand menu, navigate to:
   ```
   System Configuration → System Administration → User Management
   ```
2. Locate the user account that will be used by the Keyfactor Orchestrator (for example: `bhill`).
3. Click **Modify** next to that user to open the **Permissions for User** screen.
4. Under **Rules**, enable the following options:
   - ✅ **Certificate Creation**  
   - ✅ **Intermediate Certificates**
5. Click **Set Permissions** to apply the changes.

These permissions allow the orchestrator to create and manage intermediate and server certificates.

---

### 3. Generate and Record the API Key
1. Scroll down to the **API Keys** section.
2. Click **Generate New APIKey** to create a new key for API authentication.
3. Copy and securely store this key — it will be used in your Keyfactor orchestrator configuration as the **ServerPassword** or **API Key**.
4. You can later use **Delete** to revoke it if needed.

---

### 4. Verify API Access
Use a command line or PowerShell session to verify connectivity:

#### Using curl:
```bash
curl -k -H "Authorization: <API_KEY>" https://<loadmaster-ip>:8443/access/list
```

#### Using PowerShell:
```powershell
Invoke-RestMethod -Uri "https://<loadmaster-ip>:8443/access/list" -Headers @{ Authorization = "<API_KEY>" } -SkipCertificateCheck
```

If you receive a JSON response, API access is successfully configured.


### ✅ Summary of Required Settings

| Setting | Location | Value |
|----------|-----------|--------|
| Certificate Creation | User Permissions | Enabled |
| Intermediate Certificates | User Permissions | Enabled |
| API Key | User Management (Modify user) | Generated |
| Allow Web Administrative Access | Remote Access | Enabled |
| Enable API Interface | Remote Access | Enabled |
| Port | Remote Access | 8443 |
| Authentication Method | Remote Access | Password Only (default) |

---

## Kemp LoadMaster Orchestrator – Behavior Summary

This document summarizes the observed behaviors of the **Kemp LoadMaster Orchestrator** integration during SSL and Intermediate Certificate management operations.  
It details how the orchestrator interacts with the LoadMaster API, handles overwrite logic, manages bindings, and synchronizes data with Keyfactor Command.

---

### 🧩 Overall Integration Behavior

- The orchestrator communicates with the **Kemp LoadMaster REST API** using the configured **ServerUsername**, **API Key**, and **SSL (HTTPS)** over port 8443.  
- Operations are driven by the **Overwrite flag** and **Alias Name** supplied in the job parameters.  
- Certificates are managed in two categories:
  - **SSL Certificates** – used by virtual services (may be bound/unbound).  
  - **Intermediate Certificates** – uploaded supporting CA chain files.  
- The orchestrator validates overwrite rules, binding constraints, and synchronization with Keyfactor Command for each operation.

---

### 🧪 Test Case Behavior Summary

| # | Case Name | Behavior Summary | Outcome |
|---|------------|------------------|----------|
| **1** | **New Add New Alias SSL Certificates** | When a new alias (`TC1`) is provided and the certificate does not exist, the orchestrator successfully uploads a new SSL certificate to the LoadMaster and registers it in Keyfactor. | ✅ New certificate created successfully. |
| **2** | **Replace Alias SSL Certificates** | The orchestrator detects an existing alias (`TC1`) and, with **Overwrite=True**, replaces the existing SSL certificate. The old certificate file is overwritten. | ✅ Replacement successful. |
| **3** | **Replace Alias SSL Certificates (No Overwrite)** | Attempting to replace an existing alias without the overwrite flag causes the orchestrator to abort the operation and return an error indicating the flag is required. | ✅ Expected error: “Overwrite flag should be used.” |
| **4** | **Replace Alias Bound SSL Certificates** | When a certificate bound to a virtual service is replaced with **Overwrite=True**, the orchestrator updates the certificate file while maintaining the existing service binding. | ✅ Bound certificate replaced in place. |
| **5** | **Remove Bound SSL Certificate** | The orchestrator prevents removal of any certificate that is currently bound to a virtual service, returning an error message. | ✅ Error handled correctly (“cannot remove bound certificates”). |
| **6** | **Remove Unbound SSL Certificate** | The orchestrator removes an SSL certificate only if it is unbound, confirming removal through the LoadMaster API. | ✅ Certificate removed successfully. |
| **7** | **New Add New Alias Intermediate Certificates** | A new intermediate certificate (`TC8b`) is uploaded since no alias conflict exists. It appears under the Intermediate Certificates list. | ✅ Intermediate certificate created. |
| **8** | **Replace Alias Intermediate Certificates** | Kemp does not support overwriting intermediate certificates. The orchestrator logs and returns the expected API error (`Filename already exists`). | ✅ Expected failure recorded. |
| **9** | **Remove Intermediate Certificates** | The orchestrator deletes the intermediate certificate (`TC8b`) from the LoadMaster and synchronizes removal from Keyfactor Command. | ✅ Certificate removed successfully. |
| **10** | **Inventory Intermediate Certificates** | Performs an inventory scan of all intermediate certificates on the LoadMaster, importing them into Keyfactor Command. | ✅ Inventory successful. |
| **11** | **Inventory SSL Certificates** | Enumerates all SSL certificates (bound and unbound) on the LoadMaster and updates Keyfactor’s inventory accordingly. | ✅ Inventory successful. |

---

### ⚙️ Functional Insights

- **Overwrite Logic:** SSL certificates respect the `Overwrite` flag. Intermediate certificates cannot be overwritten.  
- **Binding Awareness:** The orchestrator checks for bound services before delete or replace operations.  
- **Error Handling:** Clear API error messages are surfaced in Keyfactor job logs.  
- **Synchronization:** Add/Remove/Inventory maintain consistent state between Keyfactor and LoadMaster.  
- **Security:** All operations occur over HTTPS using API Key authentication.

---

### ✅ Operation Coverage Summary

| Operation | Certificate Type | Supported | Notes |
|------------|------------------|------------|--------|
| Add | SSL | ✅ | Creates new alias or replaces with overwrite flag |
| Replace | SSL | ✅ | Requires `Overwrite=True` |
| Replace | Intermediate | ❌ | Unsupported – API rejects |
| Remove | SSL | ✅ | Allowed only if unbound |
| Remove | Intermediate | ✅ | Fully supported |
| Inventory | SSL | ✅ | Returns all SSL certificates |
| Inventory | Intermediate | ✅ | Returns all intermediate certificates |

---



## TEST CASES
Case Number|Case Name|Case Description|Overwrite Flag|Alias Name|Expected Results|Passed|Screenshots
------------|---------|----------------|--------------|----------|----------------|--------------|------------
1|New Add New Alias SSL Certificates|Will Create a new SSL Certificate|False|TC1|New SSL Certificate with Alias TC1 Created On Kemp LoadMaster|True|![](images/TC1Results.gif)
2|Replace Alias SSL Certificates|Will Replace SSL Certificate|True|TC1|SSL Certificate with Alias TC1 Replaced On Kemp LoadMaster|True|![](images/TC2Results.gif)
3|Replace Alias SSL Certificates no Overwrite|Will Replace SSL Certificate|False|TC1|Error should occur stating Overwrite flag should be used|True|![](images/TC3Results.gif)
4|Replace Alias Bound SSL Certificates|Will Replace Bound SSL Certificate|True|TC1|Bound Certificate should be replaced|True|![](images/TC4Results.gif)
5|Remove Bound SSL Certificate|Should fail as you cannot remove Bound Certificates|N/A|TC1|Error Occurs stating you cannot remove bound certificates.|True|![](images/TC5Results.gif)
6|Remove UnBound SSL Certificate|Try to remove SSL Certificate that is UnBound|N/A|TC8a|Unbound Certificate Is Removed from LoadMaster.|True|![](images/TC6Results.gif)
7|New Add New Alias Intermediate Certificates|Will Create a new Intermediate Certificate|False|TC8b|New Intermediate Certificate with Alias TC8b Created On Kemp LoadMaster|True|![](images/TC7Results.gif)
8|Replace Alias Intermediate Certificates|You cannot replace intermediate certificates|True|TC8b|Command Failed: Filename already exists|True|![](images/TC8Results.gif)
9|Remove Intermediate Certificates|Intermediate Certificate Will Be Removed|N/A|TC8b|Intermediate Certificate Is Removed From Keyfactor and the LoadMaster|True|![](images/TC9Results.gif)
10|Inventory Intermediate Certificates|Intermediate Certificate Will Be Inventoried|N/A|N/A|Intermediate Certificate Is Inventoried to Keyfactor|True|![](images/TC10Results.gif)
11|Inventory SSL Certificates|SSL Certificate Will Be Inventoried|N/A|N/A|SSL Certificate Is Inventoried to Keyfactor|True|![](images/TC11Results.gif)
