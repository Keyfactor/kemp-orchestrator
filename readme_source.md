**Kemp Load Balancer Configuration (LoadMaster)**

**Overview**

The Kemp Load Balancer Orchestrator remotely manages certificates on the Kemp Virtual LoadMaster Load Balancer Product

This agent implements three job types – Inventory, Management Add, and Management Remove. Below are the steps necessary to configure this AnyAgent.  It supports adding certificates with or without private keys.


**Kemp LoadMaster Configuration**

1. Read up on [Kemp LoadMaster Load Balancer](https://kemptechnologies.com/virtual-load-balancer) and how it works.
2. You need to setup a user with the following permissions for API Access on the Kemp Load Balancer
![](images/ApiUserSetup.gif)
3. The following Api Access is needed:
![](images/ApiAccessNeeded.gif)


**1. Create the New Certificate Store Type for the Kemp Load Balancer Orchestrator**

In Keyfactor Command create a new Certificate Store Type similar to the one below:

#### STORE TYPE CONFIGURATION
SETTING TAB  |  CONFIG ELEMENT	| DESCRIPTION
------|-----------|------------------
Basic |Name	|Descriptive name for the Store Type.  Kemp Load Balancer can be used.
Basic |Short Name	|The short name that identifies the registered functionality of the orchestrator. Must be Kemp
Basic |Custom Capability|Unchecked
Basic |Job Types	|Inventory, Add, and Remove are the supported job types. 
Basic |Needs Server	|Must be checked
Basic |Blueprint Allowed	|Must be checked
Basic |Requires Store Password	|Determines if a store password is required when configuring an individual store.  This must be unchecked.
Basic |Supports Entry Password	|Determined if an individual entry within a store can have a password.  This must be unchecked.
Advanced |Store Path Type| Determines how the user will enter the store path when setting up the cert store.  Freeform
Advanced |Supports Custom Alias	|Determines if an individual entry within a store can have a custom Alias.  This must be Required
Advanced |Private Key Handling |Determines how the orchestrator deals with private keys.  Optional
Advanced |PFX Password Style |Determines password style for the PFX Password. Default
Custom Fields|N/A| There are no Custom Fields
Entry Parameters|N/A| There are no Entry Parameters

**Basic Settings:**

![](images/CertStoreType-Basic.gif)

**Advanced Settings:**

![](images/CertStoreType-Advanced.gif)

**Custom Fields:**

![](images/CertStoreType-CustomFields.gif)

**Entry Params:**

![](images/CertStoreType-EntryParameters.gif)

**2. Register the Kemp Load Balancer Orchestrator with Keyfactor**
See Keyfactor InstallingKeyfactorOrchestrators.pdf Documentation.  Get from your Keyfactor contact/representative.

**3. Create a Kemp Load Balancer Store within Keyfactor Command**
In Keyfactor Command create a new Certificate Store similar to the one below

![](images/CertStoreSettings-1.gif)
![](images/CertStoreSettings-2.gif)

#### STORE CONFIGURATION 
CONFIG ELEMENT	|DESCRIPTION
----------------|---------------
Category	|The type of certificate store to be configured. Select category based on the display name configured above "Kemp Load Balancer".
Container	|This is a logical grouping of like stores. This configuration is optional and does not impact the functionality of the store.
Client Machine	|Server and port of the kemp load balancer sample is 20.62.33:8443.
Store Path	|Not used just put a "/".
Orchestrator	|This is the orchestrator server registered with the appropriate capabilities to manage this certificate store type. 
Inventory Schedule	|The interval that the system will use to report on what certificates are currently in the store. 
Use SSL	|This should be checked.
User	|This is not necessary.
Password |This is the Kemp Load Balancer API Key setup for the user created in Kemp described in the "LoadMaster Configuration Section".

*** 


#### TEST CASES
Case Number|Case Name|Case Description|Overwrite Flag|Alias Name|Expected Results|Passed
------------|---------|----------------|--------------|----------|----------------|--------------
1|Fresh Add with New Map and Entry|Will create new map, map entry and cert|False|map12/mentry12/cert12|New Map will be created, New Map Entry Created, New Cert Created|True
1a|Try Replace without Overwrite|If user does not use overwrite flag, should error out on same entry replace|False|map12/mentry12/cert12|Error Occurs Saying to Use Overwrite Flag|True
1b|Try Replace with Overwrite|Should  delete and re-insert mapentry and certificate|True|map12/mentry12/cert12|Replaced Cert Map Entry and Certificate|True
2|Fresh Add with Cert Only (No Map)|Will create cert that is not tied to map|False|cert40|Created Certificate with alias cert40|True
2a|Try Replace without Overwrite|If user does not use overwrite flag, should error out on same entry replace|False|Cert40|Error Occurs Saying to Use Overwrite Flag|True
2b|Try Replace with Overwrite|If user uses overwrite will replace cert|True|cert40|Certificate with be replaced with alias of cert40|True
3|Fresh Add with new entry to existing map|Will create cert where entry is tied to an existing map|False|map12/mentry50/cert50|Created Certificate with alias map12/mentry50/cert50|True
3a|Try Replace without Overwrite|If user does not use overwrite flag, should error out on same entry replace|False|map12/mentry50/cert50|Error Occurs Saying to Use Overwrite Flag|True
4|Remove Cert In Map|Try to remove cert in existing map.  Should leave map and delete cert map entry and cert.|N/A|map12/mentry50/cert50|Cert cert50 and map entry mentry50 should be deleted.|True
4a|Remove Standalone cert (No Map)|Try to remove cert without a map entry or map.|N/A|cert40|Cert cert40 should be deleted.|True

