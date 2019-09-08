#include <stdio.h>
#include "common.h"

#ifndef UTIL_H
#define UTIL_H

void toNetworkOrder(Header*);
void tobinstr(BYTE*, size_t, char*);
void printBinStr(BYTE*, size_t);
void printHexStr(BYTE*, size_t);

#endif