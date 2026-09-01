---
title: "How to use the CartographicProjection microservice?"
output: html_document
---

Typical Usage
===
1. Upload a new CartographicConversionSet using the `Post` web api method.
2. Call the `Get` method with the identifier of the uploaded CartographicConversionSet as argument. 
The return Json object contains the CartographicConversionSet description.
3. Optionally send a `Delete` request with the identifier of the CartographicConversionSet in order to delete the CartographicConversionSet if you do not 
want to keep the CartographicConversionSet uploaded on the microservice.


