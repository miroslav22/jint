// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/*---
info: Operator remove leading StrWhiteSpaceChar
es5id: 15.1.2.2_A2_T8
es6id: 18.2.5
esid: sec-parseint-string-radix
description: "StrWhiteSpaceChar :: LS (U+2028)"
---*/

//CHECK#1
if (parseInt("\u20281") !== parseInt("1")) {
  $ERROR('#1: parseInt("\\u20281") === parseInt("1"). Actual: ' + (parseInt("\u20281")));
}

//CHECK#2
if (parseInt("\u2028\u2028-1") !== parseInt("-1")) {
  $ERROR('#2: parseInt("\\u2028\\u2028-1") === parseInt("-1"). Actual: ' + (parseInt("\u2028\u2028-1")));
}

//CHECK#3
assert.sameValue(parseInt("\u2028"), NaN);
