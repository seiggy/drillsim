---
title: "How to use the GeothermalProperties microservice?"
output: html_document
---

Typical Usage
===
1. Upload a new GeothermalPropertiesCompletionOrder using the `Post` web api method.
2. Call the `Get` method with the identifier of the uploaded GeothermalPropertiesCompletionOrder as argument. 
The return Json object contains the GeothermalPropertiesCompletionOrder description.
3. Optionally send a `Delete` request with the identifier of the GeothermalPropertiesCompletionOrder in order to delete the GeothermalPropertiesCompletionOrder if you do not 
want to keep the GeothermalPropertiesCompletionOrder uploaded on the microservice.


