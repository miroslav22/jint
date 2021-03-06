/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch15/15.4/15.4.4/15.4.4.16/15.4.4.16-7-c-ii-1.js
 * @description Array.prototype.every - callbackfn called with correct parameters
 */


function testcase() { 
 
  function callbackfn(val, Idx, obj)
  {
    if(obj[Idx] === val)
      return true;
  }

  var arr = [0,1,2,3,4,5,6,7,8,9];
  
  if(arr.every(callbackfn) === true)
    return true;
 }
runTestCase(testcase);
