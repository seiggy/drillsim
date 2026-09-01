---
title: "How to use the DrillingFluid microservice?"
output: html_document
---

Typical Usage
===
1. Upload a new DrillingFluid using the `Post` web api method.
2. Call the `Get` method with the identifier of the uploaded DrillingFluid as argument. 
The return Json object contains the DrillingFluid description.
3. Optionally send a `Delete` request with the identifier of the DrillingFluid in order to delete the DrillingFluid if you do not 
want to keep the DrillingFluid uploaded on the microservice.


