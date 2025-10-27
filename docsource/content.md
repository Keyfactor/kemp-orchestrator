## Overview

The Kemp Load Balancer Universal Orchestrator extension enables remote management of cryptographic certificates on Kemp Load Balancers. Kemp Load Balancers use certificates to secure HTTP and HTTPS traffic efficiently, ensuring that sensitive data is encrypted during transit. This extension integrates with Keyfactor Command to automate the process of inventorying, adding, and removing certificates within Kemp Load Balancer environments. By leveraging this orchestrator, administrators can easily manage SSL/TLS certificates, ensuring the security and reliability of their load balancing infrastructure.

## Enabling API Access on Kemp LoadMaster

This guide describes how to enable and verify API access on a Kemp LoadMaster device for integration with Keyfactor Orchestrator.

---

### 🧩 Step-by-Step: Enabling API Access

#### 1. Log in to the Kemp Web UI
- Open a browser and go to your LoadMaster’s management interface:
  ```
  https://<loadmaster-hostname-or-ip>:8443
  ```
- Log in with an administrative account.

---

#### 2. Configure User Permissions and Generate an API Key
1. Navigate to:
   ```
   System Configuration → User Management
   ```
2. Under **Rules**, check the box for **Intermediate Certificates**.
   - This permission allows the orchestrator to upload or replace certificates.
3. Scroll to the **API Keys** section:
   - Click **Generate New APIKey**.
   - Copy and securely store the API key for later use.
4. (Optional) Under **Local Certificate**:
   - Click **Generate** to create a self-signed admin certificate.
   - Click **Download Certificate** if the orchestrator requires importing it.

---

#### 3. Enable API and Administrative Access
1. Go to:
   ```
   System Configuration → Remote Access
   ```
2. Under **Administrator Access**, enable the following settings:
   - ✅ **Allow Web Administrative Access**
     - **Using:** `eth0` (or the management interface)
     - **Port:** `8443`
   - ✅ **Enable API Interface**
     - **Port:** `8443`
   - ✅ **Allow Multi Interface Access** (optional, if the orchestrator connects from another subnet)
   - **Authentication Method:** `Password Only Access (default)`
   - (Optional) **Enable Software FIPS mode** if required by compliance policies.
3. Click **Set Administrative Access** (if shown), then **Save Changes**.

---

#### 4. Verify API Access
Use a command line or PowerShell session to confirm connectivity:

#### 5. Configure in Keyfactor Orchestrator
When setting up your Kemp Orchestrator Store Type, provide the following values:

| Field | Value |
|-------|--------|
| **ServerUsername** | LoadMaster admin username |
| **ServerPassword** | API Key generated earlier |
| **ServerUseSsl** | true |
| **ServerHostname** | e.g., `testkemp:8443` |

---

#### ✅ Summary of Required Settings

| Setting | Location | Value |
|----------|-----------|--------|
| Allow Web Administrative Access | System Configuration → Remote Access | Enabled |
| Enable API Interface | System Configuration → Remote Access | Enabled |
| API Port | System Configuration → Remote Access | 8443 |
| Intermediate Certificates Rule | System Configuration → User Management | Enabled |
| API Key | System Configuration → User Management | Generated |
| Authentication Method | System Configuration → Remote Access | Password Only (default) |

---


### TEST CASES
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
11|Inventory SSL Certificates|SS: Certificate Will Be Inventoried|N/A|N/A|SSL Certificate Is Inventoried to Keyfactor|True|![](images/TC11Results.gif)
