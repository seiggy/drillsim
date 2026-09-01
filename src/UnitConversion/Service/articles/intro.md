---
title: "How to use the UnitConversion microservice?"
output: html_document
---

Typical Usage
===
1. Upload a new DataUnitConversionSet using the `Post` web api method.
2. Call the `Get` method with the identifier of the uploaded DataUnitConversionSet as argument. 
The return Json object contains the DataUnitConversionSet description.
3. Optionally send a `Delete` request with the identifier of the DataUnitConversionSet in order to delete the DataUnitConversionSet if you do not 
want to keep the DataUnitConversionSet uploaded on the microservice.


