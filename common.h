#include <stdint.h>
#include <stdio.h>
#include "types.h"

#ifndef COMMON_H
#define COMMON_H

int printDnsRequest(BYTE*, size_t);
void printHeader(Header*);

uint16_t parseHeader(Header*, BYTE*);
void printResourceRecord(ResourceRecord*);

void serializeHeader(BYTE*, Header *);
void serializeQuestion(BYTE*, Question *);
// returns # bytes written
size_t serializeResourceRecord(BYTE*, ResourceRecord*);

void getRawQuestion(BYTE*, size_t*, BYTE*);
uint16_t parseQuestion(Question*, uint16_t, BYTE*);



#endif
