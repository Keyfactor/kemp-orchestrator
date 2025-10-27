## Overview

TODO Overview is a required section

#### TEST CASES
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



